using System;
using System.Linq;
using easycodegenunity.Editor.Core.Builders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor;
using UnityEngine;

namespace easycodegenunity.Editor.Core
{
    public class EasyCodeBuilder
    {
        private SyntaxNode templateRoot;
        private SyntaxNode root;
        private string outputPath;
        public string GeneratedCode { get; private set; }

        public EasyCodeBuilder WithTemplate<T>()
        {
            var template = typeof(T);
            var targetTypeGuid = AssetDatabase.FindAssets($"t:Script {template.Name}").FirstOrDefault();
            var pathToClass = AssetDatabase.GUIDToAssetPath(targetTypeGuid);
            var fullClassCode = AssetDatabase.LoadAssetAtPath<TextAsset>(pathToClass).text;
            var tree = CSharpSyntaxTree.ParseText(fullClassCode);
            templateRoot = tree.GetRoot();
            return this;
        }

        public EasyCodeBuilder AddUsingStatement(string usingStatement)
        {
            if (string.IsNullOrWhiteSpace(usingStatement))
            {
                throw new ArgumentException("Using statement cannot be null or empty.", nameof(usingStatement));
            }

            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(usingStatement));

            CompilationUnitSyntax compilationUnitRoot = root switch
            {
                null => SyntaxFactory.CompilationUnit(),
                CompilationUnitSyntax cu => cu,
                _ => throw new InvalidOperationException("Root node must be a CompilationUnitSyntax to add usings.")
            };

            root = compilationUnitRoot.AddUsings(usingDirective);
            return this;
        }

        public EasyCodeBuilder AddNamespace(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                throw new ArgumentException("Namespace name cannot be null or empty.", nameof(namespaceName));
            }

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName));

            var compilationUnitRoot = root switch
            {
                null => SyntaxFactory.CompilationUnit(),
                CompilationUnitSyntax cu => cu,
                _ => throw new InvalidOperationException("Root node must be a CompilationUnitSyntax to add namespaces.")
            };

            root = compilationUnitRoot.AddMembers(namespaceDeclaration);
            return this;
        }

        public EasyCodeBuilder AddType(Func<EasyTypeBuilder, MemberDeclarationSyntax> typeBuilderInstructions)
        {
            var typeBuilder = new EasyTypeBuilder();
            var typeDeclaration = typeBuilderInstructions(typeBuilder);
            root = AddMemberToType(root, typeDeclaration);
            return this;
        }

        public EasyCodeBuilder AddField(Func<EasyFieldBuilder, MemberDeclarationSyntax> fieldBuilderInstructions)
        {
            var fieldBuilder = new EasyFieldBuilder();
            var fieldDeclaration = fieldBuilderInstructions(fieldBuilder);
            root = AddMemberToType(root, fieldDeclaration);
            return this;
        }

        public EasyCodeBuilder AddMethod(Func<EasyMethodBuilder, MemberDeclarationSyntax> methodBuilderInstructions)
        {
            var methodBuilder = new EasyMethodBuilder(templateRoot);
            var methodDeclaration = methodBuilderInstructions(methodBuilder);
            root = AddMemberToType(root, methodDeclaration);
            return this;
        }

        public EasyCodeBuilder AddProperty(
            Func<EasyPropertyBuilder, MemberDeclarationSyntax> propertyBuilderInstructions)
        {
            var propertyBuilder = new EasyPropertyBuilder(templateRoot);
            var propertyDeclaration = propertyBuilderInstructions(propertyBuilder);
            root = AddMemberToType(root, propertyDeclaration);
            return this;
        }

        public EasyCodeBuilder SetDirectory(string path)
        {
            outputPath = path ?? throw new ArgumentNullException(nameof(path), "Output path cannot be null.");

            if (!System.IO.Directory.Exists(outputPath))
            {
                System.IO.Directory.CreateDirectory(outputPath);
            }

            return this;
        }

        public EasyCodeBuilder SetFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            if (outputPath == null)
            {
                throw new InvalidOperationException(
                    "Output path is not set. Please set the output path before setting the file name.");
            }

            outputPath = string.IsNullOrWhiteSpace(outputPath)
                ? fileName
                : System.IO.Path.Combine(outputPath, fileName);
            return this;
        }

        public EasyCodeBuilder Generate()
        {
            // This method would typically generate the final code from the root syntax node
            // and write it to the specified output path.

            if (root == null)
            {
                throw new InvalidOperationException(
                    "Root node is not set. Please add types, fields, properties, etc. before generating code.");
            }

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new InvalidOperationException(
                    "Output path is not set. Please specify an output path before generating code.");
            }

            GeneratedCode = root.NormalizeWhitespace().ToFullString();
            return this;
        }

        public void Save()
        {
            System.IO.File.WriteAllText(outputPath, GeneratedCode);
        }

        private SyntaxNode AddMemberToType(SyntaxNode rootNode, MemberDeclarationSyntax member)
        {
            // Find the first namespace in the compilation unit
            var namespaceDeclaration = rootNode.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();

            if (namespaceDeclaration != null)
            {
                // Find the first class or struct in the namespace
                var typeDeclaration = namespaceDeclaration.DescendantNodes()
                    .OfType<TypeDeclarationSyntax>()
                    .FirstOrDefault();

                if (typeDeclaration != null)
                {
                    // Add the member to the class or struct
                    var newType = typeDeclaration.AddMembers(member);
                    var newNamespace = namespaceDeclaration.ReplaceNode(typeDeclaration, newType);
                    return rootNode.ReplaceNode(namespaceDeclaration, newNamespace);
                }

                // If no class or struct is found, add the member directly to the namespace
                var newNamespaceDeclaration = namespaceDeclaration.AddMembers(member);
                return rootNode.ReplaceNode(namespaceDeclaration, newNamespaceDeclaration);
            }

            // If no namespace is found, throw an exception or handle it as needed
            if (rootNode is CompilationUnitSyntax compilationUnit)
            {
                // Add the member directly to the compilation unit
                return compilationUnit.AddMembers(member);
            }

            throw new InvalidOperationException("No namespace or type found to add the member to.");
        }
    }
}