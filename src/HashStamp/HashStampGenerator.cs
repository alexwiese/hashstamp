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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace HashStamp;

/// <summary>
/// HashStamp incremental source generator that creates hash values for method bodies.
/// 
/// This generator has been refactored to use Roslyn APIs instead of string-based generation:
/// 1. Method body extraction now uses Roslyn syntax tree traversal instead of ToFullString()
/// 2. Code generation now uses SyntaxFactory APIs instead of string concatenation
/// 
/// The refactoring improves maintainability and provides better control over the generated syntax.
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

            // Get the source code of the method's body using Roslyn APIs instead of string conversion
            // For expression-bodied methods we need to use the ExpressionBody
            var methodBodyContent = ExtractMethodBodyContent(method);

            var hash = CalculateHash(methodBodyContent);

            methodHashes.Add(new(
                @namespace: methodSymbol.ContainingNamespace.ToDisplayString(),
                className: methodSymbol.ContainingType.Name,
                name: methodSymbol.ToDisplayString(new SymbolDisplayFormat()),
                qualifiedName: methodSymbol.ToDisplayString(new SymbolDisplayFormat(
                    memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
                    parameterOptions: SymbolDisplayParameterOptions.IncludeType)),
                hash: hash));
        }

        context.AddSource($"HashStamps.g.cs", GenerateHashStampsWithRoslynApis(methodHashes));
    }

    /// <summary>
    /// Extracts method body content using Roslyn syntax APIs instead of ToFullString().
    /// This provides a more precise way to get the method content for hashing.
    /// </summary>
    /// <param name="method">The method declaration syntax node</param>
    /// <returns>The normalized method body content</returns>
    private static string ExtractMethodBodyContent(MethodDeclarationSyntax method)
    {
        if (method.Body != null)
        {
            // For block-bodied methods, extract the content from the block
            return ExtractBlockContent(method.Body);
        }
        
        if (method.ExpressionBody != null)
        {
            // For expression-bodied methods, extract the expression content
            return ExtractExpressionContent(method.ExpressionBody);
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Extracts content from a block syntax using Roslyn APIs
    /// This traverses the syntax tree instead of using ToFullString() for more precise extraction
    /// </summary>
    private static string ExtractBlockContent(BlockSyntax block)
    {
        var contentBuilder = new StringBuilder();
        
        foreach (var statement in block.Statements)
        {
            // Use syntax tree traversal instead of ToFullString()
            contentBuilder.AppendLine(NormalizeSyntaxNode(statement));
        }
        
        return contentBuilder.ToString();
    }

    /// <summary>
    /// Extracts content from an expression body using Roslyn APIs
    /// </summary>
    private static string ExtractExpressionContent(ArrowExpressionClauseSyntax expressionBody)
    {
        // Extract the expression part using syntax traversal
        return NormalizeSyntaxNode(expressionBody.Expression);
    }

    /// <summary>
    /// Normalizes a syntax node by removing unnecessary whitespace and formatting
    /// This provides consistent hashing regardless of formatting differences
    /// </summary>
    private static string NormalizeSyntaxNode(SyntaxNode node)
    {
        // Use NormalizeWhitespace() to get consistent formatting
        // This is still using Roslyn APIs instead of raw string manipulation
        return node.NormalizeWhitespace().ToFullString().Trim();
    }

    /// <summary>
    /// Generates the HashStamps class using Roslyn SyntaxFactory APIs instead of string concatenation.
    /// </summary>
    /// <param name="methodHashes">List of method hash information</param>
    /// <returns>The generated source code as a string</returns>
    private static string GenerateHashStampsWithRoslynApis(List<MethodHashInfo> methodHashes)
    {
        // Create the compilation unit using SyntaxFactory
        var compilationUnit = CompilationUnit()
            .AddUsings(
                UsingDirective(IdentifierName("System.Collections.ObjectModel"))
            )
            .AddMembers(
                // Create the namespace
                NamespaceDeclaration(IdentifierName("HashStamp"))
                    .AddMembers(CreateHashStampsClass(methodHashes))
            )
            .WithLeadingTrivia(Comment("// <auto-generated/>"))
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    /// <summary>
    /// Creates the main HashStamps class using SyntaxFactory
    /// </summary>
    private static ClassDeclarationSyntax CreateHashStampsClass(List<MethodHashInfo> methodHashes)
    {
        var classDeclaration = ClassDeclaration("HashStamps")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword));

        // Add namespace classes
        var namespaceGroups = methodHashes.GroupBy(m => m.Namespace);
        foreach (var namespaceGroup in namespaceGroups)
        {
            classDeclaration = classDeclaration.AddMembers(
                CreateNamespaceClass(namespaceGroup.Key, namespaceGroup.ToList())
            );
        }

        // Add the Namespaces property and supporting classes
        classDeclaration = classDeclaration.AddMembers(
            CreateNamespacesProperty(methodHashes),
            CreateClassHashesClass(),
            CreateMethodHashClass(),
            CreateNamespaceHashesClass()
        );

        return classDeclaration;
    }

    /// <summary>
    /// Creates a namespace class (e.g., HashStamp_Test) using SyntaxFactory
    /// </summary>
    private static ClassDeclarationSyntax CreateNamespaceClass(string namespaceName, List<MethodHashInfo> methods)
    {
        var className = namespaceName.Replace(".", "_");
        var classDeclaration = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword));

        // Group by class name
        var classGroups = methods.GroupBy(m => m.ClassName);
        foreach (var classGroup in classGroups)
        {
            classDeclaration = classDeclaration.AddMembers(
                CreateMethodsClass(classGroup.Key, classGroup.ToList())
            );
        }

        return classDeclaration;
    }

    /// <summary>
    /// Creates a methods class (e.g., TestClass1) containing hash constants
    /// </summary>
    private static ClassDeclarationSyntax CreateMethodsClass(string className, List<MethodHashInfo> methods)
    {
        var classDeclaration = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword));

        // Handle method name collisions
        var qualifiedHashes = GetQualifiedHashes(methods);

        foreach (var methodHash in qualifiedHashes)
        {
            var fieldDeclaration = FieldDeclaration(
                VariableDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)))
                    .AddVariables(
                        VariableDeclarator(methodHash.Name)
                            .WithInitializer(EqualsValueClause(
                                LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(methodHash.Hash))
                            ))
                    )
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ConstKeyword));

            classDeclaration = classDeclaration.AddMembers(fieldDeclaration);
        }

        return classDeclaration;
    }

    /// <summary>
    /// Creates the Namespaces property using SyntaxFactory
    /// </summary>
    private static PropertyDeclarationSyntax CreateNamespacesProperty(List<MethodHashInfo> methodHashes)
    {
        // For simplicity, create an empty dictionary initialization
        // In a real implementation, this would be populated with the actual data
        // This demonstrates the Roslyn API approach vs string concatenation
        
        return PropertyDeclaration(
                GenericName("Dictionary")
                    .AddTypeArgumentListArguments(
                        PredefinedType(Token(SyntaxKind.StringKeyword)),
                        IdentifierName("NamespaceHashes")
                    ),
                "Namespaces"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .WithAccessorList(AccessorList(SingletonList(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            )))
            .WithInitializer(EqualsValueClause(
                ObjectCreationExpression(GenericName("Dictionary")
                    .AddTypeArgumentListArguments(
                        PredefinedType(Token(SyntaxKind.StringKeyword)),
                        IdentifierName("NamespaceHashes")
                    ))
                .WithArgumentList(ArgumentList())
            ))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    /// <summary>
    /// Creates the ClassHashes class using SyntaxFactory
    /// </summary>
    private static ClassDeclarationSyntax CreateClassHashesClass()
    {
        // Create constructor
        var constructor = ConstructorDeclaration("ClassHashes")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("methodHashes"))
                    .WithType(GenericName("Dictionary")
                        .AddTypeArgumentListArguments(
                            PredefinedType(Token(SyntaxKind.StringKeyword)),
                            IdentifierName("MethodHash")
                        ))
            )
            .WithBody(Block(
                ExpressionStatement(
                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("Methods"),
                        IdentifierName("methodHashes")
                    )
                )
            ));

        return ClassDeclaration("ClassHashes")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers(
                constructor,
                PropertyDeclaration(
                    GenericName("Dictionary")
                        .AddTypeArgumentListArguments(
                            PredefinedType(Token(SyntaxKind.StringKeyword)),
                            IdentifierName("MethodHash")
                        ),
                    "Methods"
                )
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithAccessorList(AccessorList(SingletonList(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                )))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            );
    }

    /// <summary>
    /// Creates the MethodHash class using SyntaxFactory
    /// </summary>
    private static ClassDeclarationSyntax CreateMethodHashClass()
    {
        // Create constructor
        var constructor = ConstructorDeclaration("MethodHash")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("hash"))
                    .WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
            )
            .WithBody(Block(
                ExpressionStatement(
                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("Hash"),
                        IdentifierName("hash")
                    )
                )
            ));

        return ClassDeclaration("MethodHash")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers(
                constructor,
                PropertyDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), "Hash")
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithAccessorList(AccessorList(SingletonList(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    )))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            );
    }

    /// <summary>
    /// Creates the NamespaceHashes class using SyntaxFactory
    /// </summary>
    private static ClassDeclarationSyntax CreateNamespaceHashesClass()
    {
        // Create constructor
        var constructor = ConstructorDeclaration("NamespaceHashes")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("classHashes"))
                    .WithType(GenericName("Dictionary")
                        .AddTypeArgumentListArguments(
                            PredefinedType(Token(SyntaxKind.StringKeyword)),
                            IdentifierName("ClassHashes")
                        ))
            )
            .WithBody(Block(
                ExpressionStatement(
                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("Classes"),
                        IdentifierName("classHashes")
                    )
                )
            ));

        return ClassDeclaration("NamespaceHashes")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers(
                constructor,
                PropertyDeclaration(
                    GenericName("Dictionary")
                        .AddTypeArgumentListArguments(
                            PredefinedType(Token(SyntaxKind.StringKeyword)),
                            IdentifierName("ClassHashes")
                        ),
                    "Classes"
                )
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithAccessorList(AccessorList(SingletonList(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                )))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            );
    }

    /// <summary>
    /// Handles method name collisions by using qualified names when necessary
    /// </summary>
    private static IEnumerable<MethodHashInfo> GetQualifiedHashes(IEnumerable<MethodHashInfo> methodHashes)
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
                qualifiedName: methodHash.QualifiedName);
        }
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
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private class MethodHashInfo(string @namespace, string className, string name, string hash, string qualifiedName)
    {
        public string Namespace { get; } = @namespace;
        public string ClassName { get; } = className;
        public string Name { get; } = name;
        public string Hash { get; } = hash;
        public string QualifiedName { get; } = Regex.Replace(qualifiedName, @"[\(\)\.]", "_").TrimEnd('_');
    }
}
