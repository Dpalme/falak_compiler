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

namespace Falak
{

    class Scanner
    {

        readonly string input;

        static readonly Regex regex = new Regex(
            @"
                (?<Comment>    (?:\#.*)|(<\#.*?\#>)     )
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
              | (?<Character>  '((\\(n|r|t|\\|'|""|u[a-fA-F0-9]{6}))|[A-z0-9])' )
              | (?<String>     ""([^""\n\\]|\\([nrt\\'""]|u[0-9a-fA-F]{6}))*""  )
              | (?<End>        ;\b                      )
              | (?<Print>      print\b                  )
              | (?<Then>       then\b                   )
              | (?<Identifier> [A-z][A-z_0-9]*          )     # Must go after all keywords
              | (?<Other>      .                        )     # Must be last: match any other character.
            ",
            RegexOptions.IgnorePatternWhitespace
                | RegexOptions.Compiled
                | RegexOptions.Multiline
            );

        static readonly IDictionary<string, TokenCategory> tokenMap =
            new Dictionary<string, TokenCategory>() {
                {"Comment", TokenCategory.COMMENT},
                {"SoftGroup", TokenCategory.SOFTGROUP},
                {"CodeGroup", TokenCategory.CODEGROUP},
                {"Array", TokenCategory.ARRAY},
                {"Newline", TokenCategory.NEWLINE},
                {"WhiteSpace", TokenCategory.WHITESPACE},
                {"And", TokenCategory.AND},
                {"Or", TokenCategory.OR},
                {"BitOr", TokenCategory.BITOR},
                {"LessEquals", TokenCategory.LESSEQUALS},
                {"MoreEquals", TokenCategory.MOREEQUALS},
                {"Less", TokenCategory.LESS},
                {"More", TokenCategory.MORE},
                {"Plus", TokenCategory.PLUS},
                {"Neg", TokenCategory.NEG},
                {"Mul", TokenCategory.MUL},
                {"Equals", TokenCategory.EQUALS},
                {"NotEquals", TokenCategory.NOTEQUALS},
                {"Assign", TokenCategory.ASSIGN},
                {"IntLiteral", TokenCategory.INTLITERAL},
                {"Break", TokenCategory.BREAK},
                {"Dec", TokenCategory.DEC},
                {"Do", TokenCategory.DO},
                {"Else", TokenCategory.ELSE},
                {"ElseIf", TokenCategory.ELSEIF},
                {"False", TokenCategory.FALSE},
                {"If", TokenCategory.IF},
                {"Inc", TokenCategory.INC},
                {"Return", TokenCategory.RETURN},
                {"True", TokenCategory.TRUE},
                {"Var", TokenCategory.VAR},
                {"While", TokenCategory.WHILE},
                {"Bool", TokenCategory.BOOL},
                {"Int", TokenCategory.INT},
                {"Character", TokenCategory.CHARACTER},
                {"String", TokenCategory.STRING},
                {"End", TokenCategory.END},
                {"Print", TokenCategory.PRINT},
                {"Then", TokenCategory.THEN},
                {"Identifier", TokenCategory.IDENTIFIER},
                {"Other", TokenCategory.OTHER}
            };

        public Scanner(string input)
        {
            this.input = input;
        }

        public IEnumerable<Token> Scan()
        {

            var result = new LinkedList<Token>();
            var row = 1;
            var columnStart = 0;

            foreach (Match m in regex.Matches(input))
            {

                if (m.Groups["Newline"].Success)
                {

                    row++;
                    columnStart = m.Index + m.Length;

                }
                else if (m.Groups["WhiteSpace"].Success
                  || m.Groups["Comment"].Success)
                {

                    // Skip white space and comments.

                }
                else if (m.Groups["Other"].Success)
                {

                    // Found an illegal character.
                    result.AddLast(
                        new Token(m.Value,
                            TokenCategory.ILLEGAL_CHAR,
                            row,
                            m.Index - columnStart + 1));

                }
                else
                {

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

        Token FindToken(Match m, int row, int columnStart)
        {
            foreach (var name in tokenMap.Keys)
            {
                if (m.Groups[name].Success)
                {
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
