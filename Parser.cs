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

        static readonly ISet<TokenCategory> firstOfStatement =
            new HashSet<TokenCategory>() { };

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

        public bool Has<T>(T category)
        {
            var token = category as TokenCategory?;
            if (token != null)
            {
                return CurrentToken == token;
            }
            var tokenSet = category as ISet<TokenCategory>;
            if (tokenSet != null)
            {
                return tokenSet.Contains(CurrentToken);
            }
            throw new NotImplementedException($"Has method is not implemented for type {typeof(T).FullName}");
        }


        public void Expect<T>(T category)
        {
            if (Has(category))
            {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return;
            }
            else
            {
                var token = category as TokenCategory?;
                if (token != null)
                {
                    throw new SyntaxError((TokenCategory)token, tokenStream.Current);
                }
                var tokenSet = category as ISet<TokenCategory>;
                if (tokenSet != null)
                {
                    throw new SyntaxError(tokenSet, tokenStream.Current);
                }
                throw new NotImplementedException($"SyntaxError is not implemented for type {typeof(T).FullName}");
            }
        }

        public void Optional<T>(T category, Action onSuccess
        )
        {
            if (Has(category))
            {
                Expect(category);
                onSuccess();
            }
        }

        public void ZeroOrMore<T>(T category, Action onSucces
        )
        {
            while (Has(category))
            {
                Expect(category);
                onSucces();
            }
        }

        public void OneOrMore<T>(T category, Action onSucces
        )
        {
            do
            {
                Expect(category);
                onSucces();
            } while (Has(category));
        }

        public void Program()
        {
            Optional(TokenCategory.CONST, () =>
            {
                OneOrMore(TokenCategory.IDENTIFIER, ConstantDeclaration);
            });
            Optional(TokenCategory.VAR, () =>
            {
                OneOrMore(TokenCategory.IDENTIFIER, VariableDeclaration);
            });
            ZeroOrMore(TokenCategory.PROCEDURE, ProcedureDeclaration);
            Expect(TokenCategory.PROGRAM);
            ZeroOrMore(firstOfStatement, Statement);
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
            ZeroOrMore(TokenCategory.COMMA, () =>
            {
                Expect(TokenCategory.IDENTIFIER);
            });
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
            Expect(simpleLiterals);
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
            Expect(simpleTypes);
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
            Optional(simpleLiterals, () =>
            {
                ZeroOrMore(TokenCategory.COMMA, SimpleLiteral);
            });
            Expect(TokenCategory.CURLY_CLOSE);
        }

        public void ProcedureDeclaration()
        {
            Expect(TokenCategory.PROCEDURE);
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.PARENTHESIS_OPEN);
            ZeroOrMore(TokenCategory.IDENTIFIER, ParameterDeclaration);
            Expect(TokenCategory.PARENTHESIS_CLOSE);
            Optional(TokenCategory.COLON, Type);
            Expect(TokenCategory.SEMICOLON);
            Optional(TokenCategory.CONST, () =>
            {
                OneOrMore(TokenCategory.IDENTIFIER, ConstantDeclaration);
            });
            Optional(TokenCategory.VAR, () =>
            {
                OneOrMore(TokenCategory.IDENTIFIER, VariableDeclaration);
            });
            Expect(TokenCategory.BEGIN);
            ZeroOrMore(firstOfStatement, Statement);
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
        }

        // The doc is probably wrong about this one becasue its litteraly the same as VariableDeclaration
        // Looks more like <identifier>:<type>;
        public void ParameterDeclaration()
        {
            Expect(TokenCategory.IDENTIFIER);
            ZeroOrMore(TokenCategory.COMMA, () =>
            {
                Expect(TokenCategory.IDENTIFIER);
            });
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
            Expect(logicOperators);
        }

        public void RelationalExpression() { }

        public void RelationalOperator()
        {
            Expect(relationalOperators);
        }

        public void SumExpression() { }

        public void SumOperator()
        {
            Expect(sumOperators);
        }

        public void MulExpression() { }

        public void MulOperator()
        {
            Expect(mulOperators);
        }

        public void UnaryExpression() { }

        public void SimpleExpression() { }

        public void Call() { }

    }
}
