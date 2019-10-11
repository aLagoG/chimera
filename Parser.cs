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

        static readonly ISet<TokenCategory> unaryOperators =
            new HashSet<TokenCategory>() { TokenCategory.NOT, TokenCategory.MINUS };

        static readonly ISet<TokenCategory> firstOfStatement =
            new HashSet<TokenCategory>() {
                TokenCategory.IDENTIFIER,
                TokenCategory.IF,
                TokenCategory.LOOP,
                TokenCategory.FOR,
                TokenCategory.RETURN,
                TokenCategory.EXIT
            };

        static readonly ISet<TokenCategory> firstOfUnaryExpression;

        static readonly ISet<TokenCategory> firstOfSimpleExpression;

        static readonly ISet<TokenCategory> firstOfExpression;

        static readonly ISet<TokenCategory> firstOfAssignmentOrCallStatement =
            new HashSet<TokenCategory>() {
                TokenCategory.PARENTHESIS_OPEN,
                TokenCategory.BRACKET_OPEN,
                TokenCategory.COLON_EQUAL
            };
        
         static readonly ISet<TokenCategory> exitExpression =
            new HashSet<TokenCategory>() {
                TokenCategory.EXIT,
                TokenCategory.SEMICOLON
            };

        IEnumerator<Token> tokenStream;

        static Parser()
        {
            firstOfLiteral = new HashSet<TokenCategory>(simpleLiterals);
            firstOfLiteral.Add(TokenCategory.CURLY_OPEN);

            firstOfType = new HashSet<TokenCategory>(simpleTypes);
            firstOfType.Add(TokenCategory.LIST);

            firstOfSimpleExpression = new HashSet<TokenCategory>(firstOfLiteral);
            firstOfSimpleExpression.Add(TokenCategory.PARENTHESIS_OPEN);
            firstOfSimpleExpression.Add(TokenCategory.IDENTIFIER);

            firstOfUnaryExpression = new HashSet<TokenCategory>(unaryOperators);
            firstOfUnaryExpression.UnionWith(firstOfSimpleExpression);

            firstOfExpression = new HashSet<TokenCategory>(firstOfUnaryExpression);
        }

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


        public Token Expect<T>(T category)
        {
            if (Has(category))
            {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return current;
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

        public Node Optional<T>(T category, Action onSuccess, bool expect = false
        )
        {
            if (Has(category))
            {
                if (expect)
                {
                    Expect(category);
                }
                onSuccess();
            }
        }

        public Node ZeroOrMore<T>(T category, Action onSucces, bool expect = false
        )
        {
            while (Has(category))
            {
                if (expect)
                {
                    Expect(category);
                }
                onSucces();
            }
        }

        public Node OneOrMore<T>(T category, Action onSucces, bool expect = false
        )
        {
            do
            {
                if (expect)
                {
                    Expect(category);
                }
                onSucces();
            } while (Has(category));
        }

        public Node Program()
        {
            Optional(TokenCategory.CONST, () =>
            {
                OneOrMore(TokenCategory.IDENTIFIER, ConstantDeclaration);
            }, true);
            Optional(TokenCategory.VAR, () =>
            {
                OneOrMore(TokenCategory.IDENTIFIER, VariableDeclaration);
            }, true);
            ZeroOrMore(TokenCategory.PROCEDURE, ProcedureDeclaration);
            Expect(TokenCategory.PROGRAM);
            ZeroOrMore(firstOfStatement, Statement);
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
        }

        public Node ConstantDeclaration()
        {
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.COLON_EQUAL);
            Literal();
            Expect(TokenCategory.SEMICOLON);
        }

        public Node VariableDeclaration()
        {
            Expect(TokenCategory.IDENTIFIER);
            ZeroOrMore(TokenCategory.COMMA, () =>
            {
                Expect(TokenCategory.IDENTIFIER);
            }, true);
            Expect(TokenCategory.COLON);
            Type();
            Expect(TokenCategory.SEMICOLON);
        }

        public Node Literal()
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

        public Node SimpleLiteral()
        {   
            switch (CurrentToken) {

                case TokenCategory.INT_LITERAL:
                    return new IntLiteralNode() { 
                        AnchorToken = Expect(TokenCategory.INT_LITERAL) 
                    };

                case TokenCategory.STRING_LITERAL:
                    return new StringLiteralNode() {
                        AnchorToken = Expect(TokenCategory.STRING_LITERAL)
                    };

                case TokenCategory.TRUE:
                    return new TrueNode() {
                        AnchorToken = Expect(TokenCategory.TRUE)
                    };

                case TokenCategory.FALSE:
                    return new FalseNode() {
                        AnchorToken = Expect(TokenCategory.FALSE)
                    };        

                default:
                    throw new SyntaxError(simpleLiterals, 
                                        tokenStream.Current);
            }
        }

        public Node Type()
        {
            var type_node = new TypeNode();
            if (CurrentToken == TokenCategory.LIST)
            {
                type_node.Add(ListType());
                return type_node;
            }
            else if (simpleTypes.Contains(CurrentToken))
            {
                type_node.Add(SimpleType());
                return type_node;
            }
            else
            {
                throw new SyntaxError(firstOfType, tokenStream.Current);
            }
        }

        public Node SimpleType()
        {   
            switch (CurrentToken) {

                case TokenCategory.INTEGER:
                    return new IntNode() { 
                        AnchorToken = Expect(TokenCategory.INTEGER) 
                    };

                case TokenCategory.STRING:
                    return new StringNode() {
                        AnchorToken = Expect(TokenCategory.STRING)
                    };

                case TokenCategory.BOOLEAN:
                    return new BooleanNode() {
                        AnchorToken = Expect(TokenCategory.BOOLEAN)
                    };       

                default:
                    throw new SyntaxError(simpleTypes, 
                                        tokenStream.Current);
            }
        }

        public Node ListType()
        {
            Expect(TokenCategory.LIST);
            Expect(TokenCategory.OF);
            var simple_type_node = new ListTypeNode();
            simple_type_node.Add(SimpleType());
            return simple_type_node;
        }

        public Node List()
        {
            Expect(TokenCategory.CURLY_OPEN);
            Optional(simpleLiterals, () =>
            {
                OneOrMore(TokenCategory.COMMA, SimpleLiteral, true);
            }, true);
            Expect(TokenCategory.CURLY_CLOSE);
        }

        public Node ProcedureDeclaration()
        {
            Expect(TokenCategory.PROCEDURE);
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.PARENTHESIS_OPEN);
            ZeroOrMore(TokenCategory.IDENTIFIER, ParameterDeclaration);
            Expect(TokenCategory.PARENTHESIS_CLOSE);
            Optional(TokenCategory.COLON, Type, true);
            Expect(TokenCategory.SEMICOLON);
            Optional(TokenCategory.CONST, () =>
            {
                OneOrMore(TokenCategory.IDENTIFIER, ConstantDeclaration);
            }, true);
            Optional(TokenCategory.VAR, () =>
            {
                OneOrMore(TokenCategory.IDENTIFIER, VariableDeclaration);
            }, true);
            Expect(TokenCategory.BEGIN);
            ZeroOrMore(firstOfStatement, Statement);
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
        }

        public Node ParameterDeclaration()
        {
            Expect(TokenCategory.IDENTIFIER);
            ZeroOrMore(TokenCategory.COMMA, () =>
            {
                Expect(TokenCategory.IDENTIFIER);
            }, true);
            Expect(TokenCategory.COLON);
            Type();
            Expect(TokenCategory.SEMICOLON);
        }

        public Node Statement()
        {
            if (Has(TokenCategory.IDENTIFIER))
            {
                Expect(TokenCategory.IDENTIFIER);
                AssignmentOrCallStatement();
            }
            else if (Has(TokenCategory.IF))
            {
                IfStatement();
            }
            else if (Has(TokenCategory.LOOP))
            {
                LoopStatement();
            }
            else if (Has(TokenCategory.FOR))
            {
                ForStatement();
            }
            else if (Has(TokenCategory.RETURN))
            {
                ReturnStatement();
            }
            else if (Has(TokenCategory.EXIT))
            {
                ExitStatement();
            }
            else
            {
                throw new SyntaxError(firstOfStatement, tokenStream.Current);
            }
        }

        public Node AssignmentOrCallStatement()
        {
            if (Has(TokenCategory.PARENTHESIS_OPEN))
            {
                Expect(TokenCategory.PARENTHESIS_OPEN);
                Optional(firstOfExpression, () =>
                {
                    Expression();
                    ZeroOrMore(TokenCategory.COMMA, Expression, true);
                });
                Expect(TokenCategory.PARENTHESIS_CLOSE);
                Expect(TokenCategory.SEMICOLON);
            }
            else if (Has(TokenCategory.BRACKET_OPEN) || Has(TokenCategory.COLON_EQUAL))
            {
                Optional(TokenCategory.BRACKET_OPEN, () =>
                {
                    Expression();
                    Expect(TokenCategory.BRACKET_CLOSE);
                }, true);
                Expect(TokenCategory.COLON_EQUAL);
                Expression();
                Expect(TokenCategory.SEMICOLON);
            }
            else
            {
                throw new SyntaxError(firstOfAssignmentOrCallStatement, tokenStream.Current);
            }
        }

        public Node IfStatement()
        {
            Expect(TokenCategory.IF);
            Expression();
            Expect(TokenCategory.THEN);
            ZeroOrMore(firstOfStatement, Statement);
            ZeroOrMore(TokenCategory.ELSEIF, () =>
            {
                Expression();
                Expect(TokenCategory.THEN);
                ZeroOrMore(firstOfStatement, Statement);
            }, true);
            Optional(TokenCategory.ELSE, () => { ZeroOrMore(firstOfStatement, Statement); }, true);
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
        }

        public Node LoopStatement()
        {
            Expect(TokenCategory.LOOP);
            ZeroOrMore(firstOfStatement, Statement);
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
        }

        public Node ForStatement()
        {
            Expect(TokenCategory.FOR);
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.IN);
            Expression();
            Expect(TokenCategory.DO);
            ZeroOrMore(firstOfStatement, Statement);
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
        }

        public Node ReturnStatement()
        {
            Expect(TokenCategory.RETURN);
            Optional(firstOfExpression, Expression);
            Expect(TokenCategory.SEMICOLON);
        }

        public Node ExitStatement()
        {
            if(CurrentToken == TokenCategory.EXIT){
                var exit_node = new ExitNode() { 
                        AnchorToken = Expect(TokenCategory.EXIT) 
                    };
                Expect(TokenCategory.SEMICOLON);
                return exit_node;
            }
            throw new SyntaxError(exitExpression, 
                                    tokenStream.Current);
        }

        public Node Expression()
        {
            LogicExpression();
        }

        public Node LogicExpression()
        {
            RelationalExpression();
            ZeroOrMore(logicOperators, RelationalExpression, true);
        }

        public Node LogicOperator()
        {   
            switch (CurrentToken) {

                case TokenCategory.AND:
                    return new AndNode() { 
                        AnchorToken = Expect(TokenCategory.AND) 
                    };

                case TokenCategory.OR:
                    return new OrNode() {
                        AnchorToken = Expect(TokenCategory.OR)
                    };

                case TokenCategory.XOR:
                    return new XorNode() {
                        AnchorToken = Expect(TokenCategory.XOR)
                    };         

                default:
                    throw new SyntaxError(logicOperators, 
                                        tokenStream.Current);
            }
        }

        public Node RelationalExpression()
        {
            SumExpression();
            ZeroOrMore(relationalOperators, SumExpression, true);
        }

        public Node RelationalOperator()
            {
                switch (CurrentToken) {

                    case TokenCategory.EQUAL:
                        return new EqualNode() { 
                            AnchorToken = Expect(TokenCategory.EQUAL) 
                        };

                    case TokenCategory.UNEQUAL:
                        return new UnEqualNode() {
                            AnchorToken = Expect(TokenCategory.UNEQUAL)
                        };

                    case TokenCategory.LESS_THAN:
                        return new LessThanNode() {
                            AnchorToken = Expect(TokenCategory.LESS_THAN)
                        };

                    case TokenCategory.MORE_THAN:
                        return new MoreThanNode() {
                            AnchorToken = Expect(TokenCategory.MORE_THAN)
                        };    

                    case TokenCategory.LESS_THAN_EQUAL:
                        return new LessThanEqualNode() {
                            AnchorToken = Expect(TokenCategory.LESS_THAN_EQUAL)
                        };   

                    case TokenCategory.MORE_THAN_EQUAL:
                        return new MoreThanEqualNode() {
                            AnchorToken = Expect(TokenCategory.MORE_THAN_EQUAL)
                        };    

                    default:
                        throw new SyntaxError(relationalOperators, 
                                            tokenStream.Current);
                }
            }

        public Node SumExpression()
        {
            MulExpression();
            ZeroOrMore(sumOperators, MulExpression, true);
        }

        public Node SumOperator()
        {   
            switch(CurrentToken){

                case TokenCategory.MINUS:
                    return new MinusNode() {
                        AnchorToken = Expect(TokenCategory.MINUS)
                    }; 

                case TokenCategory.PLUS:
                    return new PlusNode() {
                        AnchorToken = Expect(TokenCategory.PLUS)
                    };      

                default:
                    throw new SyntaxError(sumOperators, 
                                        tokenStream.Current);
            }
        }

        public Node MulExpression()
        {
            UnaryExpression();
            ZeroOrMore(mulOperators, UnaryExpression, true);
        }

        public Node MulOperator()
        {
            switch(CurrentToken){

                case TokenCategory.TIMES:
                    return new TimesNode() {
                        AnchorToken = Expect(TokenCategory.TIMES)
                    }; 

                case TokenCategory.DIV:
                    return new DivNode() {
                        AnchorToken = Expect(TokenCategory.DIV)
                    };

                case TokenCategory.REM:
                    return new RemNode() {
                        AnchorToken = Expect(TokenCategory.REM)
                    };         

                default:
                    throw new SyntaxError(mulOperators, 
                                        tokenStream.Current);
            }
        }

        public Node UnaryExpression()
        {
            if (Has(unaryOperators))
            {
                Expect(unaryOperators);
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

        public Node SimpleExpression()
        {
            if (Has(TokenCategory.PARENTHESIS_OPEN))
            {
                Expect(TokenCategory.PARENTHESIS_OPEN);
                Expression();
                Expect(TokenCategory.PARENTHESIS_CLOSE);
            }
            else if (Has(TokenCategory.IDENTIFIER))
            {
                Expect(TokenCategory.IDENTIFIER);
                // May be a call
                if (Has(TokenCategory.PARENTHESIS_OPEN))
                {
                    Expect(TokenCategory.PARENTHESIS_OPEN);
                    if (Has(firstOfExpression))
                    {
                        Expression();
                        ZeroOrMore(TokenCategory.COMMA, Expression, true);
                    }
                    Expect(TokenCategory.PARENTHESIS_CLOSE);
                }
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
            }, true);
        }

    }
}
