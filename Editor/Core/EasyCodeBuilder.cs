using System;
using System.Linq;
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

        private MemberDeclarationSyntax currentMemberContext; // Tracks current method, property, etc.

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

        public EasyCodeBuilder AddCode(string code)
        {
            throw new System.NotImplementedException();
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

        public EasyCodeBuilder AddType(EasyTypeInfo typeInfo)
        {
            BaseTypeDeclarationSyntax typeDeclaration = typeInfo.Type switch
            {
                EasyType.Class => SyntaxFactory.ClassDeclaration(typeInfo.Name),
                EasyType.Struct => SyntaxFactory.StructDeclaration(typeInfo.Name),
                EasyType.Interface => SyntaxFactory.InterfaceDeclaration(typeInfo.Name),
                EasyType.Enum => SyntaxFactory.EnumDeclaration(typeInfo.Name),
                _ => throw new ArgumentException("Invalid type specified.", nameof(typeInfo.Type))
            };

            if (!string.IsNullOrWhiteSpace(typeInfo.BaseType))
            {
                var baseType = SyntaxFactory.ParseTypeName(typeInfo.BaseType);
                typeDeclaration = typeDeclaration.WithBaseList(
                    SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(baseType))));
            }

            if (typeInfo.Interfaces is { Length: > 0 })
            {
                var interfaceList = typeInfo.Interfaces.Select(i => SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName(i))).ToArray();
                typeDeclaration = typeDeclaration.WithBaseList(
                    SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(interfaceList)));
            }

            typeDeclaration = typeInfo.Modifiers.Aggregate(typeDeclaration,
                (current, modifier) => current.AddModifiers(SyntaxFactory.Token(modifier)));

            root = AddMemberToType(root, typeDeclaration);

            return this;
        }

        public EasyCodeBuilder AddField(EasyFieldInfo field)
        {
            var fieldDeclaration = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(field.Type))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(field.Name)))));

            fieldDeclaration = field.Modifiers.Aggregate(fieldDeclaration,
                (current, modifier) => current.AddModifiers(SyntaxFactory.Token(modifier)));

            SetContext(fieldDeclaration);

            root = AddMemberToType(root, fieldDeclaration);

            return this;
        }

        public EasyCodeBuilder AddProperty(EasyPropertyInfo property)
        {
            throw new System.NotImplementedException();
        }

        public EasyCodeBuilder AddMethod(EasyMethodInfo method)
        {
            var methodDeclaration = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(method.ReturnType), method.Name);


            methodDeclaration = method.Modifiers.Aggregate(methodDeclaration,
                (current, modifier) => current.AddModifiers(SyntaxFactory.Token(modifier)));


            if (method.Parameters != null && method.Parameters.Length > 0)
            {
                var parameterList = SyntaxFactory.ParameterList(
                    SyntaxFactory.SeparatedList(method.Parameters.Select(p =>
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Item2))
                            .WithType(SyntaxFactory.ParseTypeName(p.Item1)))));
                methodDeclaration = methodDeclaration.WithParameterList(parameterList);
            }

            if (!string.IsNullOrWhiteSpace(method.Body))
            {
                var body = SyntaxFactory.Block(SyntaxFactory.ParseStatement(method.Body));
                methodDeclaration = methodDeclaration.WithBody(body);
            }

            SetContext(methodDeclaration);

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

        public EasyCodeBuilder ReplaceInMethodBody(string searchText, string replaceText)
        {
            if (currentMemberContext == null)
                throw new InvalidOperationException("No active member context. Add a method or property first.");

            if (currentMemberContext is MethodDeclarationSyntax methodContext)
            {
                if (methodContext.Body == null)
                    throw new InvalidOperationException("Current method has no body to replace text in.");

                string body = methodContext.Body.ToString();
                body = body.Replace(searchText, replaceText);

                // Parse the modified text as a block, not a statement
                var newBodyText = $"{body}";
                var newBody = SyntaxFactory.ParseStatement(newBodyText) as BlockSyntax;

                if (newBody == null)
                    throw new InvalidOperationException("Failed to parse the modified method body.");

                var updatedMethod = methodContext.WithBody(newBody);

                // Find the current method in the root to make sure we're replacing the correct node
                var methodInRoot = root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(m => m.Identifier.Text == methodContext.Identifier.Text);

                if (methodInRoot != null)
                {
                    root = root.ReplaceNode(methodInRoot, updatedMethod);
                    currentMemberContext = updatedMethod;
                }

                return this;
            }

            throw new InvalidOperationException("Current context is not a method.");
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

        public string ExtractMethodBodyFromTemplate(string methodName)
        {
            // grab the body of the method from the template class T by using roslyn
            if (methodName == null)
            {
                throw new ArgumentNullException(nameof(methodName), "Method body cannot be null.");
            }

            if (templateRoot == null)
            {
                throw new InvalidOperationException("Template root is not set. Please set a template first.");
            }

            var method = templateRoot.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == methodName);

            if (method == null)
            {
                throw new Exception("Method body cannot be null.");
            }

            var body = method.Body?.ToString() ?? string.Empty;
            body = body.TrimStart('{').TrimEnd('}');
            body = body.Trim();
            return body;
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