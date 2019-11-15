/*
Chimera
Date: 11-Nov-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Chimera
{
    using VariableType = KeyValuePair<string, SymbolTable.Row>;

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

        private bool inAssignment = false;
        private int id = 0;
        private int currentId = 0;
        private int currentIfId = 0;
        private int currentElseCount = 0;

        private StringBuilder builder = new StringBuilder();

        public CILGenerator(SymbolTable symbolTable, ProcedureTable procedureTable)
        {
            this.symbolTable = symbolTable;
            this.procedureTable = procedureTable;
            this.builder.Clear();
        }

        public override string ToString()
        {
            return builder.ToString();
        }

        public void Visit(ProgramNode node)
        {
            builder.AppendLine("// Code generated by the chimera compiler\n");
            builder.AppendLine(".assembly 'Chimera' {}");
            builder.AppendLine(".assembly extern 'ChimeraLib' {}");
            builder.AppendLine(".class public 'ChimeraProgram' extends ['mscorlib']'System'.'Object' {");
            builder.AppendLine(DeclareVariablesOnScope(""));

            Visit((dynamic)node[node.Count() - 2]);

            builder.AppendLine("\t.method public static void main(){");
            builder.AppendLine("\t\t.entrypoint");
            builder.AppendLine(InitializeVariablesOnScope(""));
            Visit((dynamic)node.Last());
            builder.AppendLine("\t\tret");
            builder.AppendLine("\t}");
            builder.AppendLine("}");
        }
        public void Visit(StatementListNode node)
        {
            VisitChildren(node);
        }

        public void Visit(AndNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tand");
        }
        public void Visit(OrNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tor");
        }
        public void Visit(XorNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\txor");
        }
        public void Visit(NotNode node)
        {
            Visit((dynamic)node[0]);
            builder.AppendLine("\t\tnot");
        }

        public void Visit(EqualNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tceq");
        }
        public void Visit(UnequalNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tceq");
            builder.AppendLine("\t\tnot");
        }

        public void Visit(LessThanNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tclt");
        }
        public void Visit(MoreThanNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tcgt");
        }
        public void Visit(LessThanEqualNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tcgt");
            builder.AppendLine("\t\tnot");
        }
        public void Visit(MoreThanEqualNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tclt");
            builder.AppendLine("\t\tnot");
        }

        public void Visit(MinusNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tsub.ovf");
        }
        public void Visit(PlusNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tadd.ovf");
        }
        public void Visit(TimesNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tmul.ovf");
        }
        public void Visit(DivNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\tdiv");
        }
        public void Visit(RemNode node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\trem");
        }

        public void Visit(IntegerNode node)
        {
            builder.AppendLine("int32");
        }
        public void Visit(StringNode node)
        {
            builder.AppendLine("string");
        }
        public void Visit(BooleanNode node)
        {
            builder.Append("bool");
        }
        public void Visit(VoidTypeNode node)
        {
            builder.Append("void");
        }
        public void Visit(ListTypeNode node)
        {
            Visit((dynamic)Node.fromToken(node.AnchorToken));
            builder.Append("[]");
        }

        public void Visit(IntLiteralNode node)
        {
            builder.AppendLine($"\t\tldc.i4 {node.AnchorToken.Lexeme}");
        }
        public void Visit(StringLiteralNode node)
        {
            builder.AppendLine($"\t\tldstr {node.AnchorToken.Lexeme}");
        }
        public void Visit(BoolLiteralNode node)
        {
            var value = node.AnchorToken.Lexeme == "true" ? 1 : 0;
            builder.AppendLine($"\t\tldc.i4.{value}");
        }
        public void Visit(ListLiteralNode node)
        {
            builder.AppendLine($"\t\tldc.i4 {node.Count()}");
            string listType = "";
            string elementType = "";
            if (node[0] is IntLiteralNode)
            {
                listType = "int32";
                elementType = "i4";
            }
            else if (node[0] is BoolLiteralNode)
            {
                listType = "bool";
                elementType = "i4";
            }
            else if (node[0] is StringLiteralNode)
            {
                listType = "string";
                elementType = "ref";
            }
            builder.AppendLine($"\t\tnewarr {listType}");
            int index = 0;
            foreach (var n in node)
            {
                builder.AppendLine("\t\tdup");
                builder.AppendLine($"\t\tldc.i4 {index}");
                Visit((dynamic)n);

                builder.AppendLine($"\t\tstelem.{elementType}");
                index++;
            }
        }

        public void Visit(ListIndexNode node)
        {
            Visit((dynamic)node[0]);
            Visit((dynamic)node[1]);

            if (!inAssignment)
            {
                builder.AppendLine("\t\tldelem.ref");
            }
        }

        public void Visit(ConstantListNode node)
        {
            // Do nothing
        }
        public void Visit(ConstantDeclarationNode node)
        {
            // Do nothing
        }
        public void Visit(VariableDeclarationNode node)
        {
            // Do nothing
        }

        public void Visit(AssignmentNode node)
        {
            inAssignment = true;
            Visit((dynamic)node[0]);
            inAssignment = false;
            Visit((dynamic)node[1]);
            if (node[0] is ListIndexNode)
            {
                builder.AppendLine("stelem.ref");
            }
            else
            {
                string varName = node[0].AnchorToken.Lexeme;
                StoreInVariable(varName);
            }
        }
        public void Visit(IdentifierNode node)
        {
            string varName = node.AnchorToken.Lexeme;

            if (!inAssignment)
            {
                LoadVariable(varName);
            }
        }

        public void Visit(LoopStatementNode node)
        {
            var lastId = currentId;
            currentId = id++;
            builder.AppendLine($"loop_{currentId}:");
            VisitChildren(node);
            builder.Append($"end_{currentId}");

            currentId = lastId;
        }
        public void Visit(ForStatementNode node)
        {
            var lastId = currentId;
            currentId = id++;

            string varName = node[0].AnchorToken.Lexeme;
            string indexVarName = $"__{varName}_index";
            builder.AppendLine("\t\tldc.i4.0");
            StoreInVariable(indexVarName);

            Visit((dynamic)node[1]);
            builder.AppendLine($"\tfor_{currentId}:");
            builder.AppendLine($"\t\tdup");
            LoadVariable(indexVarName);
            switch (GetSymbol(varName).type)
            {
                case Type.BOOL:
                case Type.INT:
                    builder.AppendLine($"\t\tldelem.i4");
                    break;
                default:
                    builder.AppendLine($"\t\tldelem.ref");
                    break;

            }
            StoreInVariable(varName);

            builder.AppendLine($"// visiting {node[1].GetType().Name}");
            Visit((dynamic)node[2]);

            LoadVariable(indexVarName);
            builder.AppendLine("\t\tldc.i4.1");
            builder.AppendLine("\t\tadd");
            StoreInVariable(indexVarName);

            builder.AppendLine($"\tnext_{currentId}:");
            builder.AppendLine($"\t\tdup");
            builder.AppendLine($"\t\tldlen");
            builder.AppendLine($"\t\tconv.i4");
            LoadVariable(indexVarName);
            builder.AppendLine($"\t\tbgt for_{currentId}");

            builder.AppendLine($"\tend_{currentId}:");
            builder.AppendLine($"\t\tpop");

            currentId = lastId;
        }
        public void Visit(ExitNode node)
        {
            builder.AppendLine($"br end_{currentId}");
        }

        public void Visit(IfStatementNode node)
        {
            currentIfId = id++;
            int previousElseCount = currentElseCount;
            currentElseCount = 0;

            Visit((dynamic)node[0]);
            builder.AppendLine($"brzero If_{currentIfId}_1");
            builder.AppendLine($"If_{currentIfId}_0:");
            Visit((dynamic)node[1]);
            builder.AppendLine($"br If_{currentIfId}_End");
            VisitChildren(node, 2);
            builder.AppendLine($"If_{currentIfId}_{currentElseCount + 1}:");
            builder.AppendLine($"If_{currentIfId}_End:");

            currentElseCount = previousElseCount;
        }
        public void Visit(ElseIfListNode node)
        {
            VisitChildren(node);
        }
        public void Visit(ElifStatementNode node)
        {
            currentElseCount++;
            Visit((dynamic)node[0]);
            builder.AppendLine($"brzero If_{currentIfId}_{currentElseCount + 1}");
            builder.AppendLine($"If_{currentIfId}_{currentElseCount}:");
            VisitChildren(node, 1);
            builder.AppendLine($"If_{currentIfId}_End:");
        }
        public void Visit(ElseStatementNode node)
        {
            currentElseCount++;
            builder.AppendLine($"If_{currentIfId}_{currentElseCount}:");
            VisitChildren(node);
            builder.AppendLine($"If_{currentIfId}_End:");
        }

        public void Visit(ProcedureListNode node)
        {
            VisitChildren(node);
        }
        public void Visit(ProcedureDeclarationNode node)
        {
            var procedureName = node.AnchorToken.Lexeme;
            var procedure = procedureTable[procedureName];
            Type type = procedure.type;
            string returnType = type.ToCilType();
            var lastScope = currentScope;
            currentScope = procedureName;

            builder.Append($"\t.method public static {returnType} '{procedureName}'(");
            builder.Append(DeclareParameters(procedureName));

            builder.AppendLine("){");
            builder.AppendLine(DeclareVariablesOnScope(procedureName));
            builder.AppendLine(InitializeVariablesOnScope(currentScope));

            if (node.Last() is StatementListNode)
            {
                Visit((dynamic)node.Last());
            }

            builder.AppendLine("\t\tret");
            builder.AppendLine("\t}");

            currentScope = lastScope;
        }
        public void Visit(ParameterDeclarationNode node)
        {
            // do nothing
        }
        public void Visit(ReturnStatementNode node)
        {
            bool returnsSomething = node.Count() != 0;
            string procedureName = currentScope;
            Type returnType = procedureTable[procedureName].type;

            if (returnsSomething)
            {
                Visit((dynamic)node[0]);
            }
            else
            {
                string defaultValue = GetTypeDefaultCilValue(returnType);
                builder.AppendLine($"\t\t{defaultValue}");
            }
            builder.AppendLine("ret");
        }

        public void Visit(CallStatementNode node)
        {
            VerifyCall(node);
            var procedureName = node.AnchorToken.Lexeme;
            Type type = procedureTable[procedureName].type;
            if (type != Type.VOID)
            {
                builder.AppendLine("pop");
            }
        }
        public void Visit(CallNode node)
        {
            VerifyCall(node);
        }

        // private Type GetLiteralNodeType(IntegerNode node)
        // {
        //     return Type.INT;
        // }
        // private Type GetLiteralNodeType(BooleanNode node)
        // {
        //     return Type.BOOL;
        // }
        // private Type GetLiteralNodeType(StringNode node)
        // {
        //     return Type.STRING;
        // }

        private SymbolTable GetScopeSymbols(string scope)
        {
            if (scope == "")
            {
                return symbolTable;
            }
            else
            {
                return procedureTable[scope].symbols;
            }
        }

        private IEnumerable<VariableType> GetParams(string scope)
        {
            SymbolTable table = GetScopeSymbols(scope);
            return table.Where(kv => kv.Value.kind == Kind.PARAM)
                            .OrderBy(kv => kv.Value.pos);
        }


        private IEnumerable<VariableType> GetLocals(string scope)
        {
            SymbolTable table = GetScopeSymbols(scope);
            return table.Where(kv => kv.Value.kind != Kind.PARAM)
                            .OrderBy(kv => kv.Value.pos);
        }

        private IEnumerable<VariableType> GetLocalVars(string scope)
        {
            SymbolTable table = GetScopeSymbols(scope);
            return table.Where(kv => kv.Value.kind == Kind.VAR)
                            .OrderBy(kv => kv.Value.pos);
        }

        private IEnumerable<VariableType> GetLocalConstants(string scope)
        {
            SymbolTable table = GetScopeSymbols(scope);
            return table.Where(kv => kv.Value.kind == Kind.CONST)
                            .OrderBy(kv => kv.Value.pos);
        }

        private string GetTypeDefaultCilValue(Type type)
        {
            switch (type)
            {
                case Type.BOOL:
                case Type.INT:
                    return "ldc.i4.0";
                case Type.STRING:
                    return "ldstr \"\"";
                case Type.BOOL_LIST:
                case Type.INT_LIST:
                    return "ldc.i4.0\nnewarr int32";
                case Type.STRING_LIST:
                    return "ldc.i4.0\nnewarr string";
                case Type.VOID:
                    return "";
            }
            throw new Exception($"Could not find CIL type for: {type}");
        }

        private string GetCilValue(VariableType variable)
        {
            StringBuilder result = new StringBuilder();
            int index = 0;
            SymbolTable.Row row = variable.Value;
            switch (row.type)
            {
                case Type.BOOL:
                    var b = row.value ? 1 : 0;
                    return $"ldc.i4.{b}";
                case Type.INT:
                    return $"ldc.i4 {row.value}";
                case Type.STRING:
                    string s = row.value;
                    s = s.Count() == 0 ? "\"\"" : s;
                    return $"ldstr {s}";
                case Type.BOOL_LIST:
                    bool[] constBoolArr = row.value as bool[];
                    result.AppendLine($"ldc.i4 {constBoolArr.Length}");
                    result.AppendLine("\t\tnewarr int32");
                    foreach (bool val in constBoolArr)
                    {
                        result.AppendLine($"\t\tdup");
                        result.AppendLine($"\t\tldc.i4 {index++}");
                        result.AppendLine($"\t\tldc.i4.{(val ? 1 : 0)}");
                        result.AppendLine($"\t\tstelem.i4");
                    }
                    return result.ToString();
                case Type.INT_LIST:
                    int[] constIntArr = row.value as int[];
                    result.AppendLine($"ldc.i4 {constIntArr.Length}");
                    result.AppendLine("\t\tnewarr int32");
                    foreach (int val in constIntArr)
                    {
                        result.AppendLine($"\t\tdup");
                        result.AppendLine($"\t\tldc.i4 {index++}");
                        result.AppendLine($"\t\tldc.i4 {val}");
                        result.AppendLine($"\t\tstelem.i4");
                    }
                    return result.ToString();
                case Type.STRING_LIST:
                    string[] constStrArr = row.value as string[];
                    result.AppendLine($"ldc.i4 {constStrArr.Length}");
                    result.AppendLine("\t\tnewarr string");
                    foreach (string val in constStrArr)
                    {
                        result.AppendLine($"\t\tdup");
                        result.AppendLine($"\t\tldc.i4 {index++}");
                        result.AppendLine($"\t\tldstr {val}");
                        result.AppendLine($"\t\tstelem.ref");
                    }
                    return result.ToString();
                case Type.VOID:
                    return "";
            }
            throw new Exception($"Could not find value for: {variable.Key}");
        }

        private void VerifyCall(Node node)
        {
            string procedureName = node.AnchorToken.Lexeme;
            var procedure = procedureTable[procedureName];
            string returnType = procedure.type.ToCilType();
            string _prefix = "";

            if (procedure.isPredefined)
            {
                _prefix = "['ChimeraLib']'Chimera'.Lib";
            }
            else
            {
                _prefix = "['Chimera']'ChimeraProgram'";
            }

            VisitChildren(node);
            builder.Append($"\t\tcall {returnType} class {_prefix}::'{procedureName}'(");
            var _params = GetParams(procedureName);
            var start = true;
            foreach (var param in _params)
            {
                if (!start)
                {
                    builder.Append(",");
                }
                start = false;
                builder.Append($"{param.Value.type.ToCilType()}");
            }
            builder.AppendLine(")");
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

        private string DeclareParameters(string procedureName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var _params = GetParams(procedureName);
            var start = true;
            foreach (var param in _params)
            {
                if (!start)
                {
                    stringBuilder.Append(", ");
                }
                start = false;
                stringBuilder.Append(DeclareLocalVar(param));
            }
            return stringBuilder.ToString();
        }

        private string InitializeVariablesOnScope(string scope)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var locals = GetLocals(scope);

            if (scope == "")
            {
                foreach (var global in locals)
                {
                    stringBuilder.Append(InitializeStaticVariable(global));
                }
            }
            else
            {
                foreach (var local in locals)
                {
                    stringBuilder.Append(InitializeLocalVariable(local));
                }
            }
            return stringBuilder.ToString();
        }

        private string InitializeStaticVariable(VariableType variable)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string value = GetCilValue(variable);
            stringBuilder.AppendLine($"\t\t{value}");
            stringBuilder.AppendLine($"\t\tstsfld {variable.Value.type.ToCilType()} class ['Chimera']'ChimeraProgram'::'{variable.Key}'");
            return stringBuilder.ToString();
        }
        private string InitializeLocalVariable(VariableType variable)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string value = GetCilValue(variable);
            stringBuilder.AppendLine($"\t\t{value}");
            stringBuilder.AppendLine($"\t\tstloc '{variable.Key}'");
            return stringBuilder.ToString();
        }

        private string DeclareVariablesOnScope(string scope)
        {
            var locals = GetLocals(scope);
            var start = true;
            StringBuilder stringBuilder = new StringBuilder();
            if (scope == "")
            {
                foreach (var global in locals)
                {
                    stringBuilder.AppendLine(DeclareGlobalVar(global));
                }
            }
            else
            {
                stringBuilder.Append("\t\t.locals init (");
                start = true;
                foreach (var local in locals)
                {
                    if (!start)
                    {
                        stringBuilder.Append(", ");
                    }
                    stringBuilder.AppendLine();
                    start = false;
                    stringBuilder.Append($"\t\t\t{DeclareLocalVar(local)}");
                }
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("\t\t)");
            }
            return stringBuilder.ToString();
        }

        private string DeclareGlobalVar(VariableType variable)
        {
            return $"\t.field public static {variable.Value.type.ToCilType()} '{variable.Key}'";
        }

        private string DeclareLocalVar(VariableType variable)
        {
            return $"{variable.Value.type.ToCilType()} {variable.Key}";
        }

        private void LoadVariable(string varName)
        {
            var symbol = GetSymbol(varName);
            if (symbol.kind == Kind.PARAM)
            {
                builder.AppendLine($"\t\tldarg {varName}");
            }
            else if (currentScope != "" && procedureTable[currentScope].symbols.Contains(varName))
            {
                builder.AppendLine($"\t\tldloc {varName}");
            }
            else
            {
                builder.AppendLine($"\t\tldsfld {symbol.type.ToCilType()} class ['Chimera']'ChimeraProgram'::'{varName}'");
            }
        }

        private void StoreInVariable(string varName)
        {
            string varType = GetSymbol(varName).type.ToCilType();
            if (currentScope != "" && procedureTable[currentScope].symbols.Contains(varName))
            {
                builder.AppendLine($"\t\tstloc {varName}");
            }
            else
            {
                builder.AppendLine($"\t\tstsfld {varType} class ['Chimera']'ChimeraProgram'::'{varName}'");
            }
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
    }
}
