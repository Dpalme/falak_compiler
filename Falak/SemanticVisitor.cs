/*
  Falak compiler - Semantic Analyzer.
  
  Authors:
  Javier Pascal Flores          A01375925
  Diego Palmerin Bonada         A01747290
  Hector Ivan Aguirre Arteaga   A01169628

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
  
  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;

namespace Falak
{
    class FunctionRecord
    {
        public string name;
        public Boolean isPrimitive;
        public int arity;
        public HashSet<string> value;

        public FunctionRecord(string name, Boolean isPrimitive, int arity, HashSet<string> value)
        {
            this.name = name;
            this.isPrimitive = isPrimitive;
            this.arity = arity;
            this.value = value;
        }

        override public string ToString()
        {
            return $"{name}, {isPrimitive}, {arity}, {value}";
        }
    }

    class VariableRecord
    {
        public string name;
        public int depth;
        public string scope;

        public VariableRecord(string name, int depth, string scope)
        {
            this.name = name;
            this.depth = depth;
            this.scope = scope;
        }

        override public string ToString()
        {
            return $"{name}: @{scope}({depth})";
        }
    }

    class SemanticVisitor
    {

        public IDictionary<string, VariableRecord> VariablesTable
        {
            get;
            set;
        }

        public IDictionary<string, FunctionRecord> FunctionsTable
        {
            get;
            set;
        }
        //-----------------------------------------------------------

        int depth;
        string scope;
        //-----------------------------------------------------------
        public SemanticVisitor()
        {
            FunctionsTable = new SortedDictionary<string, FunctionRecord>();
            VariablesTable = new SortedDictionary<string, VariableRecord>();

            FunctionsTable.Add("printi", new FunctionRecord("printi", true, 1, null));
            FunctionsTable.Add("printc", new FunctionRecord("printc", true, 1, null));
            FunctionsTable.Add("prints", new FunctionRecord("prints", true, 1, null));
            FunctionsTable.Add("println", new FunctionRecord("println", true, 0, null));
            FunctionsTable.Add("readi", new FunctionRecord("readi", true, 0, null));
            FunctionsTable.Add("reads", new FunctionRecord("reads", true, 0, null));
            FunctionsTable.Add("new", new FunctionRecord("new", true, 1, null));
            FunctionsTable.Add("size", new FunctionRecord("size", true, 1, null));
            FunctionsTable.Add("add", new FunctionRecord("add", true, 2, null));
            FunctionsTable.Add("get", new FunctionRecord("get", true, 2, null));
            FunctionsTable.Add("set", new FunctionRecord("set", true, 3, null));
        }
        //-----------------------------------------------------------
        void VisitChildren(Node node)
        {
            foreach (var child in node)
            {
                Visit((dynamic)child);
            }
        }
        //-----------------------------------------------------------
        public void Visit(Node node)
        {
            VisitChildren(node);
        }

        public void Visit(Program node)
        {
            Visit((dynamic)node[0]);
            if (!FunctionsTable.ContainsKey("main"))
            {
                throw new SemanticError("No main function.");
            }
        }
        //-----------------------------------------------------------
        public void Visit(FunCall node)
        {
            var functionName = node.AnchorToken.Lexeme;
            var arity = node.ChildrenLength;
            if (FunctionsTable.ContainsKey(functionName))
            {
                if (FunctionsTable[functionName].arity != arity)
                {
                    throw new SemanticError(functionName + " expected " + FunctionsTable[functionName].arity + "arguments, but found " + arity, node.AnchorToken);
                }
            }
            else
            {
                throw new SemanticError("Function " + functionName + " not recognized", node.AnchorToken);
            }
            VisitChildren(node);
        }
        //-----------------------------------------------------------
        public void Visit(Function node)
        {
            var functionName = node.AnchorToken.Lexeme;
            var arity = node[0].ChildrenLength;
            if (FunctionsTable.ContainsKey(functionName))
            {
                throw new SemanticError("The function " + functionName + " has already been declared", node.AnchorToken);
            }
            else
            {
                FunctionsTable.Add(functionName, new FunctionRecord(functionName, false, arity, new HashSet<string>()));
            }
            scope = functionName;
            depth++;
            VisitChildren(node);
            scope = "";
            depth--;
        }
        //-----------------------------------------------------------
        public void Visit(VarDef node)
        {
            foreach (var identifier in node)
            {
                var variableName = identifier.AnchorToken.Lexeme;
                if (VariablesTable.ContainsKey(variableName + "@" + scope))
                {
                    foreach (var entry in VariablesTable)
                    {
                        Console.WriteLine(entry);
                    }
                    Console.WriteLine(scope);
                    throw new SemanticError(
                        "The variable " + variableName + " has already been declared", identifier.AnchorToken
                    );
                }
                else
                {
                    VariablesTable.Add(variableName + "@" + scope, new VariableRecord(variableName, depth, scope));
                }
            }
        }

        public void Visit(ParamList node)
        {
            foreach (var identifier in node)
            {
                VariablesTable.Add(identifier.AnchorToken.Lexeme + "@" + scope, new VariableRecord(identifier.AnchorToken.Lexeme, depth, scope));
            }
        }

        public void Visit(Assignment node)
        {
            var varName = node[0].AnchorToken.Lexeme;
            if (!VariablesTable.ContainsKey(varName + "@" + scope) && !VariablesTable.ContainsKey(varName + "@"))
            {
                foreach (var variable in VariablesTable)
                {
                    System.Console.WriteLine(variable);
                }
                throw new SemanticError("Undeclared variable: " + varName, node.AnchorToken);
            }
        }


        public void Visit(StatementList node)
        {
            VisitChildren(node);
        }
        // ---------------------------------------------------------

        public void Visit(DeclarationList node)
        {
            VisitChildren(node);
        }
        public void Visit(Identifier node)
        {
            if (node is Identifier && (!VariablesTable.ContainsKey(node.AnchorToken.Lexeme + "@" + scope) && !VariablesTable.ContainsKey(node.AnchorToken.Lexeme + "@")))
            {
                throw new SemanticError("unrecognized identifier: " + node.AnchorToken.Lexeme, node.AnchorToken);
            }
        }
        // ---------------------------------------------------------
        public void Visit(IntLiteral node)
        {
            try
            {
                Int32.Parse(node.AnchorToken.Lexeme);
            }
            catch
            {
                throw new SemanticError(
                    "Int out of bounds (32 bits)", node.AnchorToken
                );
            }
        }
        // ---------------------------------------------------------

        public void Visit(While node)
        {
            depth += 1;
            VisitChildren(node);
            depth -= 1;
        }
        // ---------------------------------------------------------

        public void Visit(Do node)
        {
            depth += 1;
            VisitChildren(node);
            depth -= 1;
        }
        // ---------------------------------------------------------

        public void Visit(Break node)
        {
            if (depth < 2)
            {
                throw new SemanticError(
                    "the break keyword is reserved for while loops.", node.AnchorToken
                );
            }
        }
        // ---------------------------------------------------------
        public void Visit(Div node)
        {
            var intDenominator = node[1].AnchorToken.Lexeme;
            int value = 0;

            if ((node[1] is IntLiteral) && !Int32.TryParse(intDenominator, out value))
            {
                throw new SemanticError(
                    $"Integer literal too large: {intDenominator}",
                    node.AnchorToken);
            }

            VisitChildren(node);
        }

        //-----------------------------------------------------------
        public void Visit(Remainder node)
        {
            var intDenominator = node[1].AnchorToken.Lexeme;
            int value = 0;

            if ((node[1] is IntLiteral) && !Int32.TryParse(intDenominator, out value))
            {
                throw new SemanticError(
                    $"Integer literal too large: {intDenominator}",
                    node.AnchorToken);
            }

            if ((node[1] is IntLiteral) && value == 0)
            {
                throw new SemanticError(
                    "Cannot divide by 0", node.AnchorToken
                );
            }
            VisitChildren(node);
        }



    }



}