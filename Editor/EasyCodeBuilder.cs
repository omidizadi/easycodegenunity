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
    /// <summary>
    /// A builder class for generating C# code.
    /// </summary>
    public class EasyCodeBuilder
    {
        private SyntaxNode templateRoot;
        private SyntaxNode root;
        private string outputPath;
        /// <summary>
        /// Gets the generated code.
        /// </summary>
        /// <value>The generated code.</value>
        public string GeneratedCode { get; private set; }

        /// <summary>
        /// Sets the template to use for code generation.
        /// </summary>
        /// <typeparam name="T">The type of the template.</typeparam>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
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

        /// <summary>
        /// Adds a using statement to the generated code.
        /// </summary>
        /// <param name="usingStatement">The using statement to add.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the using statement is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the root node is not a CompilationUnitSyntax.</exception>
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

        /// <summary>
        /// Adds a namespace to the generated code.
        /// </summary>
        /// <param name="namespaceName">The name of the namespace to add.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the namespace name is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the root node is not a CompilationUnitSyntax.</exception>
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

        /// <summary>
        /// Adds a class to the generated code.
        /// </summary>
        /// <param name="typeBuilderInstructions">The instructions for building the class.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        public EasyCodeBuilder AddClass(Func<EasyClassBuilder, MemberDeclarationSyntax> typeBuilderInstructions)
        {
            var classBuilder = new EasyClassBuilder(templateRoot);
            var typeDeclaration = typeBuilderInstructions(classBuilder);
            root = AddMemberToType(root, typeDeclaration);
            return this;
        }

        /// <summary>
        /// Adds a struct to the generated code.
        /// </summary>
        /// <param name="typeBuilderInstructions">The instructions for building the struct.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        public EasyCodeBuilder AddStruct(Func<EasyTypeBuilder, MemberDeclarationSyntax> typeBuilderInstructions)
        {
            var structBuilder = new EasyStructBuilder(templateRoot);
            var typeDeclaration = typeBuilderInstructions(structBuilder);
            root = AddMemberToType(root, typeDeclaration);
            return this;
        }

        /// <summary>
        /// Adds a constructor to the generated code.
        /// </summary>
        /// <param name="typeBuilderInstructions">The instructions for building the constructor.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        public EasyCodeBuilder AddConstructor(Func<EasyConstructorBuilder, MemberDeclarationSyntax> typeBuilderInstructions)
        {
            var constructorBuilder = new EasyConstructorBuilder(templateRoot);
            var constructorDeclaration = typeBuilderInstructions(constructorBuilder);
            root = AddMemberToType(root, constructorDeclaration);
            return this;
        }

        /// <summary>
        /// Adds an interface to the generated code.
        /// </summary>
        /// <param name="typeBuilderInstructions">The instructions for building the interface.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        public EasyCodeBuilder AddInterface(Func<EasyInterfaceBuilder, MemberDeclarationSyntax> typeBuilderInstructions)
        {
            var interfaceBuilder = new EasyInterfaceBuilder();
            var typeDeclaration = typeBuilderInstructions(interfaceBuilder);
            root = AddMemberToType(root, typeDeclaration);
            return this;
        }

        /// <summary>
        /// Adds an enum to the generated code.
        /// </summary>
        /// <param name="enumBuilderInstructions">The instructions for building the enum.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        public EasyCodeBuilder AddEnum(Func<EasyEnumBuilder, MemberDeclarationSyntax> enumBuilderInstructions)
        {
            var enumBuilder = new EasyEnumBuilder();
            var enumDeclaration = enumBuilderInstructions(enumBuilder);
            root = AddMemberToType(root, enumDeclaration);
            return this;
        }

        /// <summary>
        /// Adds a field to the generated code.
        /// </summary>
        /// <param name="fieldBuilderInstructions">The instructions for building the field.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        public EasyCodeBuilder AddField(Func<EasyFieldBuilder, MemberDeclarationSyntax> fieldBuilderInstructions)
        {
            var fieldBuilder = new EasyFieldBuilder();
            var fieldDeclaration = fieldBuilderInstructions(fieldBuilder);
            root = AddMemberToType(root, fieldDeclaration);
            return this;
        }

        /// <summary>
        /// Adds a method to the generated code.
        /// </summary>
        /// <param name="methodBuilderInstructions">The instructions for building the method.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        public EasyCodeBuilder AddMethod(Func<EasyMethodBuilder, MemberDeclarationSyntax> methodBuilderInstructions)
        {
            var methodBuilder = new EasyMethodBuilder(templateRoot);
            var methodDeclaration = methodBuilderInstructions(methodBuilder);
            root = AddMemberToType(root, methodDeclaration);
            return this;
        }

        /// <summary>
        /// Adds a property to the generated code.
        /// </summary>
        /// <param name="propertyBuilderInstructions">The instructions for building the property.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        public EasyCodeBuilder AddProperty(
            Func<EasyPropertyBuilder, MemberDeclarationSyntax> propertyBuilderInstructions)
        {
            var propertyBuilder = new EasyPropertyBuilder(templateRoot);
            var propertyDeclaration = propertyBuilderInstructions(propertyBuilder);
            root = AddMemberToType(root, propertyDeclaration);
            return this;
        }

        /// <summary>
        /// Sets the directory where the generated code will be saved.
        /// </summary>
        /// <param name="path">The output directory path.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the output path is null.</exception>
        public EasyCodeBuilder SetDirectory(string path)
        {
            outputPath = path ?? throw new ArgumentNullException(nameof(path), "Output path cannot be null.");

            if (!System.IO.Directory.Exists(outputPath))
            {
                System.IO.Directory.CreateDirectory(outputPath);
            }

            return this;
        }

        /// <summary>
        /// Sets the file name for the generated code.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the file name is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the output path is not set.</exception>
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

        /// <summary>
        /// Generates the code.
        /// </summary>
        /// <returns>The EasyCodeBuilder instance for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the root node is not set or the output path is not set.
        /// </exception>
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

        /// <summary>
        /// Saves the generated code to a file.
        /// </summary>
        public void Save()
        {
            System.IO.File.WriteAllText(outputPath, GeneratedCode);
        }

        /// <summary>
        /// Cleans up the existing file (if it exists) and saves the generated code to a file.
        /// </summary>
        public void CleanUpAndSave()
        {
            if (System.IO.File.Exists(outputPath))
            {
                System.IO.File.Delete(outputPath);
            }

            Save();
        }

        /// <summary>
        /// Adds a member to a type declaration.
        /// </summary>
        /// <param name="rootNode">The root syntax node.</param>
        /// <param name="member">The member to add.</param>
        /// <returns>The modified syntax node.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no namespace or type is found to add the member to.</exception>
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