/*
Chimera
Date: 11-Nov-2019
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

    class CILGenerator
    {
        public SymbolTable symbolTable
        {
            get;
            private set;
        }

        public ProcedureTable procedureTable
        {
            get;
            private set;
        }

        private string currentScope = "";

        private bool inLoopOrFor = false;

        public CILGenerator(SymbolTable symbolTable, ProcedureTable procedureTable)
        {
            this.symbolTable = symbolTable;
            this.procedureTable = procedureTable;
        }

        public String Visit(ProgramNode node)
        {
            return "";
            VisitChildren(node);
            // return Type.VOID;
            return "";
        }
        public String Visit(StatementListNode node)
        {
            VisitChildren(node);
            // return Type.VOID;
            return "";
        }

        public String Visit(AndNode node)
        {
            VisitBinaryOperator(node, Type.BOOL);
            // return Type.BOOL;
            return "";
        }
        public String Visit(OrNode node)
        {
            VisitBinaryOperator(node, Type.BOOL);
            // return Type.BOOL;
            return "";
        }
        public String Visit(XorNode node)
        {
            VisitBinaryOperator(node, Type.BOOL);
            // return Type.BOOL;
            return "";
        }
        public String Visit(NotNode node)
        {
            if (Visit((dynamic)node[0]) != Type.BOOL)
            {
                throw new SemanticError(
                    $"Operator {node.AnchorToken.Lexeme} requires an operand of type {Type.BOOL}",
                    node.AnchorToken);
            }
            // return Type.BOOL;
            return "";
        }

        public String Visit(EqualNode node)
        {
            VisitBinaryIntOrBoolOperator(node);
            // return Type.BOOL;
            return "";
        }
        public String Visit(UnequalNode node)
        {
            VisitBinaryIntOrBoolOperator(node);
            // return Type.BOOL;
            return "";
        }

        public String Visit(LessThanNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            // return Type.BOOL;
            return "";
        }
        public String Visit(MoreThanNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            // return Type.BOOL;
            return "";
        }
        public String Visit(LessThanEqualNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            // return Type.BOOL;
            return "";
        }
        public String Visit(MoreThanEqualNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            // return Type.BOOL;
            return "";
        }

        public String Visit(MinusNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            // return Type.INT;
            return "";
        }
        public String Visit(PlusNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            // return Type.INT;
            return "";
        }
        public String Visit(TimesNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            // return Type.INT;
            return "";
        }
        public String Visit(DivNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            // return Type.INT;
            return "";
        }
        public String Visit(RemNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            // return Type.INT;
            return "";
        }

        public String Visit(IntegerNode node)
        {
            // return Type.INT;
            return "";
        }
        public String Visit(StringNode node)
        {
            // return Type.STRING;
            return "";
        }
        public String Visit(BooleanNode node)
        {
            // return Type.BOOL;
            return "";
        }
        public String Visit(VoidTypeNode node)
        {
            // return Type.VOID;
            return "";
        }
        public String Visit(ListTypeNode node)
        {
            // return typeMapper[node.AnchorToken.Category].ToListType();
            return "";
        }

        public String Visit(IntLiteralNode node)
        {
            var intStr = node.AnchorToken.Lexeme;
            try
            {
                Convert.ToInt32(intStr);

            }
            catch (OverflowException)
            {
                throw new SemanticError(
                    $"Integer literal too large: {intStr}",
                    node.AnchorToken);
            }

            // return Type.INT;
            return "";
        }
        public String Visit(StringLiteralNode node)
        {
            // return Type.STRING;
            return "";
        }
        public String Visit(BoolLiteralNode node)
        {
            // return Type.BOOL;
            return "";
        }
        public String Visit(ListLiteralNode node)
        {
            if (node.Count() == 0)
            {
                // return Type.LIST;
                return "";
            }
            Type first = Visit((dynamic)node[0]);
            foreach (var n in node)
            {
                Type t = Visit((dynamic)n);
                if (t != first)
                {
                    throw new SemanticError("All elements of a list should be the same tipe, "
                                            + $"expected {first} but got {t}", n.AnchorToken);
                }
            }
            // return first.ToListType();
            return "";
        }

        public String Visit(ListIndexNode node)
        {
            Type type = Visit((dynamic)node[0]);
            Visit((dynamic)node[1]);
            // return type.FromListType();
            return "";
        }

        public String Visit(ConstantListNode node)
        {
            VisitChildren(node);
            // return Type.VOID;
            return "";
        }
        public String Visit(ConstantDeclarationNode node)
        {
            var varName = node.AnchorToken.Lexeme;
            if (CurrentScopeHasSymbol(varName))
            {
                throw new SemanticError($"Duplicated constant: {varName}",
                                        node.AnchorToken);
            }
            Type type = Visit((dynamic)node[0]);
            if (type == Type.LIST)
            {
                throw new SemanticError($"List constants should have at least one element",
                            node.AnchorToken);
            }
            AddSymbolToScope(varName, type, Kind.CONST);
            // return type;
            return "";
        }
        public String Visit(VariableDeclarationNode node)
        {
            foreach (var typeNode in node)
            {
                Type type = Visit((dynamic)typeNode);
                foreach (var idNode in typeNode)
                {
                    var varName = idNode.AnchorToken.Lexeme;
                    if (CurrentScopeHasSymbol(varName))
                    {
                        throw new SemanticError($"Duplicated variable: {varName}",
                                                idNode.AnchorToken);
                    }
                    else
                    {
                        AddSymbolToScope(varName, type, Kind.VAR);
                    }
                }
            }
            // return Type.VOID;
            return "";
        }
        public String Visit(AssignmentNode node)
        {
            Type type1 = Visit((dynamic)node[0]);
            Type type2 = Visit((dynamic)node[1]);
            if (!type1.CompatibleWith(type2))
            {
                throw new SemanticError($"Cannot assign a value of type {type2} to a variable of type {type1}",
                    node.AnchorToken);
            }
            // return Type.VOID;
            return "";
        }
        public String Visit(IdentifierNode node)
        {
            var variableName = node.AnchorToken.Lexeme;
            var symbol = GetSymbol(variableName);
            if (symbol != null)
            {
                // return symbol.type;
                return "";
            }

            throw new SemanticError(
                $"Undeclared variable or constant: {variableName}",
                node.AnchorToken);
        }

        public String Visit(LoopStatementNode node)
        {
            var lastInLoopOrFor = inLoopOrFor;
            inLoopOrFor = true;

            VisitChildren(node);

            inLoopOrFor = lastInLoopOrFor;
            // return Type.VOID;
            return "";
        }
        public String Visit(ForStatementNode node)
        {
            Type varType = Visit((dynamic)node[0]);
            Type listType = Visit((dynamic)node[1]);
            if (varType.ToListType() != listType)
            {
                throw new SemanticError($"Incompatible types {varType} and {listType}",
                    node[0].AnchorToken);
            }
            var lastInLoopOrFor = inLoopOrFor;
            inLoopOrFor = true;

            Visit((dynamic)node[2]);

            inLoopOrFor = lastInLoopOrFor;
            // return Type.VOID;
            return "";
        }
        public String Visit(ExitNode node)
        {
            if (!inLoopOrFor)
            {
                throw new SemanticError("Unexpected exit statement", node.AnchorToken);
            }
            // return Type.VOID;
            return "";
        }

        public String Visit(IfStatementNode node)
        {
            VerifyCondition(node);
            VisitChildren(node, 1);
            // return Type.VOID;
            return "";
        }
        public String Visit(ElseIfListNode node)
        {
            VisitChildren(node);
            // return Type.VOID;
            return "";
        }
        public String Visit(ElifStatementNode node)
        {
            VerifyCondition(node);
            Visit((dynamic)node[1]);
            // return Type.VOID;
            return "";
        }
        public String Visit(ElseStatementNode node)
        {
            VisitChildren(node);
            // return Type.VOID;
            return "";
        }
        private void VerifyCondition(Node node)
        {
            Type conditionType = Visit((dynamic)node[0]);
            if (conditionType != Type.BOOL)
            {
                throw new SemanticError($"Condition has to be of type {Type.BOOL} but got {conditionType}",
                    node.AnchorToken);
            }
        }

        public String Visit(ProcedureListNode node)
        {
            VisitChildren(node);
            // return Type.VOID;
            return "";
        }
        public String Visit(ProcedureDeclarationNode node)
        {
            var procedureName = node.AnchorToken.Lexeme;
            if (procedureTable.Contains(procedureName))
            {
                throw new SemanticError($"Duplicate procedure {procedureName}", node.AnchorToken);
            }
            Type procedureType = Visit((dynamic)node[1]);
            procedureTable[procedureName] = new ProcedureTable.Row(procedureType, false);
            currentScope = procedureName;

            Visit((dynamic)node[0]);
            VisitChildren(node, 2);

            currentScope = "";
            // return procedureType;
            return "";
        }
        public String Visit(ParameterDeclarationNode node)
        {
            foreach (var typeNode in node)
            {
                Type type = Visit((dynamic)typeNode);
                int pos = 0;
                foreach (var idNode in typeNode)
                {
                    var varName = idNode.AnchorToken.Lexeme;
                    if (CurrentScopeHasSymbol(varName))
                    {
                        throw new SemanticError($"Duplicated parameter: {varName}",
                                                idNode.AnchorToken);
                    }
                    else
                    {
                        AddSymbolToScope(varName, type, Kind.PARAM, pos++);
                    }
                }
            }
            // return Type.VOID;
            return "";
        }
        public String Visit(ReturnStatementNode node)
        {
            if (currentScope == "")
            {
                throw new SemanticError("Unexpected return statement",
                    node.AnchorToken);
            }
            Type type = node.Count() == 0 ? Type.VOID : Visit((dynamic)node[0]);
            var procedureType = procedureTable[currentScope].type;
            if (!procedureType.CompatibleWith(type))
            {
                throw new SemanticError($"Invalid return type {type} for procedure of type {procedureType}",
                    node.AnchorToken);
            }
            // return type;
            return "";
        }

        public String Visit(CallStatementNode node)
        {
            VerifyCall(node);
            // return Type.VOID;
            return "";
        }
        public String Visit(CallNode node)
        {
            // return VerifyCall(node);
            return "";
        }
        private void VerifyCall(Node node)
        {
            var name = node.AnchorToken.Lexeme;
            if (procedureTable.Contains(name))
            {
                var procedure = procedureTable[name];
                var _params = procedure.symbols.Where(kv => kv.Value.kind == Kind.PARAM)
                                            .OrderBy(kv => kv.Value.pos)
                                            .ToList();
                if (node.Count() != _params.Count())
                {
                    throw new SemanticError($"Wrong number of params to procedure call: "
                        + $"expected {_params.Count()} but got {node.Count()}", node.AnchorToken);
                }
                for (int i = 0; i < _params.Count; ++i)
                {
                    var _node = node[i];
                    var _param = _params[i];
                    Type nodeType = Visit((dynamic)_node);
                    if (!nodeType.CompatibleWith(_param.Value.type))
                    {
                        throw new SemanticError($"Incompatible types {nodeType} and {_param.Value.type} for parameter {_param.Key}",
                            _node.AnchorToken);
                    }
                }
                // return procedure.type;
            }

            throw new SemanticError($"Undeclared procedure: {name}", node.AnchorToken);
        }

        void VisitChildren(Node node, int skip = 0, int take = 0)
        {
            skip = Math.Min(skip, node.Count());
            if (take == 0)
            {
                take = node.Count() - skip;
            }
            foreach (var n in node.Skip(skip).Take(take))
            {
                Visit((dynamic)n);
            }
        }

        void VisitBinaryOperator(Node node, Type type)
        {
            if (Visit((dynamic)node[0]) != type ||
                Visit((dynamic)node[1]) != type)
            {
                throw new SemanticError(
                    System.String.Format(
                        "Operator {0} requires two operands of type {1}",
                        node.AnchorToken.Lexeme,
                        type),
                    node.AnchorToken);
            }
        }

        void VisitBinaryIntOrBoolOperator(Node node)
        {
            Type type = Visit((dynamic)node[0]);
            switch (type)
            {
                case Type.INT:
                case Type.BOOL:
                    VisitBinaryOperator(node, type);
                    break;
                default:
                    throw new SemanticError($"Operator {node.AnchorToken.Lexeme} requires one "
                                            + $"of {Type.BOOL} or {Type.INT}", node.AnchorToken);
            }
        }

        void AddSymbolToScope(string key, Type type, Kind kind, int pos = -1)
        {
            SymbolTable table;
            if (currentScope.Length == 0)
            {
                table = symbolTable;
            }
            else
            {
                table = procedureTable[currentScope].symbols;
            }
            table[key] = new SymbolTable.Row(type, kind, pos);
        }

        SymbolTable.Row GetSymbol(string key)
        {
            // Try current scope first, then global
            if (currentScope.Length > 0 && procedureTable[currentScope].symbols.Contains(key))
            {
                return procedureTable[currentScope].symbols[key];
            }
            else if (symbolTable.Contains(key))
            {
                return symbolTable[key];
            }
            return null;
        }

        bool CurrentScopeHasSymbol(string key)
        {
            SymbolTable table;
            if (currentScope.Length == 0)
            {
                table = symbolTable;
            }
            else
            {
                table = procedureTable[currentScope].symbols;
            }
            return table.Contains(key);
        }
    }
}
