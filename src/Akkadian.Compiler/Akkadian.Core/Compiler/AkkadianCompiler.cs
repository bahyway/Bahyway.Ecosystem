using Antlr4.Runtime;
using Akkadian.Core.Grammar;
using System;
using System.IO;
using Akkadian.Core.Ast;
using Akkadian.Core.Visitors;

namespace Akkadian.Core.Compiler
{
    public class AkkadianCompilerEngine
    {
        // ✅ KEPT: The new method that returns the AST
        public AkkadianProgram Compile(string sourceCode)
        {
            // 1. Lexer & Parser
            var inputStream = new AntlrInputStream(sourceCode);
            var lexer = new AkkadianLexer(inputStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new AkkadianParser(tokens);

            // Error Handling
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ConsoleErrorListener());

            // Generate Tree
            var tree = parser.program();

            // Stop if syntax errors exist
            if (parser.NumberOfSyntaxErrors > 0) return null;

            Console.WriteLine("✅ Syntax Analysis Complete.");

            // 2. Transform into AST using Visitor
            var visitor = new AkkadianAstVisitor();
            var ast = (AkkadianProgram)visitor.Visit(tree);

            // Optional: Print Debug Info
            Console.WriteLine($"✅ Compilation Success!");
            Console.WriteLine($"   Contexts Found: {ast.Contexts.Count}");
            if (ast.Contexts.Count > 0)
            {
                Console.WriteLine($"   Context Name: {ast.Contexts[0].Name}");
                Console.WriteLine($"   Identities: {ast.Contexts[0].Identities.Count}");
                // Use ?. to safely access optional Storage
                Console.WriteLine($"   Hubs: {ast.Contexts[0].Storage?.Hubs.Count ?? 0}");
            }

            return ast;
        }

        // ❌ DELETED: The old 'void' method was removed here.
    }

    // Simple Error Listener
    public class ConsoleErrorListener : BaseErrorListener
    {
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Error] Line {line}:{charPositionInLine} - {msg}");
            Console.ResetColor();
        }
    }
}