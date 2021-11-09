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
    }
    class SemanticVisitor
    {
        public HashSet<string> TableVariables
        {
            get;
            set;
        }
        public IDictionary<string, FunctionRecord> TableFunctions
        {
            get;
            set;
        }

        public SemanticVisitor()
        {
            TableVariables = new HashSet<string>();
            TableFunctions = new SortedDictionary<string, FunctionRecord>();

            TableFunctions.Add("printi", new FunctionRecord("printi", true, 1, null));
            TableFunctions.Add("printc", new FunctionRecord("printc", true, 1, null));
            TableFunctions.Add("prints", new FunctionRecord("prints", true, 1, null));
            TableFunctions.Add("println", new FunctionRecord("println", true, 0, null));
            TableFunctions.Add("readi", new FunctionRecord("readi", true, 0, null));
            TableFunctions.Add("reads", new FunctionRecord("reads", true, 0, null));
            TableFunctions.Add("new", new FunctionRecord("new", true, 1, null));
            TableFunctions.Add("size", new FunctionRecord("size", true, 1, null));
            TableFunctions.Add("add", new FunctionRecord("add", true, 2, null));
            TableFunctions.Add("get", new FunctionRecord("get", true, 2, null));
            TableFunctions.Add("set", new FunctionRecord("set", true, 3, null));
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
                Visit((dynamic) child);
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
            var arity = node[0].ChildrenLength;
            if (TableFunctions.ContainsKey(functionName))
            {
                throw new SemanticError("Duplicated Function: " + functionName, node.AnchorToken);
            }
            else
            {
                TableFunctions.Add(functionName, new FunctionRecord(functionName, false, arity, new HashSet<string>()));
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

        public IDictionary<string, FunctionRecord> TableFunctions
        {
            get;
            set;
        }
        //-----------------------------------------------------------

        int depth = 0;
        //-----------------------------------------------------------

        public SecondSemanticVisitor(HashSet<string> tableVariables, IDictionary<string, FunctionRecord> tableFunctions)
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

        public void Visit(FunCall node)
        {
            var functionName = node.AnchorToken.Lexeme;
            var arity = node.ChildrenLength;
            if (TableFunctions.ContainsKey(functionName))
            {
                if (TableFunctions[functionName].arity != arity) {
                    throw new SemanticError("Wrong number of arguments: " + functionName, node.AnchorToken);
                }
            }
            else
            {
                throw new SemanticError("Function not recognized: " + functionName, node.AnchorToken);
            }
        }

        /* TODO:

        ASSIGNMENT
        PARAM_LIST
        VAR_DEF_LIST ? 


         */


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