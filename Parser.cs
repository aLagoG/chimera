/*
Chimera
Date: 2-Dic-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System;
using System.Linq;
using System.Collections.Generic;

namespace Chimera
{

    class Parser
    {

        #region dictionaries

        static readonly HashSet<TokenCategory> simpleTypes =
            new HashSet<TokenCategory>() {
                TokenCategory.INTEGER,
                TokenCategory.STRING,
                TokenCategory.BOOLEAN
            };

        static readonly HashSet<TokenCategory> simpleLiterals =
            new HashSet<TokenCategory>() {
                TokenCategory.INT_LITERAL,
                TokenCategory.STRING_LITERAL,
                TokenCategory.TRUE,
                TokenCategory.FALSE
            };

        static readonly HashSet<TokenCategory> logicOperators =
            new HashSet<TokenCategory>() {
                TokenCategory.AND,
                TokenCategory.OR,
                TokenCategory.XOR
            };

        static readonly HashSet<TokenCategory> relationalOperators =
            new HashSet<TokenCategory>() {
                TokenCategory.EQUAL,
                TokenCategory.UNEQUAL,
                TokenCategory.LESS_THAN,
                TokenCategory.MORE_THAN,
                TokenCategory.LESS_THAN_EQUAL,
                TokenCategory.MORE_THAN_EQUAL,
            };

        static readonly HashSet<TokenCategory> sumOperators =
            new HashSet<TokenCategory>() {
                TokenCategory.PLUS,
                TokenCategory.MINUS,
            };

        static readonly HashSet<TokenCategory> mulOperators =
            new HashSet<TokenCategory>() {
                TokenCategory.TIMES,
                TokenCategory.DIV,
                TokenCategory.REM,
            };

        static readonly HashSet<TokenCategory> firstOfLiteral;

        static readonly HashSet<TokenCategory> firstOfType;

        static readonly HashSet<TokenCategory> unaryOperators =
            new HashSet<TokenCategory>() { TokenCategory.NOT, TokenCategory.MINUS };

        static readonly HashSet<TokenCategory> firstOfStatement =
            new HashSet<TokenCategory>() {
                TokenCategory.IDENTIFIER,
                TokenCategory.IF,
                TokenCategory.LOOP,
                TokenCategory.FOR,
                TokenCategory.RETURN,
                TokenCategory.EXIT
            };

        static readonly HashSet<TokenCategory> firstOfUnaryExpression;

        static readonly HashSet<TokenCategory> firstOfSimpleExpression;

        static readonly HashSet<TokenCategory> firstOfExpression;

        static readonly HashSet<TokenCategory> firstOfAssignmentOrCallStatement =
            new HashSet<TokenCategory>() {
                TokenCategory.PARENTHESIS_OPEN,
                TokenCategory.BRACKET_OPEN,
                TokenCategory.COLON_EQUAL
            };

        #endregion

        #region miscellaneous

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

        public delegate Node SingleNodeCallback();
        public delegate List<Node> MultiNodeCallback();

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
            var tokenSet = category as HashSet<TokenCategory>;
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
                var tokenSet = category as HashSet<TokenCategory>;
                if (tokenSet != null)
                {
                    throw new SyntaxError(tokenSet, tokenStream.Current);
                }
                throw new NotImplementedException($"SyntaxError is not implemented for type {typeof(T).FullName}");
            }
        }

        public Node Optional<T>(T category, SingleNodeCallback onSuccess, bool expect = false)
        {
            if (Has(category))
            {
                if (expect)
                {
                    Expect(category);
                }
                return onSuccess();
            }
            return null;
        }

        public List<Node> Optional<T>(T category, MultiNodeCallback onSuccess, bool expect = false)
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

        public List<Node> ZeroOrMore<T>(T category, SingleNodeCallback onSuccess, bool expect = false)
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

        // Rename to the actual name of this concept when I remember what was it
        public Node LeftJoin<T>(Node leftmostNode, T category, SingleNodeCallback nextNodeCallback)
        {
            var lastNode = leftmostNode;
            while (Has(category))
            {
                var node = Node.fromToken(Expect(category));
                node.Add(lastNode);
                node.Add(nextNodeCallback());
                lastNode = node;
            }
            return lastNode;
        }


        public List<Node> OneOrMore<T>(T category, SingleNodeCallback onSuccess, bool expect = false)
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

        #endregion

        #region productions

        public Node Program()
        {
            var program_node = new ProgramNode();

            program_node.Add(new ConstantListNode());
            if (Has(TokenCategory.CONST))
            {
                var node = program_node.Last();
                node.AnchorToken = Expect(TokenCategory.CONST);
                node.Add(OneOrMore(TokenCategory.IDENTIFIER, ConstantDeclaration));
            };

            program_node.Add(new VariableDeclarationNode());
            if (Has(TokenCategory.VAR))
            {
                var node = program_node.Last();
                node.AnchorToken = Expect(TokenCategory.VAR);
                node.Add(OneOrMore(TokenCategory.IDENTIFIER, VariableDeclaration));
            }

            program_node.Add(new ProcedureListNode());
            if (Has(TokenCategory.PROCEDURE))
            {
                var node = program_node.Last();
                node.Add(ZeroOrMore(TokenCategory.PROCEDURE, ProcedureDeclaration));
            }

            program_node.AnchorToken = Expect(TokenCategory.PROGRAM);

            program_node.Add(new StatementListNode());
            if (Has(firstOfStatement))
            {
                var node = program_node.Last();
                node.Add(ZeroOrMore(firstOfStatement, Statement));
            }

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
            var nodes = new List<Node>();
            nodes.Add(Node.fromToken(Expect(TokenCategory.IDENTIFIER)));
            nodes.AddRange(ZeroOrMore(TokenCategory.COMMA, () =>
                        {
                            return Node.fromToken(Expect(TokenCategory.IDENTIFIER));
                        }, true));
            Expect(TokenCategory.COLON);
            var typeNode = Type();
            Expect(TokenCategory.SEMICOLON);
            typeNode.Add(nodes);
            return typeNode;
        }

        public Node Literal()
        {
            if (Has(TokenCategory.CURLY_OPEN))
            {
                return List();
            }
            else if (Has(simpleLiterals))
            {
                return SimpleLiteral();
            }
            else
            {
                throw new SyntaxError(firstOfLiteral, tokenStream.Current);
            }
        }

        public Node SimpleLiteral()
        {
            return Node.fromToken(Expect(simpleLiterals));
        }

        public Node Type()
        {
            if (CurrentToken == TokenCategory.LIST)
            {
                return ListType();
            }
            else if (simpleTypes.Contains(CurrentToken))
            {
                return SimpleType();
            }
            else
            {
                throw new SyntaxError(firstOfType, tokenStream.Current);
            }
        }

        public Node SimpleType()
        {
            return Node.fromToken(Expect(simpleTypes));
        }

        public Node ListType()
        {
            Expect(TokenCategory.LIST);
            Expect(TokenCategory.OF);
            return new ListTypeNode()
            {
                AnchorToken = SimpleType().AnchorToken
            };
        }

        public Node List()
        {
            var list_node = new ListLiteralNode();
            Expect(TokenCategory.CURLY_OPEN);
            var nodes = Optional(simpleLiterals, () =>
                        {
                            list_node.Add(Node.fromToken(Expect(simpleLiterals)));
                            return ZeroOrMore(TokenCategory.COMMA, SimpleLiteral, true);
                        });
            list_node.Add(nodes);
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

            procedure_node.Add(new ParameterDeclarationNode()
            {
                ZeroOrMore(TokenCategory.IDENTIFIER, ParameterDeclaration)
            });

            Expect(TokenCategory.PARENTHESIS_CLOSE);

            var typeNode = Optional(TokenCategory.COLON, Type, true);
            if (typeNode == null)
            {
                typeNode = new VoidTypeNode();
            }
            procedure_node.Add(typeNode);

            Expect(TokenCategory.SEMICOLON);
            procedure_node.Add(Optional(TokenCategory.CONST, () =>
            {
                var node = new ConstantListNode()
                {
                    AnchorToken = Expect(TokenCategory.CONST)
                };
                node.Add(OneOrMore(TokenCategory.IDENTIFIER, ConstantDeclaration));
                return node;
            }));

            procedure_node.Add(Optional(TokenCategory.VAR, () =>
            {
                var node = new VariableDeclarationNode()
                {
                    AnchorToken = Expect(TokenCategory.VAR)
                };
                node.Add(OneOrMore(TokenCategory.IDENTIFIER, VariableDeclaration));
                return node;
            }));

            Expect(TokenCategory.BEGIN);

            procedure_node.Add(new StatementListNode()
            {
                ZeroOrMore(firstOfStatement, Statement)
            });

            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);

            return procedure_node;
        }

        public Node ParameterDeclaration()
        {
            var nodes = new List<Node>();
            nodes.Add(Node.fromToken(Expect(TokenCategory.IDENTIFIER)));
            nodes.AddRange(ZeroOrMore(TokenCategory.COMMA, () =>
                        {
                            return Node.fromToken(Expect(TokenCategory.IDENTIFIER));
                        }, true));
            Expect(TokenCategory.COLON);
            var typeNode = Type();
            Expect(TokenCategory.SEMICOLON);
            typeNode.Add(nodes);
            return typeNode;
        }

        public Node Statement()
        {
            if (Has(TokenCategory.IDENTIFIER))
            {
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

        public Node AssignmentOrCallStatement()
        {
            var id_node = Node.fromToken(Expect(TokenCategory.IDENTIFIER));
            Node node = null;
            if (Has(TokenCategory.PARENTHESIS_OPEN))
            {
                node = new CallStatementNode()
                {
                    AnchorToken = id_node.AnchorToken
                };
                Expect(TokenCategory.PARENTHESIS_OPEN);
                node.Add(Optional(firstOfExpression, () =>
                                                {
                                                    var list = new List<Node>() { Expression() };
                                                    list.AddRange(ZeroOrMore(TokenCategory.COMMA, Expression, true));
                                                    return list;
                                                }));
                Expect(TokenCategory.PARENTHESIS_CLOSE);
            }
            else if (Has(TokenCategory.BRACKET_OPEN) || Has(TokenCategory.COLON_EQUAL))
            {
                node = new AssignmentNode();
                if (Has(TokenCategory.BRACKET_OPEN))
                {
                    Expect(TokenCategory.BRACKET_OPEN);
                    var expression_node = Expression();
                    Expect(TokenCategory.BRACKET_CLOSE);
                    node.Add(new ListIndexNode() { id_node, expression_node });
                }
                else
                {
                    node.Add(id_node);
                }
                node.AnchorToken = Expect(TokenCategory.COLON_EQUAL);
                node.Add(Expression());
            }
            else
            {
                throw new SyntaxError(firstOfAssignmentOrCallStatement, tokenStream.Current);
            }
            Expect(TokenCategory.SEMICOLON);
            return node;
        }

        public Node ElifStatement()
        {
            var elif_node = new ElifStatementNode()
            {
                AnchorToken = Expect(TokenCategory.ELSEIF)
            };
            elif_node.Add(Expression());
            Expect(TokenCategory.THEN);
            if (Has(firstOfStatement))
            {
                elif_node.Add(new StatementListNode() { ZeroOrMore(firstOfStatement, Statement) });
            }
            return elif_node;
        }

        public Node ElseStatement()
        {
            var else_node = new ElseStatementNode()
            {
                AnchorToken = Expect(TokenCategory.ELSE)
            };
            else_node.Add(ZeroOrMore(firstOfStatement, Statement));
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
            if_node.Add(new StatementListNode(){
                ZeroOrMore(firstOfStatement, Statement)
            });
            if (Has(TokenCategory.ELSEIF))
            {
                if_node.Add(new ElseIfListNode(){
                    ZeroOrMore(TokenCategory.ELSEIF, () =>
                                    {
                                        return ElifStatement();
                                    })
                });
            }

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
            loop_node.Add(ZeroOrMore(firstOfStatement, Statement));
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
            for_node.Add(new StatementListNode(){
                ZeroOrMore(firstOfStatement, Statement)
            });
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
            return_node.Add(Optional(firstOfExpression, Expression));
            Expect(TokenCategory.SEMICOLON);
            return return_node;
        }

        public Node ExitStatement()
        {
            var exit_node = new ExitNode()
            {
                AnchorToken = Expect(TokenCategory.EXIT)
            };
            Expect(TokenCategory.SEMICOLON);
            return exit_node;
        }

        public Node Expression()
        {
            return LogicExpression();
        }

        public Node LogicExpression()
        {
            return LeftJoin(RelationalExpression(), logicOperators, RelationalExpression);
        }

        public Node LogicOperator()
        {
            return Node.fromToken(Expect(logicOperators));
        }

        public Node RelationalExpression()
        {
            return LeftJoin(SumExpression(), relationalOperators, SumExpression);
        }

        public Node RelationalOperator()
        {
            return Node.fromToken(Expect(relationalOperators));
        }

        public Node SumExpression()
        {
            return LeftJoin(MulExpression(), sumOperators, MulExpression);
        }

        public Node SumOperator()
        {
            return Node.fromToken(Expect(mulOperators));
        }

        public Node MulExpression()
        {
            return LeftJoin(UnaryExpression(), mulOperators, UnaryExpression);
        }

        public Node MulOperator()
        {
            return Node.fromToken(Expect(mulOperators));
        }

        public Node UnaryExpression()
        {
            if (Has(unaryOperators))
            {
                var node = Node.fromToken(Expect(unaryOperators));
                node.Add(UnaryExpression());
                return node;
            }
            else if (Has(firstOfSimpleExpression))
            {
                return SimpleExpression();
            }
            else
            {
                throw new SyntaxError(firstOfUnaryExpression, tokenStream.Current);
            }
        }

        public Node SimpleExpression()
        {
            Node node = null;
            if (Has(TokenCategory.PARENTHESIS_OPEN))
            {
                Expect(TokenCategory.PARENTHESIS_OPEN);
                node = Expression();
                Expect(TokenCategory.PARENTHESIS_CLOSE);
            }
            else if (Has(TokenCategory.IDENTIFIER))
            {
                node = Node.fromToken(Expect(TokenCategory.IDENTIFIER));
                // May be a call
                if (Has(TokenCategory.PARENTHESIS_OPEN))
                {
                    var call_node = new CallNode()
                    {
                        AnchorToken = node.AnchorToken
                    };
                    Expect(TokenCategory.PARENTHESIS_OPEN);
                    if (Has(firstOfExpression))
                    {
                        call_node.Add(Expression());
                        call_node.Add(ZeroOrMore(TokenCategory.COMMA, Expression, true));
                    }
                    Expect(TokenCategory.PARENTHESIS_CLOSE);
                    node = call_node;
                }
            }
            else if (Has(firstOfLiteral))
            {
                node = Literal();
            }
            else
            {
                throw new SyntaxError(firstOfSimpleExpression, tokenStream.Current);
            }

            if (Has(TokenCategory.BRACKET_OPEN))
            {
                Expect(TokenCategory.BRACKET_OPEN);
                var indexNode = new ListIndexNode() { node, Expression() };
                Expect(TokenCategory.BRACKET_CLOSE);
                return indexNode;
            }
            return node;
        }

        #endregion
    }
}
