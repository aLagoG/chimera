/*
Chimera
Date: 2-Dic-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System.Collections.Generic;

namespace Chimera
{
    class AndNode : Node { }
    class OrNode : Node { }
    class XorNode : Node { }
    class NotNode : Node { }
    class EqualNode : Node { }
    class UnequalNode : Node { }
    class LessThanNode : Node { }
    class MoreThanNode : Node { }
    class LessThanEqualNode : Node { }
    class MoreThanEqualNode : Node { }
    class MinusNode : Node { }
    class PlusNode : Node { }
    class TimesNode : Node { }
    class DivNode : Node { }
    class RemNode : Node { }
    class ExitNode : Node { }
    class IntegerNode : Node { }
    class StringNode : Node { }
    class BooleanNode : Node { }
    class VoidTypeNode : Node { }
    class ListTypeNode : Node { }
    class IntLiteralNode : Node { }
    class StringLiteralNode : Node { }
    class BoolLiteralNode : Node { }
    class ListLiteralNode : Node { }
    class ConstantDeclarationNode : Node { }
    class ConstantListNode : Node { }
    class IdentifierNode : Node
    {
        public bool isAssignment { get; set; } = false;
    }
    class ReturnStatementNode : Node { }
    class LoopStatementNode : Node { }
    class VariableDeclarationNode : Node { }
    class ForStatementNode : Node { }
    class StatementListNode : Node { }
    class IfStatementNode : Node { }
    class ElifStatementNode : Node { }
    class ElseStatementNode : Node { }
    class ProgramNode : Node { }
    class ProcedureDeclarationNode : Node { }
    class ProcedureListNode : Node { }
    class ParameterDeclarationNode : Node { }
    class AssignmentNode : Node { }
    class CallStatementNode : Node { }
    class CallNode : Node { }
    class ListIndexNode : Node
    {
        public bool isAssignment { get; set; } = false;
    }
    class ElseIfListNode : Node { }
}
