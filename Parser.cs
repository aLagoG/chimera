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

        static readonly ISet<TokenCategory> firstOfLiteral;

        static readonly ISet<TokenCategory> firstOfType;

        static readonly ISet<TokenCategory> firstOfStatement =
            new HashSet<TokenCategory>() { };

        static readonly ISet<TokenCategory> unaryOperators =
            new HashSet<TokenCategory>() { TokenCategory.NOT, TokenCategory.MINUS };

        static readonly ISet<TokenCategory> firstOfUnaryExpression;

        static readonly ISet<TokenCategory> firstOfSimpleExpression;

        static readonly ISet<TokenCategory> firstOfCall;

        static readonly ISet<TokenCategory> firstOfExpression;

        IEnumerator<Token> tokenStream;

        public Parser(IEnumerator<Token> tokenStream)
        {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext();

            firstOfLiteral = new HashSet(simpleLiterals);
            firstOfLiteral.Add(TokenCategory.CURLY_OPEN);

            firstOfType = new HashSet(simpleTypes);
            firstOfType.Add(TokenCategory.LIST);

            firstOfUnaryExpression = new HashSet(unaryOperators);
            firstOfUnaryExpression.Union(firstOfSimpleExpression);

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
            if (Has(TokenCategory.CURLY_OPEN))
            {
                List();
            }
            else if (Has(simpleLiterals))
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

        public void ReturnStatement()
        {
            Expect(TokenCategory.RETURN);
            Optional(firstOfExpression, Expression);
            Expect(TokenCategory.SEMICOLON);
        }

        public void ExitStatement()
        {
            Expect(TokenCategory.EXIT);
            Expect(TokenCategory.SEMICOLON);
        }

        public void Expression()
        {
            LogicExpression();
        }

        public void LogicExpression()
        {
            RelationalExpression();
            ZeroOrMore(logicOperators, RelationalExpression);
        }

        public void LogicOperator()
        {
            Expect(logicOperators);
        }

        public void RelationalExpression()
        {
            SumExpression();
            ZeroOrMore(relationalOperators, SumExpression);
        }

        public void RelationalOperator()
        {
            Expect(relationalOperators);
        }

        public void SumExpression()
        {
            MulExpression();
            ZeroOrMore(sumOperators, MulExpression);
        }

        public void SumOperator()
        {
            Expect(sumOperators);
        }

        public void MulExpression()
        {
            UnaryExpression();
            ZeroOrMore(mulOperators, UnaryExpression);
        }

        public void MulOperator()
        {
            Expect(mulOperators);
        }

        public void UnaryExpression()
        {
            if (Has(unaryOperators))
            {
                UnaryExpression();
            }
            else if (Has(firstOfSimpleExpression))
            {
                SimpleExpression();
            }
            else
            {
                throw new SyntaxError(firstOfUnaryExpression, tokenStream.Current);
            }
        }

        public void SimpleExpression()
        {
            if (Has(TokenCategory.PARENTHESIS_OPEN))
            {
                Expect(TokenCategory.PARENTHESIS_OPEN);
                Expression();
                Expect(TokenCategory.PARENTHESIS_CLOSE);
            }
            else if (Has(firstOfCall))
            {
                Call();
            }
            else if (Has(TokenCategory.IDENTIFIER))
            {
                Expect(TokenCategory.IDENTIFIER);
            }
            else if (Has(firstOfLiteral))
            {
                Literal();
            }
            else
            {
                throw new SyntaxError(firstOfSimpleExpression, tokenStream.Current);
            }
            Optional(TokenCategory.BRACKET_OPEN, () =>
            {
                Expression();
                Expect(TokenCategory.BRACKET_CLOSE);
            });
        }

        public void Call()
        {
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.PARENTHESIS_OPEN);
            if (Has(firstOfExpression))
            {
                Expression();
                ZeroOrMore(TokenCategory.COMMA, Expression);
            }
            Expect(TokenCategory.PARENTHESIS_CLOSE);
        }

    }
}
