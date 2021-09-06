/*
  Falak compiler - This class performs the lexical analysis,
  (a.k.a. scanning).
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
*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Falak {

    class Scanner {

        readonly string input;

        static readonly Regex regex = new Regex(
            @"
                (?<Comment>    (?:#.*)|(<#.*?#>)        )
              | (?<SoftGroup>  [(].*?[)]                ) # NOT CATEGORIZED YET
              | (?<CodeGroup>  [{].*?[}]                ) # NOT CATEGORIZED YET
              | (?<Array>      \[.*?\]                  )
              | (?<Newline>    \n                       )
              | (?<WhiteSpace> \s                       ) # Must go after Newline.
              | (?<And>        [&][&]                   )
              | (?<Or>         [|][|]                   )
              | (?<BitOr>      [^]                      )
              | (?<LessEquals> [<][=]                   )
              | (?<MoreEquals> [>][=]                   )
              | (?<Less>       [<]                      )
              | (?<More>       [>]                      )
              | (?<Plus>       [+]                      )
              | (?<Neg>        [-][!]                   )
              | (?<Mul>        [*]|[/][%]               )
              | (?<Equals>     [=][=]                   )
              | (?<NotEquals>  [!][=]                   )
              | (?<Assign>     [=]                      )
              | (?<IntLiteral> \d+                      )
              
              # KeyWords (12)

              | (?<Break>      break\b                  )
              | (?<Dec>        dec\b                    )
              | (?<Do>         do\b                     )
              | (?<Else>       else\b                   )
              | (?<ElseIf>     elseif\b                 )
              | (?<False>      false                    )
              | (?<If>         if\b                     )
              | (?<Inc>        inc\b                    )
              | (?<Return>     return\b                 )
              | (?<True>       true                     )
              | (?<Var>        var\b                    )
              | (?<While>      while\b                  )

              | (?<Bool>       (?:true\b)|(?:false\b)   )
              | (?<Int>        -?[1-9][0-9]{0-9}\b      )
              | (?<Character>  '((\\(n|r|t|\\|'|""|u[a-fA-F0-9]{6}))|[a-zA-Z0-9])' )
              | (?<String>     ""([^""\n\\]|\\([nrt\\'""]|u[0-9a-fA-F]{6}))*""    )
              | (?<End>        ;\b                      )
              | (?<Print>      print\b                  )
              | (?<Then>       then\b                   )
              | (?<Identifier> [a-zA-Z][a-Z_0-9]*       )     # Must go after all keywords
              | (?<Other>      .                        )     # Must be last: match any other character.
            ",
            RegexOptions.IgnorePatternWhitespace
                | RegexOptions.Compiled
                | RegexOptions.Multiline
            );

        static readonly IDictionary<string, TokenCategory> tokenMap =
            new Dictionary<string, TokenCategory>() {
                {"And", TokenCategory.AND},
                {"Array", TokenCategory.ARRAY},
                {"Assign", TokenCategory.ASSIGN},
                {"Bool", TokenCategory.BOOL},
                {"BitOr",TokenCategory.BIT_OR},
                {"Break", TokenCategory.BREAK},
                {"Comment", TokenCategory.COMMENT},
                {"CodeGruop", TokenCategory.CODE_GROUP},
                {"Character", TokenCategory.CHARACTER},
                {"Dec", TokenCategory.DEC},
                {"Do", TokenCategory.DO},
                {"End", TokenCategory.END},
                // EOF?
                {"Else", TokenCategory.ELSE},
                {"ElseIf", TokenCategory.ELSE_IF},
                {"Equals", TokenCategory.EQUALS},
                {"False", TokenCategory.FALSE},
                {"Identifier", TokenCategory.IDENTIFIER},
                {"If", TokenCategory.IF},
                {"Int", TokenCategory.INT},
                {"IntLiteral", TokenCategory.INT_LITERAL},
                {"Inc", TokenCategory.INC},
                {"Less", TokenCategory.LESS},
                {"LessEquals", TokenCategory.LESS_EQUALS},
                {"Mul", TokenCategory.MUL},
                {"More", TokenCategory.MORE},
                {"MoreEquals", TokenCategory.MORE_EQUALS},
                {"Neg", TokenCategory.NEG},
                {"NewLine", TokenCategory.NEW_LINE},
                {"NotEquals", TokenCategory.NOT_EQUALS},
                {"Or", TokenCategory.OR},
                {"Other", TokenCategory.OTHER},
                {"Plus", TokenCategory.PLUS},
                {"Print", TokenCategory.PRINT},
                {"Return", TokenCategory.RETURN},
                {"String", TokenCategory.STRING},
                {"Then", TokenCategory.THEN},
                {"True", TokenCategory.TRUE},
                {"Var", TokenCategory.VAR},
                {"While", TokenCategory.WHILE},
                {"ParLeft", TokenCategory.PARENTHESIS_OPEN},
                {"ParRight", TokenCategory.PARENTHESIS_CLOSE},
            };

        public Scanner(string input) {
            this.input = input;
        }

        public IEnumerable<Token> Scan() {

            var result = new LinkedList<Token>();
            var row = 1;
            var columnStart = 0;

            foreach (Match m in regex.Matches(input)) {

                if (m.Groups["Newline"].Success) {

                    row++;
                    columnStart = m.Index + m.Length;

                } else if (m.Groups["WhiteSpace"].Success
                    || m.Groups["Comment"].Success) {

                    // Skip white space and comments.

                } else if (m.Groups["Other"].Success) {

                    // Found an illegal character.
                    result.AddLast(
                        new Token(m.Value,
                            TokenCategory.ILLEGAL_CHAR,
                            row,
                            m.Index - columnStart + 1));

                } else {

                    // Must be any of the other tokens.
                    result.AddLast(FindToken(m, row, columnStart));
                }
            }

            result.AddLast(
                new Token(null,
                    TokenCategory.EOF,
                    row,
                    input.Length - columnStart + 1));

            return result;
        }

        Token FindToken(Match m, int row, int columnStart) {
            foreach (var name in tokenMap.Keys) {
                if (m.Groups[name].Success) {
                    return new Token(m.Value,
                        tokenMap[name],
                        row,
                        m.Index - columnStart + 1);
                }
            }
            throw new InvalidOperationException(
                "regex and tokenMap are inconsistent: " + m.Value);
        }
    }
}
