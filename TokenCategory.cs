/*
Chimera
Date: 9-Sep-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

namespace Chimera
{

    enum TokenCategory
    {
        IDENTIFIER,
        ILLEGAL_CHAR,
        EOF,
        PROGRAM,
        CONST,
        VAR,
        COLON,
        COMMA,
        SEMICOLON,
        ASSIGN,
        END,
        INTEGER,
        INT_LITERAL,
        BOOLEAN,
        STRING,
        STRING_LITERAL,
        LIST,
        OF,
        CURLY_OPEN,
        CURLY_CLOSE,
        PARENTHESIS_OPEN,
        PARENTHESIS_CLOSE,
        PROCEDURE,
        BEGIN,
        BRACKET_OPEN,
        BRACKET_CLOSE,
        IF,
        THEN,
        ELSEIF,
        ELSE,
        LOOP,
        FOR,
        IN,
        DO,
        RETURN,
        EXIT,
        AND,
        OR,
        XOR,
        EQUAL,
        UNEQUAL,
        LESS_THAN,
        MORE_THAN,
        LESS_THAN_EQUAL,
        MORE_THAN_EQUAL,
        PLUS,
        MINUS,
        TIMES,
        DIV,
        REM,
        NOT,
        TRUE,
        FALSE,
    }
}

