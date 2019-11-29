/*
Chimera
Date: 2-Dic-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Chimera
{

    class Node : IEnumerable<Node>
    {

        private static readonly Dictionary<TokenCategory, System.Type> nodeForToken =
            new Dictionary<TokenCategory, System.Type>() {
                { TokenCategory.PLUS, typeof(PlusNode) },
                { TokenCategory.MINUS, typeof(MinusNode) },

                { TokenCategory.AND, typeof(AndNode) },
                { TokenCategory.OR, typeof(OrNode) },
                { TokenCategory.XOR, typeof(XorNode) },
                { TokenCategory.NOT, typeof(NotNode) },

                { TokenCategory.TIMES, typeof(TimesNode) },
                { TokenCategory.DIV, typeof(DivNode) },
                { TokenCategory.REM, typeof(RemNode) },

                { TokenCategory.EQUAL, typeof(EqualNode) },
                { TokenCategory.UNEQUAL, typeof(UnequalNode) },
                { TokenCategory.LESS_THAN, typeof(LessThanNode) },
                { TokenCategory.MORE_THAN, typeof(MoreThanNode) },
                { TokenCategory.LESS_THAN_EQUAL, typeof(LessThanEqualNode) },
                { TokenCategory.MORE_THAN_EQUAL, typeof(MoreThanEqualNode) },

                { TokenCategory.INT_LITERAL, typeof(IntLiteralNode) },
                { TokenCategory.STRING_LITERAL, typeof(StringLiteralNode) },
                { TokenCategory.TRUE, typeof(BoolLiteralNode) },
                { TokenCategory.FALSE, typeof(BoolLiteralNode) },

                { TokenCategory.INTEGER, typeof(IntegerNode) },
                { TokenCategory.STRING, typeof(StringNode) },
                { TokenCategory.BOOLEAN, typeof(BooleanNode) },

                { TokenCategory.IDENTIFIER, typeof(IdentifierNode) }
            };

        public static Node fromToken(Token token)
        {
            var node = (Node)Activator.CreateInstance(nodeForToken[token.Category]);
            node.AnchorToken = token;
            return node;
        }

        List<Node> children = new List<Node>();

        public Node this[int index]
        {
            get
            {
                return children[index];
            }
        }

        public Token AnchorToken { get; set; }

        private static int lastId = 0;
        private int id { get; set; }

        public dynamic extra { get; set; }

        public Node()
        {
            id = lastId++;
        }

        public void Add(Node node)
        {
            if (node == null)
            {
                return;
            }
            children.Add(node);
        }

        public void Add(List<Node> nodes)
        {
            children.AddRange(nodes.Where(n => n != null));
        }

        public IEnumerator<Node> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return String.Format("{0} {1}", GetType().Name, AnchorToken);
        }

        public string ToStringTree()
        {
            var sb = new StringBuilder();
            TreeTraversal(this, "", sb);
            return sb.ToString();
        }

        static void TreeTraversal(Node node, string indent, StringBuilder sb)
        {
            sb.Append(indent);
            sb.Append(node);
            sb.Append('\n');
            foreach (var child in node.children)
            {
                TreeTraversal(child, indent + "  ", sb);
            }
        }

        public string ToGraphStringTree()
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph AST {");
            GraphTreeTraversal(this, "", sb);
            sb.AppendLine("}");
            return sb.ToString();
        }

        static void GraphTreeTraversal(Node node, string indent, StringBuilder sb)
        {
            var nodeName = node.GetType().Name;
            sb.AppendLine($"\t{node.id} [label=\"{nodeName.Remove(nodeName.Length - 4)}\\n{node.AnchorToken?.ToEscapedString()}\"];");
            foreach (var child in node.children)
            {
                sb.AppendLine($"\t{node.id}->{child.id};");
                GraphTreeTraversal(child, indent + "  ", sb);
            }
        }

    }
}
