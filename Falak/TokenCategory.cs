/*
  Buttercup compiler - Token categories for the scanner.
  Copyright (C) 2013 Ariel Ortiz, ITESM CEM

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

namespace Falak
{

    enum TokenCategory
    {
        AND,
        ARRAY_LEFT,
        ASSIGN,
        BIT_OR,
        BREAK,
        CHARACTER,
        DEC,
        DO,
        END,
        EOF,
        ELSE,
        ELSE_IF,
        EQUALS,
        FALSE,
        IDENTIFIER,
        IF,
        INT,
        INT_LITERAL,
        INC,
        LESS,
        LESS_EQUALS,
        MUL,
        MORE,
        MORE_EQUALS,
        NEG,
        NOT,
        NOT_EQUALS,
        OR,
        PAR_RIGHT,
        PAR_LEFT,
        CURL_LEFT,
        CURL_RIGHT,
        PLUS,
        RETURN,
        STRING,
        TRUE,
        VAR,
        WHILE,
        COMMA,
        DIV,
        MOD,
         OTHER,
         COMMENT,
         NEW_LINE,
         WHITESPACE,
         THEN,
         PRINT,
        ARRAY_RIGHT
    }
}
