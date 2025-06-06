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

        private MemberDeclarationSyntax currentMemberContext; // Tracks current method, property, etc.

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

            CompilationUnitSyntax compilationUnitRoot = root switch
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

            typeDeclaration = typeInfo.AccessModifier switch
            {
                TypeAccessModifier.Public =>
                    typeDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                TypeAccessModifier.Private => typeDeclaration.AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword)),
                TypeAccessModifier.Protected => typeDeclaration.AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)),
                TypeAccessModifier.Internal => typeDeclaration.AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.InternalKeyword)),
                TypeAccessModifier.Abstract => typeDeclaration.AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.AbstractKeyword)),
                TypeAccessModifier.Sealed =>
                    typeDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword)),
                _ => throw new ArgumentException("Invalid type access modifier specified.",
                    nameof(typeInfo.AccessModifier))
            };

            if (typeInfo.IsStatic)
            {
                typeDeclaration = typeDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            }

            if (!string.IsNullOrWhiteSpace(typeInfo.BaseType))
            {
                var baseType = SyntaxFactory.ParseTypeName(typeInfo.BaseType);
                typeDeclaration = typeDeclaration.WithBaseList(
                    SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(baseType))));
            }

            if (typeInfo.Interfaces != null && typeInfo.Interfaces.Length > 0)
            {
                var interfaceList = typeInfo.Interfaces.Select(i => SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName(i))).ToArray();
                typeDeclaration = typeDeclaration.WithBaseList(
                    SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(interfaceList)));
            }

            if (typeInfo.IsPartial)
            {
                typeDeclaration = typeDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
            }

            CompilationUnitSyntax compilationUnitRoot = root switch
            {
                null => SyntaxFactory.CompilationUnit(),
                CompilationUnitSyntax cu => cu,
                _ => throw new InvalidOperationException("Root node must be a CompilationUnitSyntax to add types.")
            };

            // Look for a namespace declaration to add the type to
            var namespaceDeclaration =
                compilationUnitRoot.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            if (namespaceDeclaration != null)
            {
                // Add the type to the namespace
                var newNamespace = namespaceDeclaration.AddMembers(typeDeclaration);
                root = compilationUnitRoot.ReplaceNode(namespaceDeclaration, newNamespace);
            }
            else
            {
                // No namespace found, add type directly to compilation unit
                root = compilationUnitRoot.AddMembers(typeDeclaration);
            }

            return this;
        }

        public EasyCodeBuilder AddField(EasyFieldInfo field)
        {
            throw new System.NotImplementedException();
        }

        public EasyCodeBuilder AddProperty(EasyPropertyInfo property)
        {
            throw new System.NotImplementedException();
        }

        public EasyCodeBuilder AddMethod(EasyMethodInfo method)
        {
            var methodDeclaration = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(method.ReturnType), method.Name);

            // Access modifiers
            SyntaxKind accessModifier = method.AccessModifier switch
            {
                MemberAccessModifier.Public => SyntaxKind.PublicKeyword,
                MemberAccessModifier.Private => SyntaxKind.PrivateKeyword,
                MemberAccessModifier.Protected => SyntaxKind.ProtectedKeyword,
                MemberAccessModifier.Internal => SyntaxKind.InternalKeyword,
                _ => SyntaxKind.PublicKeyword
            };

            methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(accessModifier));

            if (method.IsStatic)
            {
                methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            }

            // Add other modifiers
            if (method.Modifiers != null)
            {
                foreach (var modifier in method.Modifiers)
                {
                    SyntaxKind kind = modifier switch
                    {
                        "async" => SyntaxKind.AsyncKeyword,
                        "virtual" => SyntaxKind.VirtualKeyword,
                        "abstract" => SyntaxKind.AbstractKeyword,
                        "override" => SyntaxKind.OverrideKeyword,
                        "sealed" => SyntaxKind.SealedKeyword,
                        _ => throw new ArgumentException($"Unsupported modifier: {modifier}")
                    };
                    methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(kind));
                }
            }

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

            // Add the method to the appropriate location in the syntax tree
            root = AddMemberToType(root, methodDeclaration);

            return this;
        }

        public EasyCodeBuilder AddComment(string comment, bool isSummary = false)
        {
            throw new System.NotImplementedException();
        }

        public EasyCodeBuilder AddAttribute<T>(params string[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public EasyCodeBuilder AddEvent(EasyEventInfo eventInfo)
        {
            string typeName;
            if (eventInfo.Type == EasyEventInfo.EventType.Action)
            {
                typeName = "Action";
                if (eventInfo.ParameterTypes is { Length: > 0 })
                {
                    typeName += "<" + string.Join(", ", eventInfo.ParameterTypes.Select(p => p)) + ">";
                }
            }
            else if (eventInfo.Type == EasyEventInfo.EventType.Func)
            {
                if (eventInfo.ParameterTypes == null || eventInfo.ParameterTypes.Length < 1)
                {
                    throw new ArgumentException(
                        "Func event types must have at least one parameter type for the return value.",
                        nameof(eventInfo.ParameterTypes));
                }

                // Last type in the array is the return type for Func
                typeName = "Func<" + string.Join(", ", eventInfo.ParameterTypes.Select(p => p)) + ">";
            }
            else
            {
                throw new ArgumentException("Invalid event type specified.", nameof(eventInfo.Type));
            }

            var eventDeclaration = SyntaxFactory.EventFieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(typeName))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(eventInfo.Name)))));

            // Access modifiers
            SyntaxKind accessModifier = eventInfo.AccessModifier switch
            {
                MemberAccessModifier.Public => SyntaxKind.PublicKeyword,
                MemberAccessModifier.Private => SyntaxKind.PrivateKeyword,
                MemberAccessModifier.Protected => SyntaxKind.ProtectedKeyword,
                MemberAccessModifier.Internal => SyntaxKind.InternalKeyword,
                _ => SyntaxKind.PublicKeyword
            };

            eventDeclaration = eventDeclaration.AddModifiers(SyntaxFactory.Token(accessModifier));

            if (eventInfo.IsStatic)
            {
                eventDeclaration = eventDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            }

            SetContext(eventDeclaration);

            // Add the event to the appropriate location in the syntax tree
            root = AddMemberToType(root, eventDeclaration);

            return this;
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

        internal void GenerateCode()
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

            var formattedCode = root.NormalizeWhitespace().ToFullString();
            System.IO.File.WriteAllText(outputPath, formattedCode);
            Debug.Log($"Code generated successfully at {outputPath}");
        }

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

    public struct EasyMethodInfo
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public MemberAccessModifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public string[] Modifiers { get; set; }
        public (string, string)[] Parameters { get; set; }
        public string Body { get; set; }
    }

    public struct EasyFieldInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public MemberAccessModifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public string InitialValue { get; set; }
    }

    public struct EasyPropertyInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public MemberAccessModifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public string InitialValue { get; set; }
        public string Getter { get; set; }
        public string Setter { get; set; }
    }

    public struct EasyEventInfo
    {
        public string Name { get; set; }
        public EventType Type { get; set; }
        public string[] ParameterTypes { get; set; }
        public MemberAccessModifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }

        //todo: allow creating custom delegate types
        public enum EventType
        {
            Action,
            Func
        }
    }

    public struct EasyTypeInfo
    {
        public EasyType Type { get; set; }
        public string Name { get; set; }
        public bool IsPartial { get; set; }
        public string BaseType { get; set; }
        public string[] Interfaces { get; set; }
        public TypeAccessModifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
    }
}