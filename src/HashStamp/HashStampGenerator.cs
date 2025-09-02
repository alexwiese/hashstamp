using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HashStamp;

/// <summary>
/// HashStamp incremental source generator that creates hash values for method bodies.
/// 
/// This generator has been enhanced to use Roslyn APIs for method body extraction:
/// - Method body extraction now uses Roslyn syntax tree traversal instead of ToFullString()
/// - Code generation continues to use the proven string-based approach for reliability
/// 
/// The enhanced method body extraction improves accuracy while maintaining stable code generation.
/// </summary>

[Generator]
public class HashStampGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG_SOURCE_GENERATOR
        System.Diagnostics.Debugger.Launch();
#endif

        var methodDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (context, _) => (MethodDeclarationSyntax)context.Node)
            .Where(m => m is not null);

        var source = context.CompilationProvider.Combine(methodDeclarations.Collect());

        context.RegisterSourceOutput(source, static (ctx, source) => Execute(source.Left, source.Right, ctx));
    }

    private static void Execute(Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods, SourceProductionContext context)
    {
        List<MethodHashInfo> methodHashes = [];

        foreach (var method in methods)
        {
            var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);

            if (methodSymbol is null)
                continue;

            // Get the source code of the method's body using enhanced Roslyn APIs instead of string conversion
            // This provides more precise method body extraction for better hashing accuracy
            var methodBodyContent = ExtractMethodBodyContent(method);

            var hash = CalculateHash(methodBodyContent);

            var qualifiedName = methodSymbol.ToDisplayString(new SymbolDisplayFormat(
                memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType));

            methodHashes.Add(new(
                @namespace: methodSymbol.ContainingNamespace.ToDisplayString(),
                className: methodSymbol.ContainingType.Name,
                name: methodSymbol.ToDisplayString(new SymbolDisplayFormat()),
                qualifiedName: qualifiedName,
                hash: hash,
                signature: qualifiedName));
        }

        context.AddSource($"HashStamps.g.cs", GenerateHashStamps(methodHashes));
    }

    /// <summary>
    /// Extracts method body content using enhanced Roslyn syntax APIs instead of ToFullString().
    /// This provides more precise method body extraction for consistent hashing.
    /// </summary>
    /// <param name="method">The method declaration syntax node</param>
    /// <returns>The normalized method body content</returns>
    private static string ExtractMethodBodyContent(MethodDeclarationSyntax method)
    {
        if (method.Body != null)
        {
            // For block-bodied methods, extract content using syntax tree traversal
            return ExtractBlockContent(method.Body);
        }

        if (method.ExpressionBody != null)
        {
            // For expression-bodied methods, extract expression content
            return ExtractExpressionContent(method.ExpressionBody);
        }

        return string.Empty;
    }

    /// <summary>
    /// Extracts content from a block syntax using Roslyn APIs for better precision
    /// </summary>
    private static string ExtractBlockContent(BlockSyntax block)
    {
        var contentBuilder = new StringBuilder();

        foreach (var statement in block.Statements)
        {
            // Use normalized syntax instead of raw ToFullString()
            contentBuilder.AppendLine(NormalizeSyntaxNode(statement));
        }

        return contentBuilder.ToString();
    }

    /// <summary>
    /// Extracts content from an expression body using Roslyn APIs
    /// </summary>
    private static string ExtractExpressionContent(ArrowExpressionClauseSyntax expressionBody)
    {
        // Extract the expression part using syntax normalization
        return NormalizeSyntaxNode(expressionBody.Expression);
    }

    /// <summary>
    /// Normalizes a syntax node to provide consistent formatting for hashing
    /// </summary>
    private static string NormalizeSyntaxNode(SyntaxNode node)
    {
        // Use Roslyn's NormalizeWhitespace for consistent formatting
        return node.NormalizeWhitespace().ToFullString().Trim();
    }

    private static string GenerateHashStamps(List<MethodHashInfo> methodHashes)
    {
        static string GenerateNamespace(string @namespace, List<MethodHashInfo> methods)
        {
            static IEnumerable<MethodHashInfo> GetQualifiedHashes(IEnumerable<MethodHashInfo> methodHashes)
            {
                var methodsWithCollidingName = methodHashes
                  .GroupBy(m => m.Name)
                  .Where(g => g.Count() > 1)
                  .Select(g => g.Key)
                  .ToImmutableHashSet();

                string GetMethodName(MethodHashInfo methodHashInfo)
                    => methodsWithCollidingName.Contains(methodHashInfo.Name)
                        ? methodHashInfo.QualifiedName
                        : methodHashInfo.Name;

                foreach (var methodHash in methodHashes)
                {
                    yield return new MethodHashInfo(
                        @namespace: methodHash.Namespace,
                        className: methodHash.ClassName,
                        name: GetMethodName(methodHash),
                        hash: methodHash.Hash,
                        qualifiedName: methodHash.QualifiedName,
                        signature: methodHash.Signature);
                }
            }


            var classConsts = methods
                .GroupBy(m => m.ClassName)
                .Select(m => GenerateClass(m.Key, [.. GetQualifiedHashes(m)]))
                .ToList();

            return $@"
                public partial class {@namespace.Replace(".", "_")}
                {{
                    {string.Join("\r\n", classConsts)}
                }}
            ";
        }

        static string GenerateClass(string className, List<MethodHashInfo> methods)
        {
            var methodHashConsts = methods
                .Select(m => $"public const string {m.Name} = \"{m.Hash}\";")
                .ToList();

            return $@"
                public partial class {className}
                {{
                    {string.Join("\r\n", methodHashConsts)}
                }}
            ";
        }

        var classes = methodHashes
            .GroupBy(m => m.Namespace)
            .Select(g => GenerateNamespace(g.Key, [.. g]))
            .ToList();

        return $@"
        // <auto-generated/>
        using System.Collections.Generic;
        using System.Collections.ObjectModel;

        namespace HashStamp;

        public static partial class HashStamps
        {{
            {string.Join("\r\n\t", classes)}

            public static Dictionary<string, NamespaceHashes> Namespaces {{ get; }} = new() {{
                {string.Join("\r\n", methodHashes.GroupBy(h => h.Namespace).Select(nh => $@"[""{nh.Key}""] = new(new() {{
                    {string.Join("\r\n\t\t\t\t\t", nh.GroupBy(h => h.ClassName).Select(ch => $@"[""{ch.Key}""] = new(new() {{
                        {string.Join("\r\n\t\t\t\t\t\t", ch.Select(mhi => $@"[""{mhi.Name}""] = new MethodHash(""{mhi.Hash}"", ""{mhi.Signature}""),"))}
                    }}),"))}
                }}),"))}
            }};

        public class ClassHashes(Dictionary<string, MethodHash> methodHashes)
        {{
            public Dictionary<string, MethodHash> Methods {{ get; }} = methodHashes;
        }}

        public class MethodHash(string hash, string signature)
        {{
            public string Hash {{ get; }} = hash;
            public string Signature {{ get; }} = signature;
        }}
        
        public class NamespaceHashes(Dictionary<string, ClassHashes> classHashes)
        {{
            public Dictionary<string, ClassHashes> Classes {{ get; }} = classHashes;
        }}
    }}
        ";
    }

    /// <summary>
    /// Calculates a SHA-256 hash of the provided source string and returns it as a lowercase hexadecimal string.
    /// </summary>
    /// <param name="source">The input string to hash.</param>
    /// <returns>The SHA-256 hash of the input as a lowercase hexadecimal string.</returns>
    private static string CalculateHash(string source)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(source);
        var hash = sha256.ComputeHash(bytes);

        // Use char array for better performance than StringBuilder - eliminates intermediate allocations
        var hexChars = new char[hash.Length * 2];

        for (int i = 0; i < hash.Length; i++)
        {
            var b = hash[i];
            hexChars[i * 2] = GetHexChar(b >> 4);
            hexChars[i * 2 + 1] = GetHexChar(b & 0xF);
        }

        return new string(hexChars);
    }

    /// <summary>
    /// Converts a 4-bit value to its lowercase hexadecimal character representation.
    /// </summary>
    /// <param name="value">Value from 0-15</param>
    /// <returns>Hexadecimal character '0'-'9' or 'a'-'f'</returns>
    private static char GetHexChar(int value)
    {
        return (char)(value < 10 ? '0' + value : 'a' + value - 10);
    }

    private class MethodHashInfo(string @namespace, string className, string name, string hash, string qualifiedName, string signature)
    {
        private static readonly char[] InvalidChars = { '(', ')', '.' };

        public string Namespace { get; } = @namespace;
        public string ClassName { get; } = className;
        public string Name { get; } = name;
        public string Hash { get; } = hash;
        public string QualifiedName { get; } = CleanQualifiedName(qualifiedName);
        public string Signature { get; } = signature;

        /// <summary>
        /// Optimized method to clean qualified name by replacing invalid characters with underscores
        /// and removing trailing underscores. Avoids regex for better performance.
        /// </summary>
        private static string CleanQualifiedName(string qualifiedName)
        {
            if (string.IsNullOrEmpty(qualifiedName))
                return string.Empty;

            // Use char array for efficient character processing - avoid regex overhead
            var buffer = new char[qualifiedName.Length];

            int writeIndex = 0;
            for (int i = 0; i < qualifiedName.Length; i++)
            {
                char c = qualifiedName[i];
                bool isInvalid = false;
                for (int j = 0; j < InvalidChars.Length; j++)
                {
                    if (c == InvalidChars[j])
                    {
                        isInvalid = true;
                        break;
                    }
                }

                if (isInvalid)
                {
                    buffer[writeIndex++] = '_';
                }
                else
                {
                    buffer[writeIndex++] = c;
                }
            }

            // Remove trailing underscores
            while (writeIndex > 0 && buffer[writeIndex - 1] == '_')
            {
                writeIndex--;
            }

            return new string(buffer, 0, writeIndex);
        }
    }
}
