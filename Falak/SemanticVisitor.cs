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
using System.Text;
using System.Collections.Generic;

namespace Falak
{
    class FunctionRegister
    {
        public string name;
        public Boolean isPrimitive;
        public int arity;
        public HashSet<string> value;

        public FunctionRegister(string name, Boolean isPrimitive, int arity, HashSet<string> value)
        {
            this.name = name;
            this.isPrimitive = isPrimitive;
            this.arity = arity;
            this.value = value;
        }

        override public string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{this.name}: {this.isPrimitive}, {this.arity}, [");
            foreach (var variable in this.value)
            {
                sb.Append($"{variable}, ");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
    class SemanticVisitor
    {
        public HashSet<string> TableVariables
        {
            get;
            set;
        }
        public IDictionary<string, FunctionRegister> TableFunctions
        {
            get;
            set;
        }

        public SemanticVisitor()
        {
            TableVariables = new HashSet<string>();
            TableFunctions = new SortedDictionary<string, FunctionRegister>();

            TableFunctions.Add("printi", new FunctionRegister("printi", true, 1, null));
            TableFunctions.Add("printc", new FunctionRegister("printc", true, 1, null));
            TableFunctions.Add("prints", new FunctionRegister("prints", true, 1, null));
            TableFunctions.Add("println", new FunctionRegister("println", true, 0, null));
            TableFunctions.Add("readi", new FunctionRegister("readi", true, 0, null));
            TableFunctions.Add("reads", new FunctionRegister("reads", true, 0, null));
            TableFunctions.Add("new", new FunctionRegister("new", true, 1, null));
            TableFunctions.Add("size", new FunctionRegister("size", true, 1, null));
            TableFunctions.Add("add", new FunctionRegister("add", true, 2, null));
            TableFunctions.Add("get", new FunctionRegister("get", true, 2, null));
            TableFunctions.Add("set", new FunctionRegister("set", true, 3, null));
        }
        //-----------------------------------------------------------

        public void Visit(Program node)
        {
            Visit((dynamic)node[0]);
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
        public void Visit(DeclarationList node)
        {
            VisitChildren(node);
            if (!TableFunctions.ContainsKey("main"))
            {
                throw new SemanticError("No main function.");

            }
        }
        //-----------------------------------------------------------


        public void Visit(VarDef node)
        {
            var identifiers = node[0];
            foreach (var identifier in identifiers)
            {
                var variableName = identifier.AnchorToken.Lexeme;
                if (TableVariables.Contains(variableName))
                {
                    throw new SemanticError(
                        "Duplicated variable: " + variableName, identifier.AnchorToken
                    );
                }
                else
                {
                    TableVariables.Add(variableName);
                }
            }
        }

        public void Visit(Function node)
        {
            var functionName = node.AnchorToken.Lexeme;
            var arity = 0;
            if (node[0].ChildrenLength > 0)
            {
                arity = node[0][0].ChildrenLength;
            }
            if (TableFunctions.ContainsKey(functionName))
            {
                throw new SemanticError("Duplicated Function: " + functionName, node.AnchorToken);

            }
            else
            {
                TableFunctions.Add(functionName, new FunctionRegister(functionName, false, arity, new HashSet<string>()));
            }
        }
    }

    //-------------------------SECOND SEMANTIC VISITOR---------------------------------------------

    class SecondSemanticVisitor
    {

        public HashSet<string> TableVariables
        {
            get;
            set;
        }
        //-----------------------------------------------------------

        public IDictionary<string, FunctionRegister> TableFunctions
        {
            get;
            set;
        }
        //-----------------------------------------------------------

        int depth = 0;
        string inFunction = null;
        //-----------------------------------------------------------

        public SecondSemanticVisitor(HashSet<string> tableVariables, IDictionary<string, FunctionRegister> tableFunctions)
        {
            this.TableVariables = tableVariables;
            this.TableFunctions = tableFunctions;
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
        }
        //-----------------------------------------------------------

        public void Visit(Assignment node)
        {
            if (TableVariables.Contains(node.AnchorToken.Lexeme))
            {
                throw new SemanticError("Undeclared variable: " + node.AnchorToken.Lexeme, node.AnchorToken);
            }
            VisitChildren(node);
        }

        public void Visit(FunCall node)
        {
            var functionName = node.AnchorToken.Lexeme;
            if (TableFunctions.ContainsKey(functionName))
            {
                var arity = node.ChildrenLength;
                var expectedArity = TableFunctions[functionName].arity;
                if (arity == expectedArity)
                {

                }
                else
                {
                    throw new SemanticError(functionName + " expects " + expectedArity + " argument(s), recieved " + arity, node.AnchorToken);
                }
            }
            else
            {
                throw new SemanticError("Undeclared Function: " + functionName, node.AnchorToken);
            }
            VisitChildren(node);
        }


        public void Visit(VarDefList node)
        {
            var localTable = TableFunctions[inFunction].value;
            foreach (var identifier in node)
            {
                var varName = identifier.AnchorToken.Lexeme;
                if (localTable.Contains(varName))
                {
                    throw new SemanticError("Double Variable: " + varName, identifier.AnchorToken);
                }
                else
                {
                    localTable.Add(varName);
                }
            }
        }

        public void Visit(ParamList node)
        {
            var localTable = TableFunctions[inFunction].value;
            if (node.ChildrenLength > 0)
            {
                foreach (var param in node[0])
                {
                    var paramName = param.AnchorToken.Lexeme;
                    if (localTable.Contains(paramName))
                    {
                        throw new SemanticError("Doble Variable: " + paramName + param.AnchorToken);
                    }
                    else
                    {
                        localTable.Add(paramName);
                    }
                }
            }
        }

        public void Visit(Function node)
        {
            var functionName = node.AnchorToken.Lexeme;
            inFunction = functionName;
            VisitChildren(node);
            inFunction = null;
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
            if (depth == 0)
            {
                throw new SemanticError(
                    "Break cannot be used outside a loop.", node.AnchorToken
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

            if ((node[1] is IntLiteral) && value == 0)
            {
                throw new SemanticError(
                    "Cannot divide by 0", node.AnchorToken
                );
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