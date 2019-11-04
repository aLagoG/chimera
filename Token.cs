/*
Chimera
Date: 11-Nov-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System;

namespace Chimera
{

    class Token
    {

        readonly string lexeme;

        readonly TokenCategory category;

        readonly int row;

        readonly int column;

        public string Lexeme
        {
            get { return lexeme; }
        }

        public TokenCategory Category
        {
            get { return category; }
        }

        public int Row
        {
            get { return row; }
        }

        public int Column
        {
            get { return column; }
        }

        public Token(string lexeme,
                     TokenCategory category,
                     int row,
                     int column)
        {
            this.lexeme = lexeme;
            this.category = category;
            this.row = row;
            this.column = column;
        }

        public override string ToString()
        {
            return $"{{{category}, '{lexeme}', @({row}, {column})}}";
        }

        public string ToEscapedString()
        {
            var escapedLexeme = lexeme.Replace("\"", "");
            return $"{{{category}, '{escapedLexeme}', @({row}, {column})}}";
        }
    }
}

