/*
Chimera
Date: 9-Sep-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Chimera {

    class Scanner {

        readonly string input;

        static readonly Regex regex = new Regex(
            @" 
                (?<Comment>    //.*      )     # Single Line comment
                | (?<Comment>    \/\*(.|[\r\n])*\*\/  )     # Multiple Line comment
                | (?<Identifier>  \w+      )
                | (?<EndOfExpression>    [;]      )
                | (?<Assign>    :=      )
                | (?<Comma>    [,]      )
                | (?<Declare>    [:]      )
                | (?<ParOpen>    [(]       )
                | (?<ParClose>   [)]       )
                | (?<CurOpen>    [{]       )
                | (?<CurClose>   [}]       )
                | (?<BracketOpen>    [[]       )
                | (?<BracketClose>   []]       )
                | (?<Plus>       [+]       )  
                | (?<Minus>       [-]       )
                | (?<Equal>       [=]       )  
                | (?<Unequal>    <>       )
                | (?<LessThan>    [<]       )
                | (?<Newline>    \n        )
                | (?<WhiteSpace> \s        )     # Must go anywhere after Newline.
                | (?<Other>      .         )     # Must be last: match any other character.
            ", 
            RegexOptions.IgnorePatternWhitespace 
                | RegexOptions.Compiled
                | RegexOptions.Multiline
            );

        static readonly IDictionary<string, TokenCategory> keywords =
            new Dictionary<string, TokenCategory>() {
                {"const", TokenCategory.CONST},
                {"var", TokenCategory.VAR},
                {"program", TokenCategory.PROGRAM},
                {"end", TokenCategory.END},
                {"integer", TokenCategory.INTEGER},
                {"boolean", TokenCategory.BOOLEAN},
                {"string", TokenCategory.STRING},
                {"list", TokenCategory.LIST},
                {"of", TokenCategory.OF},
                {"procedure", TokenCategory.PROCEDURE},
                {"begin", TokenCategory.BEGIN},
                {"if", TokenCategory.IF},
                {"then", TokenCategory.THEN},
                {"else", TokenCategory.ELSE},
                {"loop", TokenCategory.LOOP},
                {"for", TokenCategory.FOR},
                {"in", TokenCategory.IN},
                {"do", TokenCategory.DO},
                {"loop", TokenCategory.LOOP},
                {"return", TokenCategory.RETURN},
                {"exit", TokenCategory.EXIT},
                {"and", TokenCategory.AND},
                {"or", TokenCategory.OR},
                {"xor", TokenCategory.XOR},

            };

        static readonly IDictionary<string, TokenCategory> nonKeywords =
            new Dictionary<string, TokenCategory>() {
                {"EndOfExpression", TokenCategory.END_OF_EXPRESSION},
                {"Assign", TokenCategory.ASSIGN},
                {"Comma", TokenCategory.COMMA},    
                {"Declare", TokenCategory.DECLARE},
                {"ParOpen", TokenCategory.PARENTHESIS_OPEN},
                {"ParClose", TokenCategory.PARENTHESIS_CLOSE},
                {"CurOpen", TokenCategory.CURLY_OPEN},
                {"CurClose", TokenCategory.CURLY_CLOSE},
                {"BracketOpen", TokenCategory.BRACKET_OPEN},
                {"BracketClose", TokenCategory.BRACKET_CLOSE},
                {"Equal", TokenCategory.EQUAL},
                {"Unequal", TokenCategory.UNEQUAL},
            };

        public Scanner(string input) {
            this.input = input;
        }

        public IEnumerable<Token> Start() {

            var row = 1;
            var columnStart = 0;

            Func<Match, TokenCategory, Token> newTok = (m, tc) =>
                new Token(m.Value, tc, row, m.Index - columnStart + 1);

            foreach (Match m in regex.Matches(input)) {

                if (m.Groups["Newline"].Success) {

                    // Found a new line.
                    row++;
                    columnStart = m.Index + m.Length;

                } else if (m.Groups["WhiteSpace"].Success 
                    || m.Groups["Comment"].Success) {

                    // Skip white space and comments.
                } else if (m.Groups["Identifier"].Success) {
                    if (keywords.ContainsKey(m.Value)) {

                        // Matched string is a Chimera keyword.
                        yield return newTok(m, keywords[m.Value]);                                               

                    } else { 

                        // Otherwise it's just a plain identifier.
                        yield return newTok(m, TokenCategory.IDENTIFIER);
                    }

                } else if (m.Groups["Other"].Success) {

                    // Found an illegal character.
                    yield return newTok(m, TokenCategory.ILLEGAL_CHAR);

                } else {

                    // Match must be one of the non keywords.
                    foreach (var name in nonKeywords.Keys) {
                        if (m.Groups[name].Success) {
                            yield return newTok(m, nonKeywords[name]);
                            break;
                        }
                    }
                }
            }

            yield return new Token(null, 
                                   TokenCategory.EOF, 
                                   row, 
                                   input.Length - columnStart + 1);
        }
    }
}
