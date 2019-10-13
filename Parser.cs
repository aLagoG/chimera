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

        public delegate Node FunctionToCall();
        public delegate List<Node> FunctionToCallMultiple();

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

        public List<Node> Optional<T>(T category, FunctionToCall onSuccess, bool expect = false)
        {
            if (Has(category))
            {
                if (expect)
                {
                    Expect(category);
                }
                return new List<Node> { onSuccess() };
            }
            return new List<Node>();
        }

        public List<Node> OptionalMultiple<T>(T category, FunctionToCallMultiple onSuccess, bool expect = false)
        {
            if (Has(category))
            {
                if (expect)
                {
                    Expect(category);
                }
                return onSuccess();
            }
            return new List<Node>();
        }

        public List<Node> ZeroOrMore<T>(T category, FunctionToCall onSuccess, bool expect = false)
        {
            var result_nodes = new List<Node>();
            while (Has(category))
            {
                if (expect)
                {
                    Expect(category);
                }
                result_nodes.Add(onSuccess());
            }
            return result_nodes;
        }


        public List<Node> OneOrMore<T>(T category, FunctionToCall onSuccess, bool expect = false)
        {
            var result_nodes = new List<Node>();
            do
            {
                if (expect)
                {
                    Expect(category);
                }
                result_nodes.Add(onSuccess());
            } while (Has(category));
            return result_nodes;
        }

        public Node Program()
        {
            var program_node = new ProgramNode();
            var identifiers_node = new IdentifierNode();
            identifiers_node.AddMultiple(OptionalMultiple(TokenCategory.CONST, () =>
            {
                return OneOrMore(TokenCategory.IDENTIFIER, ConstantDeclaration, true);
            }));

            var variables_node = new VariableDeclarationNode();
            variables_node.AddMultiple(OptionalMultiple(TokenCategory.VAR, () =>
            {
                return OneOrMore(TokenCategory.IDENTIFIER, VariableDeclaration);
            }, true));
            program_node.Add(variables_node);

            var procedures_node = new ProcedureDeclarationNode();
            procedures_node.AddMultiple(ZeroOrMore(TokenCategory.PROCEDURE, ProcedureDeclaration));
            program_node.Add(procedures_node);

            program_node.AnchorToken = Expect(TokenCategory.PROGRAM);

            var statements_node = new StatementNode();
            statements_node.AddMultiple(ZeroOrMore(firstOfStatement, Statement));
            program_node.Add(statements_node);

            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return program_node;
        }

        public Node ConstantDeclaration()
        {
            var constant_node = new ConstantDeclarationNode()
            {
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            };
            Expect(TokenCategory.COLON_EQUAL);
            constant_node.Add(Literal());
            Expect(TokenCategory.SEMICOLON);
            return constant_node;
        }

        public Node VariableDeclaration()
        {
            var variable_node = new VariableDeclarationNode();
            variable_node.Add(IdentifierNodeGenerator());
            var nodes = ZeroOrMore(TokenCategory.COMMA, () =>
                        {
                            return IdentifierNodeGenerator();
                        }, true);
            variable_node.AddMultiple(nodes);
            Expect(TokenCategory.COLON);
            variable_node.Add(Type());
            Expect(TokenCategory.SEMICOLON);
            return variable_node;
        }

        public Node Literal()
        {
            var literal_node = new LiteralNode();
            if (Has(TokenCategory.CURLY_OPEN))
            {
                literal_node.Add(List());
                return literal_node;
            }
            else if (Has(simpleLiterals))
            {
                literal_node.Add(SimpleLiteral());
                return literal_node;
            }
            else
            {
                throw new SyntaxError(firstOfLiteral, tokenStream.Current);
            }
        }

        public Node SimpleLiteral()
        {
            switch (CurrentToken)
            {

                case TokenCategory.INT_LITERAL:
                    return new IntLiteralNode()
                    {
                        AnchorToken = Expect(TokenCategory.INT_LITERAL)
                    };

                case TokenCategory.STRING_LITERAL:
                    return new StringLiteralNode()
                    {
                        AnchorToken = Expect(TokenCategory.STRING_LITERAL)
                    };

                case TokenCategory.TRUE:
                    return new TrueNode()
                    {
                        AnchorToken = Expect(TokenCategory.TRUE)
                    };

                case TokenCategory.FALSE:
                    return new FalseNode()
                    {
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
            switch (CurrentToken)
            {

                case TokenCategory.INTEGER:
                    return new IntegerNode()
                    {
                        AnchorToken = Expect(TokenCategory.INTEGER)
                    };

                case TokenCategory.STRING:
                    return new StringNode()
                    {
                        AnchorToken = Expect(TokenCategory.STRING)
                    };

                case TokenCategory.BOOLEAN:
                    return new BooleanNode()
                    {
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
            var list_node = new ListNode();
            Expect(TokenCategory.CURLY_OPEN);
            var nodes = OptionalMultiple(simpleLiterals, () =>
                        {
                            return OneOrMore(TokenCategory.COMMA, SimpleLiteral, true);
                        }, true);
            list_node.AddMultiple(nodes);
            Expect(TokenCategory.CURLY_CLOSE);
            return list_node;
        }

        public Node ProcedureDeclaration()
        {
            Expect(TokenCategory.PROCEDURE);
            var procedure_node = new ProcedureDeclarationNode()
            {
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            };
            Expect(TokenCategory.PARENTHESIS_OPEN);
            var parameters_node = new ParameterDeclarationNode();
            parameters_node.AddMultiple(ZeroOrMore(TokenCategory.IDENTIFIER, ParameterDeclaration));
            procedure_node.Add(parameters_node);

            Expect(TokenCategory.PARENTHESIS_CLOSE);

            var types_node = new TypeNode();
            types_node.AddMultiple(Optional(TokenCategory.COLON, Type, true));
            procedure_node.Add(types_node);

            Expect(TokenCategory.SEMICOLON);
            var constants_node = new ConstantDeclarationNode();
            constants_node.AddMultiple(OptionalMultiple(TokenCategory.CONST, () =>
                                        {
                                            return OneOrMore(TokenCategory.IDENTIFIER, ConstantDeclaration);
                                        }, true));
            procedure_node.Add(constants_node);

            var variable_node = new VariableDeclarationNode();
            variable_node.AddMultiple(OptionalMultiple(TokenCategory.VAR, () =>
                                        {
                                            return OneOrMore(TokenCategory.IDENTIFIER, VariableDeclaration);
                                        }, true));
            variable_node.Add(variable_node);

            Expect(TokenCategory.BEGIN);

            var statements_node = new StatementNode();
            statements_node.AddMultiple(ZeroOrMore(firstOfStatement, Statement));
            procedure_node.Add(statements_node);

            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);

            return procedure_node;
        }

        public Node IdentifierNodeGenerator()
        {
            return new IdentifierNode()
            {
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            };
        }
        public Node ParameterDeclaration()
        {
            var parameter_node = new ParameterDeclarationNode();
            parameter_node.Add(IdentifierNodeGenerator());
            parameter_node.AddMultiple(ZeroOrMore(TokenCategory.COMMA, () =>
                                        {
                                            return IdentifierNodeGenerator();
                                        }, true));
            Expect(TokenCategory.COLON);
            parameter_node.Add(Type());
            Expect(TokenCategory.SEMICOLON);

            return parameter_node;
        }

        public Node Statement()
        {
            if (Has(TokenCategory.IDENTIFIER))
            {
                Expect(TokenCategory.IDENTIFIER);
                return AssignmentOrCallStatement();
            }
            else if (Has(TokenCategory.IF))
            {
                return IfStatement();
            }
            else if (Has(TokenCategory.LOOP))
            {
                return LoopStatement();
            }
            else if (Has(TokenCategory.FOR))
            {
                return ForStatement();
            }
            else if (Has(TokenCategory.RETURN))
            {
                return ReturnStatement();
            }
            else if (Has(TokenCategory.EXIT))
            {
                return ExitStatement();
            }
            else
            {
                throw new SyntaxError(firstOfStatement, tokenStream.Current);
            }
        }

        public Node MultipleExpressionAndComma()
        {
            var expression = Expression();
            expression.AddMultiple(ZeroOrMore(TokenCategory.COMMA, Expression, true));
            return expression;
        }

        public Node ExpressionAndBracketClose()
        {
            var expression = Expression();
            Expect(TokenCategory.BRACKET_CLOSE);
            return expression;
        }
        public Node AssignmentOrCallStatement()
        {
            var assignment_call_node = new AssignmentOrCallStatementNode();
            if (Has(TokenCategory.PARENTHESIS_OPEN))
            {
                Expect(TokenCategory.PARENTHESIS_OPEN);
                assignment_call_node.AddMultiple(Optional(firstOfExpression, () =>
                                                {
                                                    return MultipleExpressionAndComma();
                                                }));
                Expect(TokenCategory.PARENTHESIS_CLOSE);
                Expect(TokenCategory.SEMICOLON);
            }
            else if (Has(TokenCategory.BRACKET_OPEN) || Has(TokenCategory.COLON_EQUAL))
            {
                assignment_call_node.AddMultiple(Optional(TokenCategory.BRACKET_OPEN, () =>
                                                {
                                                    return ExpressionAndBracketClose();
                                                }, true));
                Expect(TokenCategory.COLON_EQUAL);
                assignment_call_node.Add(Expression());
                Expect(TokenCategory.SEMICOLON);
            }
            else
            {
                throw new SyntaxError(firstOfAssignmentOrCallStatement, tokenStream.Current);
            }
            return assignment_call_node;
        }

        public Node ElifStatement()
        {
            var elif_node = new ElifStatementNode()
            {
                AnchorToken = Expect(TokenCategory.ELSEIF)
            };
            elif_node.Add(Expression());
            Expect(TokenCategory.THEN);
            elif_node.AddMultiple(ZeroOrMore(firstOfStatement, Statement));
            return elif_node;
        }

        public Node ElseStatement()
        {
            var else_node = new ElseStatementNode()
            {
                AnchorToken = Expect(TokenCategory.ELSE)
            };
            else_node.AddMultiple(ZeroOrMore(firstOfStatement, Statement));
            return else_node;
        }
        public Node IfStatement()
        {
            var if_node = new IfStatementNode()
            {
                AnchorToken = Expect(TokenCategory.IF)
            };
            if_node.Add(Expression());
            Expect(TokenCategory.THEN);
            var if_statement_node = new StatementNode();
            if_statement_node.AddMultiple(ZeroOrMore(firstOfStatement, Statement));
            if_node.Add(if_statement_node);

            if_node.AddMultiple(ZeroOrMore(TokenCategory.ELSEIF, () =>
                                {
                                    return ElifStatement();
                                }, false));

            if (Has(TokenCategory.ELSE))
            {
                if_node.Add(ElseStatement());
            }

            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return if_node;
        }

        public Node LoopStatement()
        {
            var loop_node = new LoopStatementNode()
            {
                AnchorToken = Expect(TokenCategory.LOOP)
            };
            loop_node.AddMultiple(ZeroOrMore(firstOfStatement, Statement));
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return loop_node;
        }

        public Node ForStatement()
        {
            var for_node = new ForStatementNode()
            {
                AnchorToken = Expect(TokenCategory.FOR)
            };
            for_node.Add(new IdentifierNode()
            {
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            });
            Expect(TokenCategory.IN);
            for_node.Add(Expression());
            Expect(TokenCategory.DO);
            var statement_node = new StatementNode();
            statement_node.AddMultiple(ZeroOrMore(firstOfStatement, Statement));
            for_node.Add(statement_node);
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return for_node;
        }

        public Node ReturnStatement()
        {
            var return_node = new ReturnStatementNode()
            {
                AnchorToken = Expect(TokenCategory.RETURN)
            };
            var expression_node = Expression();
            expression_node.AddMultiple(Optional(firstOfExpression, Expression));
            return_node.Add(expression_node);
            Expect(TokenCategory.SEMICOLON);
            return return_node;
        }

        public Node ExitStatement()
        {
            if (CurrentToken == TokenCategory.EXIT)
            {
                var exit_node = new ExitNode()
                {
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
            return LogicExpression();
        }

        public Node LogicExpression()
        {
            var logic_node = new LogicalExpressionNode();
            logic_node.Add(RelationalExpression());
            logic_node.AddMultiple(ZeroOrMore(logicOperators, RelationalExpression, true));
            return logic_node;
        }

        public Node LogicOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.AND:
                    return new AndNode()
                    {
                        AnchorToken = Expect(TokenCategory.AND)
                    };

                case TokenCategory.OR:
                    return new OrNode()
                    {
                        AnchorToken = Expect(TokenCategory.OR)
                    };

                case TokenCategory.XOR:
                    return new XorNode()
                    {
                        AnchorToken = Expect(TokenCategory.XOR)
                    };

                default:
                    throw new SyntaxError(logicOperators,
                                        tokenStream.Current);
            }
        }

        public Node RelationalExpression()
        {
            var relational_node = new RelationalExpressionNode();
            relational_node.Add(SumExpression());
            relational_node.AddMultiple(ZeroOrMore(relationalOperators, SumExpression, true));
            return relational_node;
        }

        public Node RelationalOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.EQUAL:
                    return new EqualNode()
                    {
                        AnchorToken = Expect(TokenCategory.EQUAL)
                    };

                case TokenCategory.UNEQUAL:
                    return new UnEqualNode()
                    {
                        AnchorToken = Expect(TokenCategory.UNEQUAL)
                    };

                case TokenCategory.LESS_THAN:
                    return new LessThanNode()
                    {
                        AnchorToken = Expect(TokenCategory.LESS_THAN)
                    };

                case TokenCategory.MORE_THAN:
                    return new MoreThanNode()
                    {
                        AnchorToken = Expect(TokenCategory.MORE_THAN)
                    };

                case TokenCategory.LESS_THAN_EQUAL:
                    return new LessThanEqualNode()
                    {
                        AnchorToken = Expect(TokenCategory.LESS_THAN_EQUAL)
                    };

                case TokenCategory.MORE_THAN_EQUAL:
                    return new MoreThanEqualNode()
                    {
                        AnchorToken = Expect(TokenCategory.MORE_THAN_EQUAL)
                    };

                default:
                    throw new SyntaxError(relationalOperators,
                                        tokenStream.Current);
            }
        }

        public Node SumExpression()
        {
            var sum_node = new SumExpressionNode();
            sum_node.Add(MulExpression());
            sum_node.AddMultiple(ZeroOrMore(sumOperators, MulExpression, true));
            return sum_node;
        }

        public Node SumOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.MINUS:
                    return new MinusNode()
                    {
                        AnchorToken = Expect(TokenCategory.MINUS)
                    };

                case TokenCategory.PLUS:
                    return new PlusNode()
                    {
                        AnchorToken = Expect(TokenCategory.PLUS)
                    };

                default:
                    throw new SyntaxError(sumOperators,
                                        tokenStream.Current);
            }
        }

        public Node MulExpression()
        {
            var mul_node = new MulExpessionNode();
            mul_node.Add(UnaryExpression());
            mul_node.AddMultiple(ZeroOrMore(mulOperators, UnaryExpression, true));
            return mul_node;
        }

        public Node MulOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.TIMES:
                    return new TimesNode()
                    {
                        AnchorToken = Expect(TokenCategory.TIMES)
                    };

                case TokenCategory.DIV:
                    return new DivNode()
                    {
                        AnchorToken = Expect(TokenCategory.DIV)
                    };

                case TokenCategory.REM:
                    return new RemNode()
                    {
                        AnchorToken = Expect(TokenCategory.REM)
                    };

                default:
                    throw new SyntaxError(mulOperators,
                                        tokenStream.Current);
            }
        }

        public Node UnaryExpression()
        {
            var unary_expression = new UnaryExpressionNode();
            if (Has(unaryOperators))
            {
                unary_expression.AnchorToken = Expect(unaryOperators);
                unary_expression.Add(UnaryExpression());
                return unary_expression;
            }
            else if (Has(firstOfSimpleExpression))
            {
                unary_expression.Add(SimpleExpression());
                return unary_expression;
            }
            else
            {
                throw new SyntaxError(firstOfUnaryExpression, tokenStream.Current);
            }
        }

        public Node SimpleExpression()
        {
            var simple_node = new SimpleExpressionNode();
            if (Has(TokenCategory.PARENTHESIS_OPEN))
            {
                Expect(TokenCategory.PARENTHESIS_OPEN);
                simple_node.Add(Expression());
                Expect(TokenCategory.PARENTHESIS_CLOSE);
            }
            else if (Has(TokenCategory.IDENTIFIER))
            {
                simple_node.Add(IdentifierNodeGenerator());
                // May be a call
                if (Has(TokenCategory.PARENTHESIS_OPEN))
                {
                    Expect(TokenCategory.PARENTHESIS_OPEN);
                    if (Has(firstOfExpression))
                    {
                        var expressions_node = Expression();
                        expressions_node.AddMultiple(ZeroOrMore(TokenCategory.COMMA, Expression, true));
                        simple_node.Add(expressions_node);
                    }
                    Expect(TokenCategory.PARENTHESIS_CLOSE);
                }
            }
            else if (Has(firstOfLiteral))
            {
                simple_node.Add(Literal());
            }
            else
            {
                throw new SyntaxError(firstOfSimpleExpression, tokenStream.Current);
            }

            if (Has(TokenCategory.PARENTHESIS_OPEN))
            {
                Expect(TokenCategory.BRACKET_OPEN);
                simple_node.Add(Expression());
                Expect(TokenCategory.BRACKET_CLOSE);
            }
            return simple_node;
        }

    }
}
