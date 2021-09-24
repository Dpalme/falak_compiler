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
‹stmt-incr›         ::= "inc" ‹id› ";"
‹stmt-decr›         ::= "dec" ‹id› ";"
‹stmt-fun-call›     ::= ‹fun-call›
‹fun-call›          ::= ‹id› "(" ‹expr-list› ")"                                        DECOMPOSED
‹expr-list›         ::= (‹expr› ‹expr-list-cont›)*                                      NOT SURE
‹expr-list-cont›    ::= ("," ‹expr› ‹expr-list-cont›)*                                  NOT SURE
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
‹expr-unary›        ::= ‹op-unary›* ‹expr-primary›                              CHANGE HAPPENED HERE
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

        static readonly ISet<TokenCategory> firstOfStmt =
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

        static readonly ISet<TokenCategory> firstOfAfterId =
            new HashSet<TokenCategory>() {
                TokenCategory.PARENTHESIS_OPEN,
                TokenCategory.ASSIGN
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
        static readonly ISet<TokenCategory> firstOfAdditionOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.PLUS,
                TokenCategory.NEG
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
                };
        static readonly ISet<TokenCategory> firstOfPrimaryExpression =
            new HashSet<TokenCategory>() {
                TokenCategory.IDENTIFIER,
                TokenCategory.INT_LITERAL,
                TokenCategory.CHARACTER,
                TokenCategory.STRING,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
                TokenCategory.PARENTHESIS_OPEN,
                TokenCategory.CURL_LEFT
            };

        static readonly ISet<TokenCategory> firstOfExpression =
            new HashSet<TokenCategory>() {

                TokenCategory.CHARACTER,
                TokenCategory.CURL_LEFT,
                TokenCategory.FALSE,
                TokenCategory.IDENTIFIER,
                TokenCategory.INT_LITERAL,
                TokenCategory.NEG,
                TokenCategory.NOT,
                TokenCategory.PLUS,
                TokenCategory.PARENTHESIS_OPEN,
                TokenCategory.TRUE,
                TokenCategory.STRING
            };
        static readonly ISet<TokenCategory> firstOfLiteral =
            new HashSet<TokenCategory>() {
                TokenCategory.CHAR_LITERAL,
                TokenCategory.FALSE,
                TokenCategory.INT_LITERAL,
                TokenCategory.STRING_LITERAL,
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

        }

        public void DefinitionList()
        {

        }
        public void Definition()
        {

        }

        public void VarDef()
        {

        }

        public void IdList()
        {

        }

        public void FunDef()
        {

        }

        public void ParamList()
        {

        }

        public void VarDefList()
        {

        }

        public void StmtList()
        {

        }

        public void Statement()
        {

        }

        public void StmtAssign()
        {

        }

        public void StmtInc()
        {

        }

        public void StmtDec(){

        }

        public void StmtFunCall(){

        }

        public void FunCall(){

        }

        public void ExprList(){

        }

        public void ExprListCont(){

        }

        public void StmtIf(){

        }

        public void ElseIfList(){

        }

        public void Else(){

        }

        public void StmtWhile(){

        }

        public void StmtDoWhile(){

        }

        public void StmtBreak(){

        }

        public void StmtReturn(){

        }

        public void StmtEmpty(){

        }

        public void Expression(){

        }

        public void ExprOr(){

        }

        public void OpOr(){

        }

        public void ExprAnd(){

        }

        public void ExprComp(){

        }

        public void OpComp(){

        }

        public void ExprRel(){

        }

        public void OpRel(){

        }

        public void ExprAdd(){

        }

        public void ExprMul(){

        }

        public void OpMul(){

        }

        public void ExprUnary(){

        }

        public void OpUnary(){

        }

        public void ExprPrimary(){

        }

        public void Array(){

        }

        public void Lit(){

        }


    }
}