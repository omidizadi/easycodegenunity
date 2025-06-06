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
        private MemberDeclarationSyntax currentMemberContext;

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

        public EasyCodeBuilder AddType(Func<EasyTypeBuilder, BaseTypeDeclarationSyntax> typeBuilderInstructions)
        {
            var typeBuilder = new EasyTypeBuilder();
            var typeDeclaration = typeBuilderInstructions(typeBuilder);
            root = AddMemberToType(root, typeDeclaration);
            return this;
        }

        public EasyCodeBuilder AddField(Func<EasyFieldBuilder, BaseFieldDeclarationSyntax> fieldBuilderInstructions)
        {
            var fieldBuilder = new EasyFieldBuilder();
            var fieldDeclaration = fieldBuilderInstructions(fieldBuilder);
            root = AddMemberToType(root, fieldDeclaration);
            return this;
        }

        public EasyCodeBuilder AddProperty(EasyPropertyInfo property)
        {
            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.ParseTypeName(property.Type), property.Name)
                .WithModifiers(SyntaxFactory.TokenList(property.Modifiers.Select(SyntaxFactory.Token)));

            if (property.Getter != null)
            {
                var getMethod = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(property.Getter)));
                propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(getMethod);
            }

            if (property.Setter != null)
            {
                var setMethod = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(property.Setter)));

                propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(setMethod);
            }

            SetContext(propertyDeclaration);

            root = AddMemberToType(root, propertyDeclaration);

            return this;
        }

        public EasyPropertyInfo ExtractPropertyFromTemplate(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");
            }

            if (templateRoot == null)
            {
                throw new InvalidOperationException("Template root is not set. Please set a template first.");
            }

            var property = templateRoot.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == propertyName);

            if (property == null)
            {
                throw new Exception($"Property '{propertyName}' not found in the template.");
            }

            var propertyInfo = new EasyPropertyInfo
            {
                Name = property.Identifier.Text,
                Type = property.Type.ToString(),
                Modifiers = property.Modifiers.Select(m => m.Kind()).ToArray(),
                Getter = property.AccessorList?.Accessors
                    .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration))?.Body?.ToString(),
                Setter = property.AccessorList?.Accessors
                    .FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration))?.Body?.ToString()
            };

            return propertyInfo;
        }

        public EasyCodeBuilder AddMethod(Func<EasyMethodBuilder, BaseMethodDeclarationSyntax> methodBuilderInstructions)
        {
            var methodBuilder = new EasyMethodBuilder(templateRoot);
            var methodDeclaration = methodBuilderInstructions(methodBuilder);
            root = AddMemberToType(root, methodDeclaration);
            return this;
        }


        public EasyCodeBuilder AddComment(string comment, bool isXmlDoc = false)
        {
            if (string.IsNullOrEmpty(comment))
                throw new ArgumentNullException(nameof(comment), "Comment text cannot be null or empty.");

            if (currentMemberContext == null)
                throw new InvalidOperationException(
                    "No active context. Add a class, method, property, or field first.");

            SyntaxTrivia commentTrivia;
            if (isXmlDoc)
            {
                var lines = comment.Split('\n');
                var leadingTrivia = currentMemberContext.GetLeadingTrivia();
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    string commentStart = i == 0 ? "///" : string.Empty;
                    leadingTrivia = leadingTrivia.Add(SyntaxFactory.Comment(commentStart + line.Trim()))
                        .Add(SyntaxFactory.CarriageReturnLineFeed);
                }

                var newContextXml = currentMemberContext.WithLeadingTrivia(leadingTrivia);
                root = root.ReplaceNode(currentMemberContext, newContextXml);
                currentMemberContext = newContextXml;
                return this;
            }

            if (comment.Contains('\n'))
            {
                commentTrivia = SyntaxFactory.Comment("/* " + comment + " */");
            }
            else
            {
                commentTrivia = SyntaxFactory.Comment("// " + comment);
            }

            var newContext = currentMemberContext.WithLeadingTrivia(
                currentMemberContext.GetLeadingTrivia().Add(commentTrivia).Add(SyntaxFactory.CarriageReturnLineFeed));

            root = root.ReplaceNode(currentMemberContext, newContext);
            currentMemberContext = newContext;
            return this;
        }

        public string ExtractCommentFromTemplate(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException(nameof(memberName), "Name cannot be null or empty.");
            }

            if (templateRoot == null)
            {
                throw new InvalidOperationException("Template root is not set. Please set a template first.");
            }

            SyntaxNode namedNode = FindNamedNode(memberName);

            if (namedNode == null)
            {
                return string.Empty;
            }

            var leadingTrivia = namedNode.GetLeadingTrivia();

            var commentBuilder = new System.Text.StringBuilder();
            bool hasComment = false;

            foreach (var trivia in leadingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    string text = trivia.ToString().TrimStart('/').Trim();
                    commentBuilder.AppendLine(text);
                    hasComment = true;
                }
                else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    string text = trivia.ToString();
                    text = text.Substring(2, text.Length - 4).Trim();
                    commentBuilder.AppendLine(text);
                    hasComment = true;
                }
                else if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                {
                    string text = trivia.ToString().TrimStart('/').Trim();
                    commentBuilder.AppendLine(text);
                    hasComment = true;
                }
            }

            return hasComment ? commentBuilder.ToString().TrimEnd() : string.Empty;
        }

        public EasyCodeBuilder AddAttribute<T>(params string[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public EasyCodeBuilder AddConstructor(string[] parameters = null, string constructorBody = null)
        {
            throw new System.NotImplementedException();
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


        private SyntaxNode FindNamedNode(string name)
        {
            var typeDeclaration = templateRoot.DescendantNodes().OfType<BaseTypeDeclarationSyntax>()
                .FirstOrDefault(t => t.Identifier.Text == name);
            if (typeDeclaration != null)
                return typeDeclaration;

            var methodDeclaration = templateRoot.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == name);
            if (methodDeclaration != null)
                return methodDeclaration;

            var propertyDeclaration = templateRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == name);
            if (propertyDeclaration != null)
                return propertyDeclaration;

            var fieldDeclaration = templateRoot.DescendantNodes().OfType<FieldDeclarationSyntax>()
                .FirstOrDefault(f => f.Declaration.Variables.Any(v => v.Identifier.Text == name));
            if (fieldDeclaration != null)
                return fieldDeclaration;

            var enumMemberDeclaration = templateRoot.DescendantNodes().OfType<EnumMemberDeclarationSyntax>()
                .FirstOrDefault(e => e.Identifier.Text == name);
            if (enumMemberDeclaration != null)
                return enumMemberDeclaration;

            return null; // No matching node found
        }

        private void SetContext(MemberDeclarationSyntax memberSyntax)
        {
            currentMemberContext = memberSyntax;
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