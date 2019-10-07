/*
Chimera
Date: 21-Oct-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Chimera
{

    class Scanner
    {

        readonly string input;

        static readonly Regex regex = new Regex(
            @"
                (?<String>          ""(?:[^""\n]|"""")*"" )
                | (?<Comment>       //.*                  )   # Single Line comment
                | (?<Comment>       \/\*(?:.|\n)*?\*\/    )   # Multi Line comment
                | (?<Identifier>    [a-z]\w*              )
                | (?<IntLiteral>    \d+                   )
                | (?<Semicolon>     ;                     )
                | (?<ColonEqual>    :=                    )
                | (?<Comma>         ,                     )
                | (?<Colon>         :                     )
                | (?<ParOpen>       [(]                   )
                | (?<ParClose>      [)]                   )
                | (?<CurOpen>       [{]                   )
                | (?<CurClose>      [}]                   )
                | (?<BracketOpen>   [[]                   )
                | (?<BracketClose>  []]                   )
                | (?<Plus>          [+]                   )
                | (?<Minus>         -                     )
                | (?<Times>         [*]                   )
                | (?<LessThanEqual> <=                    )
                | (?<MoreThanEqual> >=                    )
                | (?<Equal>         =                     )
                | (?<Unequal>       <>                    )
                | (?<LessThan>      <                     )
                | (?<MoreThan>      >                     )
                | (?<NewLine>       \n                    )
                | (?<WhiteSpace>    \s                    )     # Must go anywhere after Newline.
                | (?<Other>         .                     )     # Must be last: match any other character.
            ",
            RegexOptions.IgnorePatternWhitespace
                | RegexOptions.Compiled
                | RegexOptions.Multiline
                | RegexOptions.IgnoreCase
            );

        static readonly IDictionary<string, TokenCategory> keywords =
            new Dictionary<string, TokenCategory>() {
                {"const", TokenCategory.CONST},
                {"var", TokenCategory.VAR},
                {"program", TokenCategory.PROGRAM},
                {"end", TokenCategory.END},
                {"integer", TokenCategory.INTEGER},
                {"string", TokenCategory.STRING},
                {"boolean", TokenCategory.BOOLEAN},
                {"list", TokenCategory.LIST},
                {"of", TokenCategory.OF},
                {"procedure", TokenCategory.PROCEDURE},
                {"begin", TokenCategory.BEGIN},
                {"if", TokenCategory.IF},
                {"then", TokenCategory.THEN},
                {"elseif", TokenCategory.ELSEIF},
                {"else", TokenCategory.ELSE},
                {"loop", TokenCategory.LOOP},
                {"for", TokenCategory.FOR},
                {"in", TokenCategory.IN},
                {"do", TokenCategory.DO},
                {"return", TokenCategory.RETURN},
                {"exit", TokenCategory.EXIT},
                {"and", TokenCategory.AND},
                {"or", TokenCategory.OR},
                {"xor", TokenCategory.XOR},
                {"div", TokenCategory.DIV},
                {"rem", TokenCategory.REM},
                {"not", TokenCategory.NOT},
                {"true", TokenCategory.TRUE},
                {"false", TokenCategory.FALSE},
            };

        static readonly IDictionary<string, TokenCategory> nonKeywords =
            new Dictionary<string, TokenCategory>() {
                {"Semicolon", TokenCategory.SEMICOLON},
                {"String", TokenCategory.STRING_LITERAL},
                {"ColonEqual", TokenCategory.COLON_EQUAL},
                {"Comma", TokenCategory.COMMA},
                {"Colon", TokenCategory.COLON},
                {"ParOpen", TokenCategory.PARENTHESIS_OPEN},
                {"ParClose", TokenCategory.PARENTHESIS_CLOSE},
                {"CurOpen", TokenCategory.CURLY_OPEN},
                {"CurClose", TokenCategory.CURLY_CLOSE},
                {"BracketOpen", TokenCategory.BRACKET_OPEN},
                {"BracketClose", TokenCategory.BRACKET_CLOSE},
                {"Equal", TokenCategory.EQUAL},
                {"Unequal", TokenCategory.UNEQUAL},
                {"LessThanEqual", TokenCategory.LESS_THAN_EQUAL},
                {"MoreThanEqual", TokenCategory.MORE_THAN_EQUAL},
                {"LessThan", TokenCategory.LESS_THAN},
                {"MoreThan", TokenCategory.MORE_THAN},
                {"Plus", TokenCategory.PLUS},
                {"Minus", TokenCategory.MINUS},
                {"Times", TokenCategory.TIMES},
                {"IntLiteral", TokenCategory.INT_LITERAL}
            };

        public Scanner(string input)
        {
            this.input = input;
        }

        public IEnumerable<Token> Start()
        {

            var row = 1;
            var columnStart = 0;

            Func<Match, TokenCategory, Token> newTok = (m, tc) =>
                new Token(m.Value, tc, row, m.Index - columnStart + 1);

            foreach (Match m in regex.Matches(input))
            {
                if (m.Groups["NewLine"].Success)
                {
                    // Found a new line.
                    row++;
                    columnStart = m.Index + m.Length;
                }
                else if (m.Groups["WhiteSpace"].Success)
                {
                    // Skip white space.
                }
                else if (m.Groups["Comment"].Success)
                {
                    // Found a comment.
                    // Process New lines for better row and column detection
                    MatchCollection newLineMatches = Regex.Matches(m.Groups["Comment"].Value, "\n", RegexOptions.Multiline);

                    if (newLineMatches.Count > 0)
                    {
                        Match lastMatch = newLineMatches[newLineMatches.Count - 1];
                        row += newLineMatches.Count;
                        columnStart = m.Index + lastMatch.Index + lastMatch.Length;
                    }
                }
                else if (m.Groups["Identifier"].Success)
                {
                    if (keywords.ContainsKey(m.Value))
                    {
                        // Matched string is a Chimera keyword.
                        yield return newTok(m, keywords[m.Value]);
                    }
                    else
                    {
                        // Otherwise it's just a plain identifier.
                        yield return newTok(m, TokenCategory.IDENTIFIER);
                    }
                }
                else if (m.Groups["Other"].Success)
                {
                    // Found an illegal character.
                    yield return newTok(m, TokenCategory.ILLEGAL_CHAR);
                }
                else
                {
                    // Match must be one of the non keywords.
                    foreach (var name in nonKeywords.Keys)
                    {
                        if (m.Groups[name].Success)
                        {
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
