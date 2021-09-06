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
*/

namespace Falak
{

    enum TokenCategory
    {
        AND,
        ARRAY,
        ASSIGN,
        BOOL,
        BIT_OR,
        BREAK,
        COMMENT,
        CODE_GROUP,
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
        NEW_LINE,
        NOT_EQUALS,
        OR,
        OTHER,
        PARENTHESIS_OPEN,// SOFT GROUP? OR INDIVIDUAL CHAR?
        PARENTHESIS_CLOSE,// SAME AS ABOVE
        PLUS,
        PRINT,
        RETURN,
        STRING,
        THEN,
        TRUE,
        VAR,
        WHILE,
        ILLEGAL_CHAR,
    }
}
