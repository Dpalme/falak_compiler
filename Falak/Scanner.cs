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
              | (?<SoftGroup>  [(].*?[)]                )
              | (?<CodeGroup>  [{].*?[}]                )
              | (?<Array>      \[.*?\]                  )
              | (?<Newline>    \n                       )
              | (?<WhiteSpace> \s                       )     # Must go after Newline.
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
              | (?<True>       true                     )
              | (?<False>      false                    )
              | (?<IntLiteral> \d+                      )
              | (?<Break>      break\b                  )
              | (?<Return>     return\b                 )
              | (?<Dec>        dec\b                    )
              | (?<Inc>        inc\b                    )
              | (?<Do>         do\b                     )
              | (?<Boolean>    (?:true\b)|(?:false\b)   )
              | (?<Integer>    -?[1-9][0-9]{0-9}\b      )
              | (?<Character>  '(?:[\](?:n|r|t|[\]|'|\|""|u[0-9a-fA-F]{8})'\b  )
              | (?<String>     ""\w\s""\b    )
              | (?<End>        ;\b                      )
              | (?<If>         if\b                     )
              | (?<Else>       else\b                   )
              | (?<ElseIf>     elseif\b                 )
              | (?<Var>        var\b                    )
              | (?<While>      while\b                  )
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
                {"Less", TokenCategory.LESS},
                {"Plus", TokenCategory.PLUS},
                {"Mul", TokenCategory.MUL},
                {"Neg", TokenCategory.NEG},
                {"ParLeft", TokenCategory.PARENTHESIS_OPEN},
                {"ParRight", TokenCategory.PARENTHESIS_CLOSE},
                {"Assign", TokenCategory.ASSIGN},
                {"True", TokenCategory.TRUE},
                {"False", TokenCategory.FALSE},
                {"IntLiteral", TokenCategory.INT_LITERAL},
                {"Bool", TokenCategory.BOOL},
                {"End", TokenCategory.END},
                {"If", TokenCategory.IF},
                {"Int", TokenCategory.INT},
                {"Print", TokenCategory.PRINT},
                {"Then", TokenCategory.THEN},
                {"Identifier", TokenCategory.IDENTIFIER}
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
