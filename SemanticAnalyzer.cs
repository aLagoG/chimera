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

    class SemanticAnalyzer
    {
        static readonly IDictionary<TokenCategory, Type> typeMapper =
            new Dictionary<TokenCategory, Type>() {
                { TokenCategory.BOOLEAN, Type.BOOL },
                { TokenCategory.STRING, Type.STRING },
                { TokenCategory.INTEGER, Type.INT }
            };

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

        public SemanticAnalyzer()
        {
            symbolTable = new SymbolTable();
            procedureTable = new ProcedureTable();
            procedureTable["WrInt"] = new ProcedureTable.Row(Type.VOID, true);
            procedureTable["WrInt"].symbols["i"] = new SymbolTable.Row(Type.INT, Kind.PARAM, 0);

            procedureTable["WrStr"] = new ProcedureTable.Row(Type.VOID, true);
            procedureTable["WrStr"].symbols["s"] = new SymbolTable.Row(Type.STRING, Kind.PARAM, 0);

            procedureTable["WrBool"] = new ProcedureTable.Row(Type.VOID, true);
            procedureTable["WrBool"].symbols["b"] = new SymbolTable.Row(Type.BOOL, Kind.PARAM, 0);

            procedureTable["WrLn"] = new ProcedureTable.Row(Type.VOID, true);

            procedureTable["RdInt"] = new ProcedureTable.Row(Type.INT, true);
            procedureTable["RdStr"] = new ProcedureTable.Row(Type.STRING, true);

            procedureTable["AtStr"] = new ProcedureTable.Row(Type.STRING, true);
            procedureTable["AtStr"].symbols["s"] = new SymbolTable.Row(Type.STRING, Kind.PARAM, pos: 0);
            procedureTable["AtStr"].symbols["i"] = new SymbolTable.Row(Type.INT, Kind.PARAM, pos: 1);

            procedureTable["LenStr"] = new ProcedureTable.Row(Type.INT, true);
            procedureTable["LenStr"].symbols["s"] = new SymbolTable.Row(Type.STRING, Kind.PARAM, pos: 0);

            procedureTable["CmpStr"] = new ProcedureTable.Row(Type.INT, true);
            procedureTable["CmpStr"].symbols["s1"] = new SymbolTable.Row(Type.STRING, Kind.PARAM, pos: 0);
            procedureTable["CmpStr"].symbols["s2"] = new SymbolTable.Row(Type.STRING, Kind.PARAM, pos: 1);

            procedureTable["CatStr"] = new ProcedureTable.Row(Type.STRING, true);
            procedureTable["CatStr"].symbols["s1"] = new SymbolTable.Row(Type.STRING, Kind.PARAM, pos: 0);
            procedureTable["CatStr"].symbols["s2"] = new SymbolTable.Row(Type.STRING, Kind.PARAM, pos: 1);

            procedureTable["LenLstInt"] = new ProcedureTable.Row(Type.INT, true);
            procedureTable["LenLstInt"].symbols["loi"] = new SymbolTable.Row(Type.INT_LIST, Kind.PARAM, pos: 0);

            procedureTable["LenLstStr"] = new ProcedureTable.Row(Type.INT, true);
            procedureTable["LenLstStr"].symbols["los"] = new SymbolTable.Row(Type.STRING_LIST, Kind.PARAM, pos: 0);

            procedureTable["LenLstBool"] = new ProcedureTable.Row(Type.INT, true);
            procedureTable["LenLstBool"].symbols["lob"] = new SymbolTable.Row(Type.BOOL_LIST, Kind.PARAM, pos: 0);

            procedureTable["NewLstInt"] = new ProcedureTable.Row(Type.INT_LIST, true);
            procedureTable["NewLstInt"].symbols["size"] = new SymbolTable.Row(Type.INT, Kind.PARAM, pos: 0);

            procedureTable["NewLstStr"] = new ProcedureTable.Row(Type.STRING_LIST, true);
            procedureTable["NewLstStr"].symbols["size"] = new SymbolTable.Row(Type.INT, Kind.PARAM, pos: 0);

            procedureTable["NewLstBool"] = new ProcedureTable.Row(Type.BOOL_LIST, true);
            procedureTable["NewLstBool"].symbols["size"] = new SymbolTable.Row(Type.INT, Kind.PARAM, pos: 0);

            procedureTable["IntToStr"] = new ProcedureTable.Row(Type.STRING, true);
            procedureTable["IntToStr"].symbols["i"] = new SymbolTable.Row(Type.INT, Kind.PARAM, pos: 0);

            procedureTable["StrToInt"] = new ProcedureTable.Row(Type.INT, true);
            procedureTable["StrToInt"].symbols["s"] = new SymbolTable.Row(Type.STRING, Kind.PARAM, pos: 0);
        }

        public Type Visit(ProgramNode node)
        {
            VisitChildren(node);
            return Type.VOID;
        }
        public Type Visit(StatementListNode node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(AndNode node)
        {
            VisitBinaryOperator(node, Type.BOOL);
            return Type.BOOL;
        }
        public Type Visit(OrNode node)
        {
            VisitBinaryOperator(node, Type.BOOL);
            return Type.BOOL;
        }
        public Type Visit(XorNode node)
        {
            VisitBinaryOperator(node, Type.BOOL);
            return Type.BOOL;
        }
        public Type Visit(NotNode node)
        {
            if (Visit((dynamic)node[0]) != Type.BOOL)
            {
                throw new SemanticError(
                    $"Operator {node.AnchorToken.Lexeme} requires an operand of type {Type.BOOL}",
                    node.AnchorToken);
            }
            return Type.BOOL;
        }

        public Type Visit(EqualNode node)
        {
            VisitBinaryIntOrBoolOperator(node);
            return Type.BOOL;
        }
        public Type Visit(UnequalNode node)
        {
            VisitBinaryIntOrBoolOperator(node);
            return Type.BOOL;
        }

        public Type Visit(LessThanNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            return Type.BOOL;
        }
        public Type Visit(MoreThanNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            return Type.BOOL;
        }
        public Type Visit(LessThanEqualNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            return Type.BOOL;
        }
        public Type Visit(MoreThanEqualNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            return Type.BOOL;
        }

        public Type Visit(MinusNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            return Type.INT;
        }
        public Type Visit(PlusNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            return Type.INT;
        }
        public Type Visit(TimesNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            return Type.INT;
        }
        public Type Visit(DivNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            return Type.INT;
        }
        public Type Visit(RemNode node)
        {
            VisitBinaryOperator(node, Type.INT);
            return Type.INT;
        }

        public Type Visit(IntegerNode node)
        {
            return Type.INT;
        }
        public Type Visit(StringNode node)
        {
            return Type.STRING;
        }
        public Type Visit(BooleanNode node)
        {
            return Type.BOOL;
        }
        public Type Visit(VoidTypeNode node)
        {
            return Type.VOID;
        }
        public Type Visit(ListTypeNode node)
        {
            return typeMapper[node.AnchorToken.Category].ToListType();
        }

        public Type Visit(IntLiteralNode node)
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

            return Type.INT;
        }
        public Type Visit(StringLiteralNode node)
        {
            return Type.STRING;
        }
        public Type Visit(BoolLiteralNode node)
        {
            return Type.BOOL;
        }
        public Type Visit(ListLiteralNode node)
        {
            if (node.Count() == 0)
            {
                return Type.LIST;
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
            return first.ToListType();
        }

        public Type Visit(ListIndexNode node)
        {
            Type type = Visit((dynamic)node[0]);
            Type indexType = Visit((dynamic)node[1]);
            node.extra = type;
            if (indexType != Type.INT)
            {
                throw new SemanticError($"List indexes should be {Type.INT}, got {indexType}", node[1].AnchorToken);
            }
            return type.FromListType();
        }

        public Type Visit(ConstantListNode node)
        {
            VisitChildren(node);
            return Type.VOID;
        }
        public Type Visit(ConstantDeclarationNode node)
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
            AddSymbolToScope(varName, type, Kind.CONST, value: GetValueFromNode(node[0]));
            return type;
        }

        public dynamic GetValueFromNode(Node node)
        {
            if (node is IntLiteralNode)
            {
                return Convert.ToInt32(node.AnchorToken.Lexeme);
            }
            else if (node is StringLiteralNode)
            {
                return node.AnchorToken.Lexeme;
            }
            else if (node is BoolLiteralNode)
            {
                return Convert.ToBoolean(node.AnchorToken.Lexeme);
            }
            else if (node is ListLiteralNode)
            {
                if (node.Count() == 0)
                {
                    throw new SemanticError("Cannot get the value of an empty list", node.AnchorToken);
                }
                Type t = Visit((dynamic)node[0]);
                switch (t)
                {
                    case Type.INT:
                        return node.Select(n => (int)GetValueFromNode(n)).ToArray();
                    case Type.BOOL:
                        return node.Select(n => (bool)GetValueFromNode(n)).ToArray();
                    case Type.STRING:
                        return node.Select(n => (string)GetValueFromNode(n)).ToArray();
                }
            }
            return null;
        }

        public Type Visit(VariableDeclarationNode node)
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
                        AddSymbolToScope(varName, type, Kind.VAR, value: type.DefaultValue());
                    }
                }
            }
            return Type.VOID;
        }
        public Type Visit(AssignmentNode node)
        {
            Type type1 = Visit((dynamic)node[0]);
            if (node[0] is IdentifierNode && GetSymbol(node[0].AnchorToken.Lexeme).kind == Kind.CONST)
            {
                throw new SemanticError($"Cannot assign to constant '{node[0].AnchorToken.Lexeme}'", node[0].AnchorToken);
            }
            Type type2 = Visit((dynamic)node[1]);
            if (!type1.CompatibleWith(type2))
            {
                throw new SemanticError($"Cannot assign a value of type {type2} to a variable of type {type1}",
                    node.AnchorToken);
            }
            return Type.VOID;
        }
        public Type Visit(IdentifierNode node)
        {
            var variableName = node.AnchorToken.Lexeme;
            var symbol = GetSymbol(variableName);
            if (symbol != null)
            {
                return symbol.type;
            }

            throw new SemanticError(
                $"Undeclared variable or constant: {variableName}",
                node.AnchorToken);
        }

        public Type Visit(LoopStatementNode node)
        {
            var lastInLoopOrFor = inLoopOrFor;
            inLoopOrFor = true;

            VisitChildren(node);

            inLoopOrFor = lastInLoopOrFor;
            return Type.VOID;
        }
        public Type Visit(ForStatementNode node)
        {
            Type varType = Visit((dynamic)node[0]);
            Type listType = Visit((dynamic)node[1]);
            if (varType.ToListType() != listType)
            {
                throw new SemanticError($"Incompatible types {varType} and {listType}",
                    node[0].AnchorToken);
            }
            AddSymbolToScope($"__{node[0].AnchorToken.Lexeme}_index", Type.INT, Kind.VAR, value: 0);
            var lastInLoopOrFor = inLoopOrFor;
            inLoopOrFor = true;

            Visit((dynamic)node[2]);

            inLoopOrFor = lastInLoopOrFor;
            return Type.VOID;
        }
        public Type Visit(ExitNode node)
        {
            if (!inLoopOrFor)
            {
                throw new SemanticError("Unexpected exit statement", node.AnchorToken);
            }
            return Type.VOID;
        }

        public Type Visit(IfStatementNode node)
        {
            VerifyCondition(node);
            VisitChildren(node, 1);
            return Type.VOID;
        }
        public Type Visit(ElseIfListNode node)
        {
            VisitChildren(node);
            return Type.VOID;
        }
        public Type Visit(ElifStatementNode node)
        {
            VerifyCondition(node);
            Visit((dynamic)node[1]);
            return Type.VOID;
        }
        public Type Visit(ElseStatementNode node)
        {
            VisitChildren(node);
            return Type.VOID;
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

        public Type Visit(ProcedureListNode node)
        {
            VisitChildren(node);
            return Type.VOID;
        }
        public Type Visit(ProcedureDeclarationNode node)
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
            return procedureType;
        }
        public Type Visit(ParameterDeclarationNode node)
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
                        AddSymbolToScope(varName, type, Kind.PARAM, pos: pos++, value: type.DefaultValue());
                    }
                }
            }
            return Type.VOID;
        }
        public Type Visit(ReturnStatementNode node)
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
            return type;
        }

        public Type Visit(CallStatementNode node)
        {
            VerifyCall(node);
            return Type.VOID;
        }
        public Type Visit(CallNode node)
        {
            return VerifyCall(node);
        }
        private Type VerifyCall(Node node)
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
                return procedure.type;
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

        void AddSymbolToScope(string key, Type type, Kind kind, dynamic value = null, int pos = -1)
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
            table[key] = new SymbolTable.Row(type, kind, pos, value);
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
