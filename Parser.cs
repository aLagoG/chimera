/*
Chimera
Date: 23-Sep-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

// Buttercup examples:

// public void Statement() {
//     switch (CurrentToken)
//     {
//         case TokenCategory.IDENTIFIER:
//             Assignment();
//             break;
//         case TokenCategory.PRINT:
//             Print();
//             break;
//         case TokenCategory.IF:
//             If();
//             break;
//         default:
//             throw new SyntaxError(firstOfStatement,
//                                   tokenStream.Current);
//     }
// }

// public void If() {
//     Expect(TokenCategory.IF);
//     Expression();
//     Expect(TokenCategory.THEN);
//     while (firstOfStatement.Contains(CurrentToken)) {
//         Statement();
//     }
//     Expect(TokenCategory.END);
// }

using System;
using System.Collections.Generic;

namespace Chimera
{

    class Parser
    {
        static readonly ISet<TokenCategory> simpleTypes =
            new HashSet<TokenCategory>() {
                TokenCategory.INTEGER,
                TokenCategory.STRING,
                TokenCategory.BOOLEAN
            };

        static readonly ISet<TokenCategory> simpleLiterals =
            new HashSet<TokenCategory>() {
                TokenCategory.INT_LITERAL,
                TokenCategory.STRING_LITERAL,
                TokenCategory.TRUE,
                TokenCategory.FALSE
            };

        static readonly ISet<TokenCategory> logicOperators =
            new HashSet<TokenCategory>() {
                TokenCategory.AND,
                TokenCategory.OR,
                TokenCategory.XOR
            };

        static readonly ISet<TokenCategory> relationalOperators =
            new HashSet<TokenCategory>() {
                TokenCategory.EQUAL,
                TokenCategory.UNEQUAL,
                TokenCategory.LESS_THAN,
                TokenCategory.MORE_THAN,
                TokenCategory.LESS_THAN_EQUAL,
                TokenCategory.MORE_THAN_EQUAL,
            };

        static readonly ISet<TokenCategory> sumOperators =
            new HashSet<TokenCategory>() {
                TokenCategory.PLUS,
                TokenCategory.MINUS,
            };

        static readonly ISet<TokenCategory> mulOperators =
            new HashSet<TokenCategory>() {
                TokenCategory.TIMES,
                TokenCategory.DIV,
                TokenCategory.REM,
            };

        static readonly ISet<TokenCategory> firstOfLiteral =
            new HashSet<TokenCategory>() {
                TokenCategory.INT_LITERAL,
                TokenCategory.STRING_LITERAL,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
                TokenCategory.CURLY_OPEN
            };

        static readonly ISet<TokenCategory> firstOfType =
            new HashSet<TokenCategory>() {
                TokenCategory.INTEGER,
                TokenCategory.STRING,
                TokenCategory.BOOLEAN,
                TokenCategory.LIST
            };

        IEnumerator<Token> tokenStream;

        public Parser(IEnumerator<Token> tokenStream)
        {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext();
        }

        public TokenCategory CurrentToken
        {
            get { return tokenStream.Current.Category; }
        }

        public Token Expect(TokenCategory category)
        {
            if (CurrentToken == category)
            {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return current;
            }
            else
            {
                throw new SyntaxError(category, tokenStream.Current);
            }
        }

        public Token ExpectAnyOf(ISet<TokenCategory> categories)
        {
            if (categories.Contains(CurrentToken))
            {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return current;
            }
            else
            {
                throw new SyntaxError(categories, tokenStream.Current);
            }
        }

        public void Program()
        {
            if (CurrentToken == TokenCategory.CONST)
            {
                Expect(TokenCategory.CONST);
                do
                {
                    ConstantDeclaration();
                }
                while (CurrentToken == TokenCategory.IDENTIFIER);
            }
            if (CurrentToken == TokenCategory.VAR)
            {
                Expect(TokenCategory.VAR);
                do
                {
                    VariableDeclaration();
                }
                while (CurrentToken == TokenCategory.IDENTIFIER);
            }
            while (CurrentToken == TokenCategory.PROCEDURE)
            {
                ProcedureDeclaration();
            }
            Expect(TokenCategory.PROGRAM);
            // TODO: change this with startOfStatement
            while (false)
            {
                Statement();
            }
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
        }

        public void ConstantDeclaration()
        {
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.COLON_EQUAL);
            Literal();
            Expect(TokenCategory.SEMICOLON);
        }

        public void VariableDeclaration()
        {
            Expect(TokenCategory.IDENTIFIER);
            while (CurrentToken == TokenCategory.COMMA)
            {
                Expect(TokenCategory.COMMA);
                Expect(TokenCategory.IDENTIFIER);
            }
            Expect(TokenCategory.COLON);
            Type();
            Expect(TokenCategory.SEMICOLON);
        }

        public void Literal()
        {
            if (CurrentToken == TokenCategory.CURLY_OPEN)
            {
                List();
            }
            else if (simpleLiterals.Contains(CurrentToken))
            {
                SimpleLiteral();
            }
            else
            {
                throw new SyntaxError(firstOfLiteral, tokenStream.Current);
            }
        }

        public void SimpleLiteral()
        {
            ExpectAnyOf(simpleLiterals);
        }

        public void Type()
        {
            if (CurrentToken == TokenCategory.LIST)
            {
                ListType();
            }
            else if (simpleTypes.Contains(CurrentToken))
            {
                SimpleType();
            }
            else
            {
                throw new SyntaxError(firstOfType, tokenStream.Current);
            }
        }

        public void SimpleType()
        {
            ExpectAnyOf(simpleTypes);
        }

        public void ListType()
        {
            Expect(TokenCategory.LIST);
            Expect(TokenCategory.OF);
            SimpleType();
        }

        public void List()
        {
            Expect(TokenCategory.CURLY_OPEN);
            if (simpleLiterals.Contains(CurrentToken))
            {
                SimpleLiteral();
                while (CurrentToken == TokenCategory.COMMA)
                {
                    Expect(TokenCategory.COMMA);
                    SimpleLiteral();
                }
            }
            Expect(TokenCategory.CURLY_CLOSE);
        }

        public void ProcedureDeclaration() { }

        // The doc is probably wrong about this one becasue its litteraly the same as VariableDeclaration
        // Looks more like <identifier>:<type>;
        public void ParameterDeclaration()
        {
            Expect(TokenCategory.IDENTIFIER);
            while (CurrentToken == TokenCategory.COMMA)
            {
                Expect(TokenCategory.COMMA);
                Expect(TokenCategory.IDENTIFIER);
            }
            Expect(TokenCategory.COLON);
            Type();
            Expect(TokenCategory.SEMICOLON);
        }

        public void Statement() { }

        public void AssignmentStatement() { }

        public void CallStatement() { }

        public void IfStatement() { }

        public void LoopStatement() { }

        public void ForStatement() { }

        public void ReturnStatement() { }

        public void ExitStatement()
        {
            Expect(TokenCategory.EXIT);
            Expect(TokenCategory.SEMICOLON);
        }

        public void Expression() { }

        public void LogicExpression() { }

        public void LogicOperator()
        {
            ExpectAnyOf(logicOperators);
        }

        public void RelationalExpression() { }

        public void RelationalOperator()
        {
            ExpectAnyOf(relationalOperators);
        }

        public void SumExpression() { }

        public void SumOperator()
        {
            ExpectAnyOf(sumOperators);
        }

        public void MulExpression() { }

        public void MulOperator()
        {
            ExpectAnyOf(mulOperators);
        }

        public void UnaryExpression() { }

        public void SimpleExpression() { }

        public void Call() { }

    }
}