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

  Modified by team 6
  Javier Pascal Flores          A01375925
  Diego Palmerin Bonada         A01747290
  Hector Ivan Aguirre Arteaga   A01169628

*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Falak
{

    class Scanner
    {

        readonly string input;


/* MISSING SUBSTRACTION */
        static readonly Regex regex = new Regex(
            @"
                (?<Comment>    (?:\#.*)|(?:\<\#(?:.|\n)*?\#>)  )
              | (?<ParLeft>    [(]                      ) 
              | (?<ParRight>   [)]                      ) 
              | (?<CurlLeft>   [{]                      ) 
              | (?<CurlRight>  [}]                      )
              | (?<ArrayLeft>  \[                       )
              | (?<ArrayRight> \]                       )
              | (?<Newline>    \n                       )
              | (?<WhiteSpace> \s                       )
              | (?<And>        [&][&]                   )
              | (?<Or>         [|][|]                   )
              | (?<BitOr>      [\^]                     )
              | (?<LessEquals> [\<][=]                  )
              | (?<MoreEquals> [\>][=]                  )
              | (?<Less>       [<]                      )
              | (?<More>       [>]                      )
              | (?<Plus>       [+]                      )              
              | (?<Neg>        [-]                      )
              | (?<Mul>        [*]                      )
              | (?<Div>        [/]                      )
              | (?<Mod>        [%]                      )
              | (?<Equals>     [=][=]                   )
              | (?<NotEquals>  [!][=]                   )
              | (?<Not>        [!]                      )
              | (?<Assign>     [=]                      )
              | (?<IntLiteral> \d+                      )
              
              # KeyWords (12)

              | (?<Break>      break\b                  )
              | (?<Dec>        dec\b                    )
              | (?<Do>         do\b                     )
              | (?<Else>       else\b                   )
              | (?<ElseIf>     elseif\b                 )
              | (?<False>      false\b                  )
              | (?<If>         if\b                     )
              | (?<Inc>        inc\b                    )
              | (?<Return>     return\b                 )
              | (?<True>       true\b                   )
              | (?<Var>        var\b                    )
              | (?<While>      while\b                  )

              | (?<Int>        -?[1-9][0-9]{0-9}\b      )
              | (?<Character>  '((\\(n|r|t|\\|'|""|u[a-fA-F0-9]{6}))|[A-z0-9/])' )
              | (?<String>     ""([^""\n\\]|\\([nrt\\'""]|u[0-9a-fA-F]{6}))*""  )
              | (?<End>        ;                        )
              | (?<Then>       then\b                   )
              | (?<Identifier> [A-z][A-z_0-9]*          )     # Must go after all keywords
              | (?<Comma>      ,                        )     # Must be last: match any other character.
              | (?<Other>      .+                       )     # Must be last: match any other character.
            ",
            RegexOptions.IgnorePatternWhitespace
                | RegexOptions.Compiled
                | RegexOptions.Multiline
            );

        static readonly IDictionary<string, TokenCategory> tokenMap =
            new Dictionary<string, TokenCategory>() {
                {"Comment", TokenCategory.COMMENT},
                {"ParLeft", TokenCategory.PAR_LEFT},
                {"ParRight", TokenCategory.PAR_RIGHT},
                {"CurlLeft", TokenCategory.CURL_LEFT},
                {"CurlRight", TokenCategory.CURL_RIGHT},
                {"ArrayLeft", TokenCategory.ARRAY_LEFT},
                {"ArrayRight", TokenCategory.ARRAY_RIGHT},
                {"Newline", TokenCategory.NEW_LINE},
                {"WhiteSpace", TokenCategory.WHITESPACE},
                {"And", TokenCategory.AND},
                {"Or", TokenCategory.OR},
                {"BitOr", TokenCategory.BIT_OR},
                {"LessEquals", TokenCategory.LESS_EQUALS},
                {"MoreEquals", TokenCategory.MORE_EQUALS},
                {"Less", TokenCategory.LESS},
                {"More", TokenCategory.MORE},
                {"Plus", TokenCategory.PLUS},
                {"Neg", TokenCategory.NEG},
                {"Not", TokenCategory.NOT},
                {"Mul", TokenCategory.MUL},
                {"Equals", TokenCategory.EQUALS},
                {"NotEquals", TokenCategory.NOT_EQUALS},
                {"Assign", TokenCategory.ASSIGN},
                {"IntLiteral", TokenCategory.INT_LITERAL},
                {"Break", TokenCategory.BREAK},
                {"Dec", TokenCategory.DEC},
                {"Do", TokenCategory.DO},
                {"Else", TokenCategory.ELSE},
                {"ElseIf", TokenCategory.ELSE_IF},
                {"False", TokenCategory.FALSE},
                {"If", TokenCategory.IF},
                {"Inc", TokenCategory.INC},
                {"Return", TokenCategory.RETURN},
                {"True", TokenCategory.TRUE},
                {"Var", TokenCategory.VAR},
                {"While", TokenCategory.WHILE},
                {"Int", TokenCategory.INT},
                {"Character", TokenCategory.CHARACTER},
                {"String", TokenCategory.STRING},
                {"End", TokenCategory.END},
                {"Then", TokenCategory.THEN},
                {"Comma", TokenCategory.COMMA},
                {"Identifier", TokenCategory.IDENTIFIER},
                {"Other", TokenCategory.OTHER},
                {"Div", TokenCategory.DIV},
                {"Mod", TokenCategory.MOD}
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

                else if (m.Groups["WhiteSpace"].Success){
                    
                }
                else if (m.Groups["Comment"].Success){
                    Regex rx = new Regex("\n");
                    MatchCollection matches = rx.Matches(m.Value);
                    row += matches.Count;
                }
                
                else if (m.Groups["Other"].Success)
                {
                    // Found an illegal character.
                    result.AddLast(
                        new Token(m.Value,
                            TokenCategory.OTHER,
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
