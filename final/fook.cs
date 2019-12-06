/*
* Andres de Lago Gomez  A01371779
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;

namespace fook
{

    #region Driver
    public class Compiler
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine($"Incorrect number of parameters, expecting 1 and got {args.Length}");
                return;
            }
            try
            {
                string code = args[0];
                Scanner scanner = new Scanner();
                var tokens = scanner.Start(code);

                Parser parser = new Parser();
                Node tree = parser.Parse(tokens.GetEnumerator());
                Console.WriteLine(tree.TraverseTree());

                CILGenerator generator = new CILGenerator();
                string cilCode = generator.GenerateCode(tree);
                StreamWriter writer = new StreamWriter("./output.il");
                writer.Write(cilCode);
                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
            }
        }
    }

    #endregion

    #region Token
    public enum TokenCategory
    {
        True,
        False,
        Imp,
        Eqv,
        Open_round,
        Close_round,
        Open_square,
        Close_square,
        Open_curly,
        Close_curly,
        Open_angular,
        Close_angular,
        Comma,
        EOF
    }

    public class Token
    {
        public TokenCategory Category { get; private set; }
        public string Lexeme { get; private set; }

        public Token(TokenCategory category, string lexeme)
        {
            Lexeme = lexeme;
            Category = category;
        }
    }

    #endregion

    #region Scanner

    public class Scanner
    {
        private Regex regex = new Regex(@"
            (?<True>             42 ) |
            (?<False>           ~42 ) |
            (?<Imp>             imp ) |
            (?<Eqv>             eqv ) |
            (?<Open_round>      \(  ) |
            (?<Close_round>     \)  ) |
            (?<Open_square>     \[  ) |
            (?<Close_square>    \]  ) |
            (?<Open_curly>      \{  ) |
            (?<Close_curly>     \}  ) |
            (?<Open_angular>    \<  ) |
            (?<Close_angular>   \>  ) |
            (?<Comma>           ,   ) |
            (?<Whitespase>      \s+ ) |
            (?<Other>       .+      )
        ",
        RegexOptions.IgnorePatternWhitespace |
        RegexOptions.Compiled |
        RegexOptions.Multiline |
        RegexOptions.IgnoreCase
        );

        public IEnumerable<Token> Start(string input)
        {
            foreach (Match m in regex.Matches(input))
            {
                if (m.Groups["True"].Success)
                {
                    yield return new Token(TokenCategory.True, m.Value);
                }
                else if (m.Groups["False"].Success)
                {
                    yield return new Token(TokenCategory.False, m.Value);
                }
                else if (m.Groups["Imp"].Success)
                {
                    yield return new Token(TokenCategory.Imp, m.Value);
                }
                else if (m.Groups["Eqv"].Success)
                {
                    yield return new Token(TokenCategory.Eqv, m.Value);
                }
                else if (m.Groups["Open_round"].Success)
                {
                    yield return new Token(TokenCategory.Open_round, m.Value);
                }
                else if (m.Groups["Close_round"].Success)
                {
                    yield return new Token(TokenCategory.Close_round, m.Value);
                }
                else if (m.Groups["Open_square"].Success)
                {
                    yield return new Token(TokenCategory.Open_square, m.Value);
                }
                else if (m.Groups["Close_square"].Success)
                {
                    yield return new Token(TokenCategory.Close_square, m.Value);
                }
                else if (m.Groups["Open_curly"].Success)
                {
                    yield return new Token(TokenCategory.Open_curly, m.Value);
                }
                else if (m.Groups["Close_curly"].Success)
                {
                    yield return new Token(TokenCategory.Close_curly, m.Value);
                }
                else if (m.Groups["Open_angular"].Success)
                {
                    yield return new Token(TokenCategory.Open_angular, m.Value);
                }
                else if (m.Groups["Close_angular"].Success)
                {
                    yield return new Token(TokenCategory.Close_angular, m.Value);
                }
                else if (m.Groups["Comma"].Success)
                {
                    yield return new Token(TokenCategory.Comma, m.Value);
                }
                else if (m.Groups["Whitespase"].Success)
                {
                    // Skip spaces
                }
                else if (m.Groups["Other"].Success)
                {
                    throw new Exception($"Unrecongnized Token: {m.Value}");
                }
                else
                {
                    throw new Exception($"Unrecongnized Token.");
                }
            }
            yield return new Token(TokenCategory.EOF, null);
        }
    }

    #endregion

    #region Node

    public class Node : IEnumerable<Node>
    {
        List<Node> children = new List<Node>();

        public IEnumerator<Node> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Node this[int index]
        {
            get
            {
                return children[index];
            }
        }

        public Token AnchorToken { get; set; }

        public void Add(Node node)
        {
            children.Add(node);
        }

        public override string ToString()
        {
            return $"{GetType().Name}";
        }

        public string TraverseTree(string identation = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{identation}{ToString()}");

            foreach (Node child in children)
            {
                sb.Append(child.TraverseTree($"{identation}  "));
            }

            return sb.ToString();
        }
    }

    public class Program : Node { }
    public class True : Node { }
    public class False : Node { }
    public class Imp : Node { }
    public class Eqv : Node { }

    #endregion

    #region Parser

    public class Parser
    {
        static readonly HashSet<TokenCategory> openingBraces =
                new HashSet<TokenCategory>() {
                TokenCategory.Open_angular,
                TokenCategory.Open_round,
                TokenCategory.Open_curly,
                TokenCategory.Open_square,
                };
        static readonly Dictionary<TokenCategory, TokenCategory> openToClose =
                new Dictionary<TokenCategory, TokenCategory>() {
                {TokenCategory.Open_angular,TokenCategory.Close_angular},
                {TokenCategory.Open_round,TokenCategory.Close_round},
                {TokenCategory.Open_curly,TokenCategory.Close_curly},
                {TokenCategory.Open_square,TokenCategory.Close_square},
                };
        private IEnumerator<Token> tokens;
        private TokenCategory current
        {
            get
            {
                return tokens.Current.Category;
            }
        }
        private Token Expect(TokenCategory tokenCategory)
        {
            if (current != tokenCategory)
            {
                throw new Exception($"Expected '{tokenCategory}' and got {current}");
            }
            Token result = tokens.Current;
            tokens.MoveNext();
            return result;
        }
        private Token Expect(HashSet<TokenCategory> tokenSet)
        {
            if (!tokenSet.Contains(current))
            {
                var exp = String.Join(", ", tokenSet);
                throw new Exception($"Expected one of [{exp}] and got {current}");
            }
            Token result = tokens.Current;
            tokens.MoveNext();
            return result;
        }
        private bool Has(TokenCategory tokenCategory)
        {
            return current == tokenCategory;
        }
        public Node Parse(IEnumerator<Token> tokens)
        {
            this.tokens = tokens;
            tokens.MoveNext();
            return Program();
        }

        private Node Program()
        {
            Node node = new Program() { Expression() };
            Expect(TokenCategory.EOF);
            return node;
        }

        private Node Expression()
        {
            switch (current)
            {
                case TokenCategory.True:
                    return new True { AnchorToken = Expect(TokenCategory.True) };
                case TokenCategory.False:
                    return new False { AnchorToken = Expect(TokenCategory.False) };
                case TokenCategory.Imp:
                    return Imp();
                case TokenCategory.Eqv:
                    return Eqv();
                default:
                    throw new Exception($"Unexpected token {current}");
            }
        }

        private Node Imp()
        {
            Node res = new Imp() { AnchorToken = Expect(TokenCategory.Imp) };
            var open = Expect(openingBraces);
            res.Add(Expression());
            Expect(TokenCategory.Comma);
            res.Add(Expression());
            Expect(openToClose[open.Category]);
            return res;
        }
        private Node Eqv()
        {
            Node res = new Eqv() { AnchorToken = Expect(TokenCategory.Eqv) };
            var open = Expect(openingBraces);
            res.Add(Expression());
            Expect(TokenCategory.Comma);
            res.Add(Expression());
            Expect(openToClose[open.Category]);
            return res;
        }
    }

    #endregion

    #region CIL

    public class CILGenerator
    {

        private StringBuilder builder = new StringBuilder();

        private void Visit(Program node)
        {
            builder.AppendLine(".assembly 'fook' {}");
            builder.AppendLine();
            builder.AppendLine(".class public 'output' extends ['mscorlib']'System'.'Object' {");
            builder.AppendLine("\t.method public static void 'start'() {");
            builder.AppendLine("\t\t.entrypoint");
            VisitChildren(node);
            builder.AppendLine("\t\tbrfalse 'dont_panic'");
            builder.AppendLine("\t\tldstr \"42: The answer to Life, the Universe, and Everything.\"");
            builder.AppendLine("\t\tbr 'after_dont_panic'");

            builder.AppendLine("\t'dont_panic':");
            builder.AppendLine("\t\tldstr \"~42: Don't panic.\"");

            builder.AppendLine("\t'after_dont_panic':");
            builder.AppendLine("\t\tcall void ['mscorlib']'System'.'Console'::'WriteLine'(string)");
            builder.AppendLine("\t\tret");

            builder.AppendLine("\t}");
            builder.AppendLine("}");
        }
        private void Visit(True node)
        {
            builder.AppendLine("\t\tldc.i4.1");
        }
        private void Visit(False node)
        {
            builder.AppendLine("\t\tldc.i4.0");
        }
        private void Visit(Imp node)
        {
            Visit((dynamic)node[0]);
            builder.AppendLine("\t\tldc.i4.1");
            builder.AppendLine("\t\txor");
            Visit((dynamic)node[1]);
            builder.AppendLine("\t\tor");
        }
        private void Visit(Eqv node)
        {
            VisitChildren(node);
            builder.AppendLine("\t\txor");
            builder.AppendLine("\t\tldc.i4.1");
            builder.AppendLine("\t\txor");
        }

        private void VisitChildren(Node node)
        {
            foreach (Node child in node)
            {
                Visit((dynamic)child);
            }
        }

        public string GenerateCode(Node node)
        {
            builder.Clear();
            Visit((dynamic)node);
            return builder.ToString();
        }
    }

    #endregion
}
