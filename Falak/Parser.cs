/*
  Falak compiler - This class performs the syntax analysis,
  (a.k.a. Parsing).
  Copyright (C) 2013-2021 Ariel Ortiz, ITESM CEM

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

  Modified by team 6
  Javier Pascal Flores          A01375925
  Diego Palmerin Bonada         A01747290
  Hector Ivan Aguirre Arteaga   A01169628

/*
<program>               -> <def-list> EOF

<def-list>              -> <def>*

<def>                   -> <var-def>|<fun-def>

<var-def>               -> "var" <var-list> ";"

<var-list>              -> <id-list>

<id-list>               -> <id> ("," <id>)*

<fun-def>               -> <id> "(" <param-list> ")" "{" <var-def-list> <stmt-list> "}"

<param-list>            -> <id-list>?

<var-def-list>          -> <var-def>*

<stmt-list>             -> <stmt>*

<stmt>                  -> <stmt-assign>|<stmt-inc>|

					        <stmt-dec>|<stmt-fun-call>|

					        <stmt-if>|<stmt-while>|

					        <stmt-do-while>|<stmt-break>|

					        <stmt-return>|<stmt-empty>

<stmt-assign>           -> <id> "=" <expr> ";"

<stmt-incr>             -> "inc" <id> ";"

<stmt-decr>             -> "dec" <id> ";"

<stmt-fun-call>         -> <fun-call> ";"

<fun-call>              -> <id> "(" <expr-list> ")"

<expr-list>             -> <expr>? ("," <expr>)*

<stmt-if>               -> "if" "(" <expr> ")" "{" <stmt-list> "}" <else-if-list> <else>

<else-if-list>          -> ("elseif" "(" <expr> ")" "{" <stmt-list> "}")*

<else>                  -> ("else" "{" <stmt-list> "}")?

<stmt-while>            -> "while" "(" <expr> ")" "{" <stmt-list> "}"

<stmt-do-while>         -> "do" "{" <stmt-list> "}" "while" "(" <expr> ")" ";"

<stmt-break>            -> "break" ";"

<stmt-return>           -> "return" <expr> ";"

<stmt-empty>            -> ";"

<expr>                  -> <expr-or>

<expr-or>               -><expr-and> (<op-or> <expr-and>)*
	
<op-or>                 -> "||" | "^"
	
<expr-and>              -> <expr-comp> (&& <expr-comp>)*
	
<expr-comp>             -> <expr-rel> (<op-comp> <expr-rel>)*
	
<op-comp>               -> "=="|"!="
	
<expr-rel>	            -> <expr-add> (<op-rel> <exrp-add>)*
	
<op-rel>                -> "<" | "<="|">"|">="
	
<expr-add>              -> <expr-mul> (<op-add> <expr-mul>)*

<op-add>                -> "+"|"-"
	
<expr-mul>              -> <expr-unary> (<op-mul> <expr-unary>)*
	
<op-mul>                -> "*"|"/"|"%"
	
<expr-unary>            -> <op-unary> <expr-unary> | <expr-primary>
	
<op-unary>              -> "+"|"-"|"!"
	
<expr-primary>          -> <id>|<fun-call>|<array>|<lit>| "(" <expr> ")"
	
<array>                 -> "[" <expr-list> "]"

<lit>                   -> <lit-bool>|<lit-int>|<lit-char>|<lit-str>

*/


using System;
using System.Collections.Generic;

namespace Falak
{
    class Parser
    {
        static readonly ISet<TokenCategory> firstOfDeclaration =
           new HashSet<TokenCategory>() {
                TokenCategory.IDENTIFIER,
                TokenCategory.VAR
           };

        static readonly ISet<TokenCategory> firstOfStatement =
            new HashSet<TokenCategory>() {
                TokenCategory.BREAK,
                TokenCategory.DEC,
                TokenCategory.DO,
                TokenCategory.IDENTIFIER,
                TokenCategory.IF,
                TokenCategory.INC,
                TokenCategory.RETURN,
                TokenCategory.END,
                TokenCategory.WHILE
            };

        static readonly ISet<TokenCategory> firstOfComparisonsOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.EQUALS,
                TokenCategory.NOT_EQUALS
                };

        static readonly ISet<TokenCategory> firstOfRelationalOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.MORE,
                TokenCategory.MORE_EQUALS,
                TokenCategory.LESS,
                TokenCategory.LESS_EQUALS
            };

        static readonly ISet<TokenCategory> firstOfExprUnary =
            new HashSet<TokenCategory>() {
                TokenCategory.IDENTIFIER,
                TokenCategory.INT_LITERAL,
                TokenCategory.CHARACTER,
                TokenCategory.STRING,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
                TokenCategory.PAR_LEFT,
                TokenCategory.CURL_LEFT,
                TokenCategory.ARRAY_LEFT,
                TokenCategory.PLUS,
                TokenCategory.NEG,
                TokenCategory.NOT
        };
        static readonly ISet<TokenCategory> firstOfAdditionOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.PLUS,
                TokenCategory.NEG
                };

        static readonly ISet<TokenCategory> firstOfOrOperators =
            new HashSet<TokenCategory>() {
                TokenCategory.OR,
                TokenCategory.BIT_OR
                };
        static readonly ISet<TokenCategory> firstOfUnaryOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.PLUS,
                TokenCategory.NEG,
                TokenCategory.NOT
            };
        static readonly ISet<TokenCategory> firstOfMultiplicationOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.MUL,
                TokenCategory.DIV,
                TokenCategory.MOD
                };
        static readonly ISet<TokenCategory> firstOfPrimaryExpression =
            new HashSet<TokenCategory>() {
                TokenCategory.IDENTIFIER,
                TokenCategory.INT_LITERAL,
                TokenCategory.CHARACTER,
                TokenCategory.STRING,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
                TokenCategory.PAR_LEFT,
                TokenCategory.CURL_LEFT,
                TokenCategory.ARRAY_LEFT,
            };

        static readonly ISet<TokenCategory> firstOfLiteral =
            new HashSet<TokenCategory>() {
                TokenCategory.BOOL,
                TokenCategory.CHARACTER,
                TokenCategory.FALSE,
                TokenCategory.INT_LITERAL,
                TokenCategory.STRING,
                TokenCategory.TRUE,
            };


        IEnumerator<Token> tokenStream;

        public Parser(IEnumerator<Token> tokenStream)
        {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext();
        }

        public TokenCategory CurrentToken
        {
            get { return tokenStream.Current.Category; }
        }

        public Token Expect(TokenCategory category)
        {
            // Console.WriteLine("Expecting " + category);
            if (CurrentToken == category)
            {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return current;
            }
            else
            {
                throw new SyntaxError(category, tokenStream.Current);
            }
        }

        public void Program()
        {
            DefinitionList();

            StatementList();

            Expect(TokenCategory.EOF);
        }

        public void DefinitionList()
        {
            while (firstOfDeclaration.Contains(CurrentToken))
            {
                Definition();
            }
        }
        public void Definition()
        {
            switch (CurrentToken)
            {
                case TokenCategory.IDENTIFIER:
                    FunDef();
                    break;

                case TokenCategory.VAR:
                    VarDef();
                    break;

                default:
                    throw new SyntaxError(firstOfDeclaration,
                                          tokenStream.Current);
            }
        }

        public void VarDef()
        {
            Expect(TokenCategory.VAR);
            VarList();
            Expect(TokenCategory.END);
        }

        public void VarList()
        {
            IdList();
        }
        public void IdList()
        {
            Expect(TokenCategory.IDENTIFIER);
            while (CurrentToken == TokenCategory.COMMA)
            {
                Expect(TokenCategory.COMMA);
                Expect(TokenCategory.IDENTIFIER);
            }
        }


        public void FunDef()
        {
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.PAR_LEFT);
            ParamList();
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURL_LEFT);
            VarDefList();
            StatementList();
            Expect(TokenCategory.CURL_RIGHT);
        }

        public void ParamList()
        {
            if (CurrentToken == TokenCategory.IDENTIFIER)
            {
                IdList();
            }
        }

        public void VarDefList()
        {
            while (CurrentToken == TokenCategory.VAR)
            {
                VarDef();
            }
        }

        public void StatementList()
        {
            while (firstOfStatement.Contains(CurrentToken))
            {
                Statement();
            }
        }

        public void Statement()
        {
            switch (CurrentToken)
            {
                case TokenCategory.IDENTIFIER:
                    StatementStartId();
                    break;

                case TokenCategory.INC:
                    StatementInc();
                    break;

                case TokenCategory.DEC:
                    StatementDec();
                    break;

                case TokenCategory.IF:
                    StatementIf();
                    break;

                case TokenCategory.WHILE:
                    StatementWhile();
                    break;

                case TokenCategory.DO:
                    StatementDoWhile();
                    break;

                case TokenCategory.BREAK:
                    StatementBreak();
                    break;

                case TokenCategory.RETURN:
                    StatementReturn();
                    break;

                case TokenCategory.END:
                    StatementEmpty();
                    break;

                default:
                    throw new SyntaxError(firstOfStatement,
                                          tokenStream.Current);
            }
        }

        public void StatementStartId()
        {
            Expect(TokenCategory.IDENTIFIER);
            switch (CurrentToken)
            {
                case TokenCategory.ASSIGN:
                    StatementAssign();
                    break;

                case TokenCategory.PAR_LEFT:
                    StatementFunCall();
                    break;
                default:
                    throw new SyntaxError(firstOfStatement,
                                          tokenStream.Current);
            }
        }

        public void StatementAssign()
        {
            Expect(TokenCategory.ASSIGN);
            Expression();
            Expect(TokenCategory.END);
        }

        public void StatementInc()
        {
            Expect(TokenCategory.INC);
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.END);
        }

        public void StatementDec()
        {
            Expect(TokenCategory.DEC);
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.END);
        }

        public void StatementFunCall()
        {
            FunCall();
        }

        public void FunCall()
        {
            Expect(TokenCategory.PAR_LEFT);
            ExprList();
            Expect(TokenCategory.PAR_RIGHT);
        }

        public void StatementIf()
        {
            Expect(TokenCategory.IF);
            Expect(TokenCategory.PAR_LEFT);
            Expression();
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURL_LEFT);
            StatementList();
            Expect(TokenCategory.CURL_RIGHT);
            ElseIfList();
            Else();
        }

        public void ElseIfList()
        {
            while (CurrentToken == TokenCategory.ELSE_IF)
            {
                Expect(TokenCategory.ELSE_IF);
                Expect(TokenCategory.PAR_LEFT);
                Expression();
                Expect(TokenCategory.PAR_RIGHT);
                Expect(TokenCategory.CURL_LEFT);
                StatementList();
                Expect(TokenCategory.CURL_RIGHT);
            }
        }

        public void Else()
        {
            if (CurrentToken == TokenCategory.ELSE)
            {
                Expect(TokenCategory.ELSE);
                Expect(TokenCategory.CURL_LEFT);
                StatementList();
                Expect(TokenCategory.CURL_RIGHT);
            }
        }

        public void StatementWhile()
        {
            Expect(TokenCategory.WHILE);
            // Expect(TokenCategory.ELSE_IF);
            Expect(TokenCategory.PAR_LEFT);
            Expression();
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURL_LEFT);
            StatementList();
            Expect(TokenCategory.CURL_RIGHT);
        }

        public Node StatementDoWhile()
        {
            var doToken = Expect(TokenCategory.DO);
            Expect(TokenCategory.CURL_LEFT);
            var stmtList = new StatementList();
            while (firstOfStatement.Contains(CurrentToken))
            {
                stmtList.Add(Statement());  
            }

            Expect(TokenCategory.CURL_RIGHT);
            Expect(TokenCategory.WHILE);
            Expect(TokenCategory.PAR_LEFT);

            var condition = Expression();
            
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.END);

            var result = new Do(){condition, stmtList};
            result.AnchorToken = doToken;

            return result;
        }

        public Node StatementBreak()
        {
            var result = new Break() { AnchorToken = Expect(TokenCategory.BREAK) };
            Expect(TokenCategory.END);
            return result;
        }

        public Node StatementReturn()
        {
            var result = new Return() { AnchorToken = Expect(TokenCategory.RETURN) };
            var expr = Expression();
            result.Add(expr);
            Expect(TokenCategory.END);
            return result;
        }

        public Node StatementEmpty()
        {
            return new Empty() { AnchorToken = Expect(TokenCategory.END) };
        }

        // TODO EXPR LIST
        public Node ExprList()
        {
            try
            {
                Expression();
            }
            catch (SyntaxError)
            {
                return;
            }
            while (TokenCategory.COMMA == CurrentToken)
            {
                Expect(TokenCategory.COMMA);
                Expression();
            }
        }

        public Node Expression()
        {
            return ExprOr();
        }

        public Node ExprOr()
        {
            var firstExpr = ExprAnd();
            while (firstOfOrOperators.Contains(CurrentToken))
            {
                var secondExpr = OpOr();
                secondExpr.Add(firstExpr);
                secondExpr.Add(ExprAnd());
                firstExpr = secondExpr;
            }
            return firstExpr;
        }

        public Node OpOr()
        {
            switch (CurrentToken)
            {
                case TokenCategory.OR:
                    return new Or()
                    {
                        AnchorToken = Expect(TokenCategory.OR)
                    };
                case TokenCategory.BIT_OR:
                    return new BitOr()
                    {
                        AnchorToken = Expect(TokenCategory.BIT_OR)
                    };
                default:
                    throw new SyntaxError(firstOfOrOperators,
                                          tokenStream.Current);
            }
        }

        public Node ExprAnd()
        {
            var firstExpr = ExprComp();
            while (CurrentToken == TokenCategory.AND)
            {
                var secondExpr = new And() { AnchorToken = Expect(TokenCategory.AND) };
                secondExpr.Add(firstExpr);
                secondExpr.Add(ExprComp());
                firstExpr = secondExpr;
            }
            return firstExpr;
        }

        public Node ExprComp()
        {
            var firstExpr = ExprRel();

            while (firstOfComparisonsOperator.Contains(CurrentToken))
            {
                var secondExpr = OpComp();
                secondExpr.Add(firstExpr);
                secondExpr.Add(ExprRel());
                firstExpr = secondExpr;

            }
            return firstExpr;
        }

        public Node OpComp()
        {
            switch (CurrentToken)
            {
                case TokenCategory.EQUALS:
                    return new Equals()
                    {
                        AnchorToken = Expect(TokenCategory.EQUALS)
                    };
                case TokenCategory.NOT_EQUALS:
                    return new NotEquals()
                    {
                        AnchorToken = Expect(TokenCategory.NOT_EQUALS)
                    };
                default:
                    throw new SyntaxError(firstOfComparisonsOperator,
                                          tokenStream.Current);
            }
        }

        public Node ExprRel()
        {
            var firstExpr = ExprAdd();
            while (firstOfRelationalOperator.Contains(CurrentToken))
            {
                var secondExpr = OpRel();
                secondExpr.Add(firstExpr);
                secondExpr.Add(ExprAdd());
                firstExpr = secondExpr;
            }
            return firstExpr;
        }

        public Node OpRel()
        {
            switch (CurrentToken)
            {
                case TokenCategory.LESS:
                    return new Less()
                    {
                        AnchorToken = Expect(TokenCategory.LESS)
                    };
                case TokenCategory.LESS_EQUALS:
                    return new LessEqual()
                    {
                        AnchorToken = Expect(TokenCategory.LESS_EQUALS)
                    };
                case TokenCategory.MORE:
                    return new More()
                    {
                        AnchorToken = Expect(TokenCategory.MORE)
                    };
                case TokenCategory.MORE_EQUALS:
                    return new MoreEqual()
                    {
                        AnchorToken = Expect(TokenCategory.MORE_EQUALS)
                    };
                default:
                    throw new SyntaxError(firstOfRelationalOperator,
                                          tokenStream.Current);
            }
        }

        public Node ExprAdd()
        {
            var firstExpr = ExprMul();
            while (firstOfAdditionOperator.Contains(CurrentToken))
            {
                var secondExpr = OpAdd();
                secondExpr.Add(firstExpr);
                secondExpr.Add(ExprMul());
                firstExpr = secondExpr;

            }
            return firstExpr;
        }

        public Node OpAdd()
        {
            switch (CurrentToken)
            {
                case TokenCategory.PLUS:
                    return new Plus()
                    {
                        AnchorToken = Expect(TokenCategory.PLUS)
                    };
                case TokenCategory.NEG:
                    return new Neg()
                    {
                        AnchorToken = Expect(TokenCategory.NEG)
                    };
                default:
                    throw new SyntaxError(firstOfAdditionOperator,
                                          tokenStream.Current);
            }
        }

        public Node ExprMul()
        {
            var firstExpr = ExprUnary();
            while (firstOfMultiplicationOperator.Contains(CurrentToken))
            {
                var secondExpr = OpMul();
                secondExpr.Add(firstExpr);
                secondExpr.Add(ExprUnary());
                firstExpr = secondExpr;
            }
            return firstExpr;
        }

        public Node OpMul()
        {
            switch (CurrentToken)
            {
                case TokenCategory.MUL:
                    return new Mul()
                    {
                        AnchorToken = Expect(TokenCategory.MUL)
                    };
                case TokenCategory.DIV:
                    return new Div()
                    {
                        AnchorToken = Expect(TokenCategory.DIV)
                    };
                    break;
                case TokenCategory.MOD:
                    return new Remainder()
                    {
                        AnchorToken = Expect(TokenCategory.MOD)
                    };
                default:
                    throw new SyntaxError(firstOfMultiplicationOperator,
                                          tokenStream.Current);
            }
        }

        public Node ExprUnary()
        {
            // TODO
            List<Node> unaryOperators = new List<Node>();

            if (firstOfUnaryOperator.Contains(CurrentToken))
            {
                unaryOperators.add(OpUnary());
                ExprUnary();
            }
            else if (firstOfPrimaryExpression.Contains(CurrentToken))
            {
                ExprPrimary();
            }
            else
            {
                throw new SyntaxError(firstOfExprUnary,
                                          tokenStream.Current);
            }
        }

        public Node OpUnary()
        {
            switch (CurrentToken)
            {
                case TokenCategory.PLUS:
                    return new Plus()
                    {
                        AnchorToken = Expect(TokenCategory.PLUS)
                    };
                case TokenCategory.NEG:
                    return new Neg()
                    {
                        AnchorToken = Expect(TokenCategory.NEG)
                    };

                case TokenCategory.NOT:
                    return new Not()
                    {
                        AnchorToken = Expect(TokenCategory.NOT)
                    };
                default:
                    throw new SyntaxError(firstOfUnaryOperator,
                                          tokenStream.Current);
            }
        }

        public Node ExprPrimary()
        {
            var result = new Node();


            if (firstOfLiteral.Contains(CurrentToken))
            {
                return Lit();
            }
            else
            {
                switch (CurrentToken)
                {
                    case TokenCategory.IDENTIFIER:
                        var idToken = Expect(TokenCategory.IDENTIFIER);
                        if (CurrentToken == TokenCategory.PAR_LEFT)
                        {
                            result = FunCall();
                            result.AnchorToken = idToken;
                            return result;
                        }
                        result = new Identifier() { AnchorToken = idToken };
                        return result;
                    case TokenCategory.ARRAY_LEFT:
                        return Array();
                    case TokenCategory.PAR_LEFT:
                        Expect(TokenCategory.PAR_LEFT);
                        result = Expression();
                        Expect(TokenCategory.PAR_RIGHT);
                        return result;
                    default:
                        throw new SyntaxError(firstOfPrimaryExpression,
                                              tokenStream.Current);
                }
            }
        }

        public Node Array()
        {
            var result = new Array();
            Expect(TokenCategory.ARRAY_LEFT);
            result.add(ExprList());
            Expect(TokenCategory.ARRAY_RIGHT);
            return result;
        }

        public Node Lit()
        {
            switch (CurrentToken)
            {
                case TokenCategory.BOOL:
                    return new Boolean()
                    {
                        AnchorToken = Expect(TokenCategoty.BOOL)
                    };
                case TokenCategory.FALSE:
                    return new False()
                    {
                        AnchorToken = Expect(TokenCategory.FALSE)
                    };
                case TokenCategory.TRUE:
                    return new True()
                    {
                        AnchorToken = Expect(TokenCategory.TRUE)
                    };
                case TokenCategory.INT_LITERAL:
                    return new IntLiteral()
                    {
                        AnchorToken = Expect(TokenCategory.INT_LITERAL)
                    };
                case TokenCategory.CHARACTER:
                    return new Character()
                    {
                        AnchorToken = Expect(TokenCategory.CHARACTER)
                    };
                case TokenCategory.STRING:
                    return new String()
                    {
                        AnchorToken = Expect(TokenCategory.STRING)
                    };
                default:
                    throw new SyntaxError(firstOfLiteral,
                                          tokenStream.Current);
            }
        }


    }
}