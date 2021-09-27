using System;
using System.Collections.Generic;
using System.Text;

namespace Falak {

    class SyntaxError: Exception {

        public SyntaxError(TokenCategory expectedCategory,
                           Token token):
            base($"Syntax Error: Expecting {expectedCategory} \n"
                 + $"but found {token.Category} (\"{token.Lexeme}\") at "
                 + $"row {token.Row}, column {token.Column}.") {
        }

        public SyntaxError(ISet<TokenCategory> expectedCategories,
                           Token token):
            base($"Syntax Error: Expecting one of {Elements(expectedCategories)}\n"
                 + $"but found {token.Category} (\"{token.Lexeme}\") at "
                 + $"row {token.Row}, column {token.Column}.") {
        }

        static string Elements(ISet<TokenCategory> expectedCategories) {
            var sb = new StringBuilder("{");
            var first = true;
            foreach (var elem in expectedCategories) {
                if (first) {
                    first = false;
                } else {
                    sb.Append(", ");
                }
                sb.Append(elem);
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
