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

<id-list>               -> <id> <id-list-cont>

<id-list-cont>          -> ("," <id>)*

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

<expr-list>             -> (<expr> <expr-list-cont>)?

<expr-list-cont>        -> ("," <expr>)*

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
	
<expr-unary>            -> <op-unary>* <expr-primary>
	
<op-unary>              -> "+"|"-"|"!"
	
<expr-primary>          -> <id>|<fun-call>|<array>|<lit>| "(" <expr> ")"
	
<array>                 -> "[" <expr-list> "]"

<lit>                   -> <lit-bool>|<lit-int>|<lit-char>|<lit-str>

	

*/


/*
 * Falak LL(1) Grammar:

Program             ::= ‹def-list›* EOF                                                 NOT SURE
‹def-list›          ::= ‹def›*                                                          CHANGE HAPPENED HERE
‹def›               ::= ‹var-def› | ‹fun-def›
‹var-def›           ::= "var" ‹id-list› ";"                                             NOT SURE IF VAR-LIST IS NECESSARY
‹id-list›           ::= ‹id› ("," ‹id›)*                                                NOT SURE IF ID-LIST-CONT IS NECESSARY
‹fun-def›           ::= ‹id› "(" ‹param-list› ")" "{" ‹var-def-list› ‹stmt-list› "}"    DECOMPOSED
‹param-list›        ::= ‹id-list›?                                                      DECOMPOSED
‹var-def-list›      ::= ‹var-def›*                                                      CHANGE HAPPENED HERE
‹stmt-list›         ::= ‹stmt›*                                                         CHANGE HAPPENED HERE
‹stmt›              ::= ‹stmt-assign› | ‹stmt-incr› | ‹stmt-decr›  | 
                        ‹stmt-fun-call› | ‹stmt-if› | ‹stmt-while› | 
                        ‹stmt-do-while› | ‹stmt-break› | ‹stmt-return› | ‹stmt-empty›
‹stmt-assign›       ::= ‹id› "=" ‹expr› ";"
‹stmt-fun-call›     ::= ‹fun-call›
‹fun-call›          ::= ‹id› "(" ‹expr-list› ")"                                        DECOMPOSED
‹stmt-incr›         ::= "inc" ‹id› ";"
‹stmt-decr›         ::= "dec" ‹id› ";"
‹expr-list›         ::= ‹expr› ("," ‹expr›)*                                            NOT SURE
‹stmt-if›           ::= "if" "(" ‹expr› ")" "{" ‹stmt-list› "}" ‹else-if-list› ‹else›   CHANGE HAPPENED HERE
‹else-if-list›      ::= ("elseif" "(" ‹expr› ")" "{" ‹stmt-list› "}")*
‹else›              ::= ("else" "{" ‹stmt-list› "}")?                                   CHANGE HAPPENED HERE
‹stmt-while›        ::= "while" "(" ‹expr› ")" "{" ‹stmt-list› "}"
‹stmt-do-while›     ::= "do" "{" ‹stmt-list› "}" "while" "(" ‹expr› ")" ";"
‹stmt-break›        ::= "break" ";"
‹stmt-return›       ::= "return" ‹expr› ";"
‹stmt-empty›        ::= ";"
‹expr›              ::= ‹expr-or›
‹expr-or›           ::= ‹expr-and› (‹op-or› ‹expr-and›)*
‹op-or›             ::= "||" | "^"
‹expr-and›          ::= ‹expr-comp› ("&&" ‹expr-comp›)*
‹expr-comp›         ::= ‹expr-rel› (‹op-comp› ‹expr-rel›)*
‹op-comp›           ::= "==" | "!="
‹expr-rel›          ::= ‹expr-add› (‹op-rel› ‹expr-add›)*
‹op-rel›            ::= "<" | "<=" | ">" | ">="
‹expr-add›          ::= ‹expr-mul› (‹op-add› ‹expr-mul›)*
‹op-add›            ::= "+" | "−"
‹expr-mul›          ::= ‹expr-unary›  (‹op-mul› ‹expr-unary›)*             
‹op-mul›            ::= "*" | "/" | "%"
‹expr-unary›        ::= (‹op-unary› ‹expr-primary›)*                             CHANGE HAPPENED HERE
‹op-unary›          ::= "+" | "−" | "!"
‹expr-primary›      ::= ‹id› | ‹fun-call› | ‹array› | ‹lit› | "(" ‹expr› ")"
‹array›             ::= "[" ‹expr-list› "]"
‹lit›               ::= ‹lit-bool› | ‹lit-int› | ‹lit-char› | ‹lit-str›

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
            Console.WriteLine("Expecting " + category);
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
            // while (CurrentToken == TokenCategory.COMMA)
            // {
            //     Expect(TokenCategory.COMMA);
            //     Expect(TokenCategory.IDENTIFIER);
            // }
            // Expect(TokenCategory.END);
            IdListCont();
        }


        public void IdListCont()
        {
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
            if (CurrentToken == TokenCategory.ELSE_IF)
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

        public void StatementDoWhile()
        {
            Expect(TokenCategory.DO);
            Expect(TokenCategory.CURL_LEFT);
            StatementList();
            Expect(TokenCategory.CURL_RIGHT);
            Expect(TokenCategory.WHILE);
            Expect(TokenCategory.PAR_LEFT);
            Expression();
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.END);
        }

        public void StatementBreak()
        {
            Expect(TokenCategory.BREAK);
            Expect(TokenCategory.END);
        }

        public void StatementReturn()
        {
            Expect(TokenCategory.RETURN);
            Expression();
            Expect(TokenCategory.END);
        }

        public void StatementEmpty()
        {
            Expect(TokenCategory.END);
        }

        public void ExprList()
        {
            Expression();
            while (TokenCategory.COMMA == CurrentToken)
            {
                Expect(TokenCategory.COMMA);
                Expression();
            }
        }

        public void Expression()
        {
            ExprOr();
        }

        public void ExprOr()
        {
            ExprAnd();
            while (firstOfOrOperators.Contains(CurrentToken))
            {
                OpOr();
                ExprAnd();
            }
        }

        public void OpOr()
        {
            switch (CurrentToken)
            {
                case TokenCategory.OR:
                    Expect(TokenCategory.OR);
                    break;
                case TokenCategory.BIT_OR:
                    Expect(TokenCategory.BIT_OR);
                    break;
                default:
                    throw new SyntaxError(firstOfStatement,
                                          tokenStream.Current);
            }
        }

        public void ExprAnd()
        {
            ExprComp();
            while (CurrentToken == TokenCategory.AND)
            {
                Expect(TokenCategory.AND);
                ExprComp();
            }
        }

        public void ExprComp()
        {
            ExprRel();
            while (firstOfComparisonsOperator.Contains(CurrentToken))
            {
                OpComp();
                ExprRel();
            }
        }

        public void OpComp()
        {
            switch (CurrentToken)
            {
                case TokenCategory.EQUALS:
                    Expect(TokenCategory.EQUALS);
                    break;
                case TokenCategory.NOT_EQUALS:
                    Expect(TokenCategory.NOT_EQUALS);
                    break;
                default:
                    throw new SyntaxError(firstOfStatement,
                                          tokenStream.Current);
            }
        }

        public void ExprRel()
        {
            ExprAdd();
            while (firstOfRelationalOperator.Contains(CurrentToken))
            {
                OpRel();
                ExprAdd();
            }
        }

        public void OpRel()
        {
            switch (CurrentToken)
            {
                case TokenCategory.LESS:
                    Expect(TokenCategory.LESS);
                    break;
                case TokenCategory.LESS_EQUALS:
                    Expect(TokenCategory.LESS_EQUALS);
                    break;
                case TokenCategory.MORE:
                    Expect(TokenCategory.MORE);
                    break;
                case TokenCategory.MORE_EQUALS:
                    Expect(TokenCategory.MORE_EQUALS);
                    break;
                default:
                    throw new SyntaxError(firstOfStatement,
                                          tokenStream.Current);
            }
        }

        public void ExprAdd()
        {
            ExprMul();
            while (firstOfAdditionOperator.Contains(CurrentToken))
            {
                OpAdd();
                ExprMul();
            }
        }

        public void OpAdd()
        {
            switch (CurrentToken)
            {
                case TokenCategory.PLUS:
                    Expect(TokenCategory.PLUS);
                    break;
                case TokenCategory.NEG:
                    Expect(TokenCategory.NEG);
                    break;
                default:
                    throw new SyntaxError(firstOfStatement,
                                          tokenStream.Current);
            }
        }

        public void ExprMul()
        {
            ExprUnary();
            while (firstOfMultiplicationOperator.Contains(CurrentToken))
            {
                OpMul();
                ExprUnary();
            }
        }

        public void OpMul()
        {
            switch (CurrentToken)
            {
                case TokenCategory.MUL:
                    Expect(TokenCategory.MUL);
                    break;
                case TokenCategory.DIV:
                    Expect(TokenCategory.DIV);
                    break;
                case TokenCategory.MOD:
                    Expect(TokenCategory.MOD);
                    break;
                default:
                    throw new SyntaxError(firstOfStatement,
                                          tokenStream.Current);
            }
        }

        public void ExprUnary()
        {
            while (firstOfExprUnary.Contains(CurrentToken))
            {
                if (firstOfPrimaryExpression.Contains(CurrentToken))
                {
                    ExprPrimary();
                }
                else if (firstOfUnaryOperator.Contains(CurrentToken))
                {
                    OpUnary();
                }
                else
                {
                    throw new SyntaxError(firstOfExprUnary,
                                              tokenStream.Current);
                }
            }
        }

        public void OpUnary()
        {
            switch (CurrentToken)
            {   
                case TokenCategory.PLUS:
                    Expect(TokenCategory.PLUS);
                    break;
                case TokenCategory.NEG:
                    Expect(TokenCategory.NEG);
                    break;
                case TokenCategory.NOT:
                    Expect(TokenCategory.NOT);
                    break;
                default:
                    throw new SyntaxError(firstOfUnaryOperator,
                                          tokenStream.Current);
            }
        }

        public void ExprPrimary()
        {
            if (firstOfLiteral.Contains(CurrentToken))
            {
                Lit();
            }
            else
            {
                switch (CurrentToken)
                {
                    case TokenCategory.IDENTIFIER:
                        Expect(TokenCategory.IDENTIFIER);
                        if (CurrentToken == TokenCategory.PAR_LEFT)
                        {
                            FunCall();
                        }
                        break;
                    case TokenCategory.ARRAY_LEFT:
                        Array();
                        break;
                    case TokenCategory.PAR_LEFT:
                        Expect(TokenCategory.PAR_LEFT);
                        Expression();
                        Expect(TokenCategory.PAR_RIGHT);
                        break;
                    default:
                        throw new SyntaxError(firstOfUnaryOperator,
                                              tokenStream.Current);
                }
            }
        }

        public void Array()
        {
            Expect(TokenCategory.ARRAY_LEFT);
            ExprList();
            Expect(TokenCategory.ARRAY_RIGHT);
        }

        public void Lit()
        {
            switch (CurrentToken)
            {
                case TokenCategory.BOOL:
                    Expect(TokenCategory.BOOL);
                    break;
                case TokenCategory.FALSE:
                    Expect(TokenCategory.FALSE);
                    break;
                case TokenCategory.TRUE:
                    Expect(TokenCategory.TRUE);
                    break;
                case TokenCategory.INT_LITERAL:
                    Expect(TokenCategory.INT_LITERAL);
                    break;
                case TokenCategory.CHARACTER:
                    Expect(TokenCategory.CHARACTER);
                    break;
                case TokenCategory.STRING:
                    Expect(TokenCategory.STRING);
                    break;
                default:
                    throw new SyntaxError(firstOfUnaryOperator,
                                          tokenStream.Current);
            }
        }


    }
}