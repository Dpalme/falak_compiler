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
        static readonly ISet<TokenCategory> firstOfAfterIdentifier =
           new HashSet<TokenCategory>() {
                TokenCategory.PAR_LEFT,
                TokenCategory.ASSIGN
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



        public Node Program()
        {
            var program = new Program() {
                DefinitionList()
            };
            Expect(TokenCategory.EOF);
            return program;
        }


        public Node DefinitionList()
        {
            var declList = new DeclarationList();

            while (firstOfDeclaration.Contains(CurrentToken))
            {
                declList.Add(Definition());
            }
            return declList;
        }


        public Node Definition()
        {
            switch (CurrentToken)
            {
                case TokenCategory.IDENTIFIER:
                    return FunDef();

                case TokenCategory.VAR:
                    return VarDef();

                default:
                    throw new SyntaxError(firstOfDeclaration,
                                          tokenStream.Current);
            }
        }


        public Node VarDef()
        {
            if (CurrentToken == TokenCategory.VAR)
            {
                Expect(TokenCategory.VAR);
            }
            else
            {
                return new VarDef();
            }
            var result = VarList();
            Expect(TokenCategory.END);
            return result;
        }



        public Node VarList()
        {
            Node varList = new VarDef();
            foreach (Node node in IdList())
            {
                varList.Add(node);
            }
            return varList;
        }


        public List<Node> IdList()
        {
            var idList = new List<Node>();
            if (CurrentToken == TokenCategory.IDENTIFIER)
            {
                idList.Add(
                    new Identifier()
                    {
                        AnchorToken = Expect(TokenCategory.IDENTIFIER)
                    }
                );
            }
            else
            {
                return idList;
            }
            while (CurrentToken == TokenCategory.COMMA)
            {
                Expect(TokenCategory.COMMA);
                idList.Add(
                    new Identifier()
                    {
                        AnchorToken = Expect(TokenCategory.IDENTIFIER)
                    }
                );
            }
            return idList;
        }


        public Node FunDef()
        {
            var functionToken = Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.PAR_LEFT);
            var paramList = new ParamList();
            foreach (Node id in IdList())
            {
                paramList.Add(id);
            };
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURL_LEFT);
            var varDefList = VarDef();
            Node stmtList = StmtList();
            var function = new Function() { paramList, varDefList, stmtList };
            function.AnchorToken = functionToken;
            Expect(TokenCategory.CURL_RIGHT);
            return function;
        }

        public Node StmtList()
        {
            var stmtList = new StatementList();
            while (firstOfStatement.Contains(CurrentToken))
            {
                stmtList.Add(Statement());
            }
            return stmtList;
        }

        public Node Statement()
        {
            switch (CurrentToken)
            {
                case TokenCategory.IDENTIFIER:
                    return StatementStartId();

                case TokenCategory.INC:
                    return StatementInc();

                case TokenCategory.DEC:
                    return StatementDec();

                case TokenCategory.IF:
                    return StatementIf();

                case TokenCategory.WHILE:
                    return StatementWhile();

                case TokenCategory.DO:
                    return StatementDoWhile();

                case TokenCategory.BREAK:
                    return StatementBreak();

                case TokenCategory.RETURN:
                    return StatementReturn();

                case TokenCategory.END:
                    return StatementEmpty();

                default:
                    throw new SyntaxError(firstOfStatement,
                                          tokenStream.Current);
            }
        }

        public Node StatementStartId()
        {
            switch (CurrentToken)
            {
                case TokenCategory.IDENTIFIER:
                    var idToken = Expect(TokenCategory.IDENTIFIER);
                    switch (CurrentToken)
                    {
                        case TokenCategory.ASSIGN:
                            return StatementAssign(idToken);

                        case TokenCategory.PAR_LEFT:
                            return StatementFunCall(idToken);
                        default:
                            throw new SyntaxError(firstOfAfterIdentifier,
                                                  tokenStream.Current);
                    }
                default:
                    throw new SyntaxError(firstOfAfterIdentifier,
                                          tokenStream.Current);
            }
        }

        public Node StatementAssign(Token idToken)
        {
            var assignToken = Expect(TokenCategory.ASSIGN);
            var expr = Expression();
            var result = new Assignment() { AnchorToken = assignToken };
            result.Add(new Identifier() { AnchorToken = idToken });
            result.Add(expr);
            Expect(TokenCategory.END);
            return result;
        }

        public Node StatementInc()
        {
            var result = new Inc() { AnchorToken = Expect(TokenCategory.INC) };
            result.Add(new Identifier() { AnchorToken = Expect(TokenCategory.IDENTIFIER) });
            Expect(TokenCategory.END);
            return result;
        }

        public Node StatementDec()
        {
            var result = new Dec() { AnchorToken = Expect(TokenCategory.DEC) };
            result.Add(new Identifier() { AnchorToken = Expect(TokenCategory.IDENTIFIER) });
            Expect(TokenCategory.END);
            return result;
        }

        public Node StatementFunCall(Token idToken)
        {
            var result = FunCall();
            Expect(TokenCategory.END);
            result.AnchorToken = idToken;
            return result;
        }

        public Node FunCall()
        {
            var result = new FunCall();
            Expect(TokenCategory.PAR_LEFT);
            foreach (Node node in ExprList())
            {
                result.Add(node);
            }

            Expect(TokenCategory.PAR_RIGHT);
            return result;
        }

        public Node StatementIf()
        {
            var ifToken = Expect(TokenCategory.IF);
            Expect(TokenCategory.PAR_LEFT);
            var expr = Expression();
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURL_LEFT);
            var stmtList = new StatementList();
            while (firstOfStatement.Contains(CurrentToken))
            {
                stmtList.Add(Statement());
            }
            Expect(TokenCategory.CURL_RIGHT);
            var elseifStmt = ElseIfList();
            var elseStmt = Else();
            var stmtIf = new If() { expr, stmtList, elseifStmt, elseStmt };
            stmtIf.AnchorToken = ifToken;
            return stmtIf;
        }

        public Node ElseIfList()
        {
            var result = new ElseIfList();
            while (CurrentToken == TokenCategory.ELSE_IF)
            {
                var ifToken = Expect(TokenCategory.ELSE_IF);
                Expect(TokenCategory.PAR_LEFT);
                var expr = Expression();
                Expect(TokenCategory.PAR_RIGHT);
                Expect(TokenCategory.CURL_LEFT);
                var stmtList = StmtList();
                while (firstOfStatement.Contains(CurrentToken))
                {
                    stmtList.Add(Statement());
                }
                Expect(TokenCategory.CURL_RIGHT);
                var elseIf = new ElseIf() { expr, stmtList };
                elseIf.AnchorToken = ifToken;
                result.Add(elseIf);
            }
            return result;
        }

        public Node Else()
        {
            var result = new Else();
            if (CurrentToken == TokenCategory.ELSE)
            {
                var elseToken = Expect(TokenCategory.ELSE);
                result.AnchorToken = elseToken;
                Expect(TokenCategory.CURL_LEFT);
                var stmtList = new StatementList();
                while (firstOfStatement.Contains(CurrentToken))
                {
                    stmtList.Add(Statement());
                }
                result.Add(stmtList);
                Expect(TokenCategory.CURL_RIGHT);
            }
            return result;
        }

        public Node StatementWhile()
        {
            var result = new While() { AnchorToken = Expect(TokenCategory.WHILE) };

            Expect(TokenCategory.PAR_LEFT);
            var expr = Expression();
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURL_LEFT);
            var stmtList = new StatementList();
            while (firstOfStatement.Contains(CurrentToken))
            {
                stmtList.Add(Statement());
            }
            Expect(TokenCategory.CURL_RIGHT);
            result.Add(expr);
            result.Add(stmtList);

            return result;
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

            var result = new Do() { condition, stmtList };
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
            return new Empty()
            {
                AnchorToken = Expect(TokenCategory.END)
            };
        }

        public List<Node> ExprList()
        {
            List<Node> exprList = new List<Node>();
            if (firstOfExprUnary.Contains(CurrentToken) || firstOfPrimaryExpression.Contains(CurrentToken) || firstOfUnaryOperator.Contains(CurrentToken))
            {
                exprList.Add(Expression());
                while (TokenCategory.COMMA == CurrentToken)
                {
                    Expect(TokenCategory.COMMA);
                    exprList.Add(Expression());
                }
            }
            return exprList;
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
                    return new Minus()
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
            Node firstExpr = ExprUnary();
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
            if (firstOfUnaryOperator.Contains(CurrentToken))
            {
                var unaryOperator = OpUnary();
                var expr = ExprUnary();
                unaryOperator.Add(expr);
                return unaryOperator;
            }
            else if (firstOfPrimaryExpression.Contains(CurrentToken))
            {
                return ExprPrimary();
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
            foreach (Node node in ExprList())
            {
                result.Add(node);
            }
            Expect(TokenCategory.ARRAY_RIGHT);
            return result;
        }

        public Node Lit()
        {
            switch (CurrentToken)
            {
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