/*
  Falak compiler - Specific Node subclasses for the AST (Abstract Syntax Tree).
  
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

namespace Falak {
    class And: Node {}
    class Array: Node {}
    class Assignment: Node {}
    class BitOr: Node{}
    class Break: Node {}
    class Character: Node {}
    class Dec: Node {}
    class DeclarationList: Node {}
    class Div: Node {}
    class Do: Node {}
    class Else: Node {}
    class Expression: Node {}
     class ExprList: Node {}
    class ElseIf: Node {}
    class ElseIfList: Node {}
    class Equals: Node {}
    class Or: Node {}    
    class False: Node {}
    class FunCall: Node {}
    class Function: Node {}
    class Identifier: Node {}
    class If: Node {}
    class Inc: Node {}
    class IntLiteral: Node {}
    class IdList: Node {}
    class Less: Node {}
    class LessEqual: Node {}
    class More: Node {}
    class MoreEqual: Node {}
    class Mul: Node {}
    class Minus: Node {}
    class Neg: Node {}
    class Not: Node {}
    class NotEquals: Node {}
    class ParamList: Node {}
    class Plus: Node {}
    class Program: Node {}
    class Remainder: Node {}
    class Return: Node {}
    class Empty: Node {}
    class StatementList: Node {}
    class String: Node {}
    class True: Node {}
    class VarDef: Node {}
    class VarDefList: Node {}
    class While: Node {}

}