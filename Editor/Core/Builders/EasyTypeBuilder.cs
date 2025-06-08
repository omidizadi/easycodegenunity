using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyTypeBuilder : EasyBasicBuilder
    {
        private string name;
        private EasyType type;
        private string baseType;
        private string[] interfaces;
        private SyntaxKind[] modifiers;
        private string constructorBody;
        private (string, string)[] constructorParameters;

        public EasyTypeBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Type name cannot be null or empty.", nameof(name));
            }

            this.name = name;
            return this;
        }

        public EasyTypeBuilder WithType(EasyType type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Type name cannot be null or empty.", nameof(name));
            }

            this.type = type;
            return this;
        }

        public EasyTypeBuilder WithBaseType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new InvalidOperationException(
                    "Base type cannot be null or empty. Use 'WithType' method to set the type first.");
            }

            baseType = type;
            return this;
        }

        public EasyTypeBuilder WithInterfaces(params string[] interfaces)
        {
            if (interfaces == null || interfaces.Length == 0)
            {
                throw new ArgumentException("At least one interface must be specified.", nameof(interfaces));
            }

            this.interfaces = interfaces;
            return this;
        }

        public EasyTypeBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            this.modifiers = modifiers;
            return this;
        }

        public EasyTypeBuilder WithConstructor(string body, params (string, string)[] parameters)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException("Constructor body cannot be null or empty.", nameof(body));
            }

            constructorBody = body;
            constructorParameters =
                parameters ?? throw new ArgumentException("Parameters cannot be null.", nameof(parameters));
            return this;
        }

        public EasyBasicBuilder WithConstructorFromTemplate()
        {
            // extract the constructor from the template root, if null, throw an exception
            if (templateRoot == null)
            {
                throw new InvalidOperationException("Template root is not set. Please set a template first.");
            }

            var constructor = templateRoot.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == name);

            if (constructor?.Body == null)
            {
                throw new InvalidOperationException($"Constructor for type '{name}' not found in the template.");
            }

            constructorBody = constructor.Body.ToString();
            constructorParameters = constructor.ParameterList.Parameters
                .Select(p => (p.Identifier.Text, p.Type?.ToString())).ToArray();

            return this;
        }

        public EasyBasicBuilder ReplaceInConstructorBody(string search, string replace)
        {
            if (string.IsNullOrWhiteSpace(constructorBody))
            {
                throw new InvalidOperationException("Constructor body is not set. Please set it before replacing.");
            }

            if (string.IsNullOrWhiteSpace(search) || string.IsNullOrWhiteSpace(replace))
            {
                throw new ArgumentException("Search and replace strings cannot be null or empty.");
            }

            constructorBody = constructorBody.Replace(search, replace);
            return this;
        }

        protected override MemberDeclarationSyntax BuildDeclarationSyntax()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Type must be set before building.");
            }

            BaseTypeDeclarationSyntax typeDeclaration = type switch
            {
                EasyType.Class => SyntaxFactory.ClassDeclaration(name),
                EasyType.Struct => SyntaxFactory.StructDeclaration(name),
                EasyType.Interface => SyntaxFactory.InterfaceDeclaration(name),
                EasyType.Enum => SyntaxFactory.EnumDeclaration(name),
                _ => throw new ArgumentException("Invalid type specified.", nameof(type))
            };

            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    typeDeclaration = typeDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
                }
            }

            if (!string.IsNullOrWhiteSpace(baseType))
            {
                var baseTypeSyntax = SyntaxFactory.ParseTypeName(baseType);
                typeDeclaration = typeDeclaration.WithBaseList(
                    SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(baseTypeSyntax))));
            }

            if (interfaces != null && interfaces.Length > 0)
            {
                var interfaceList = interfaces.Select(i => SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName(i))).ToArray();
                if (typeDeclaration.BaseList != null)
                {
                    var existingBaseTypes = typeDeclaration.BaseList.Types.ToList();
                    existingBaseTypes.AddRange(interfaceList);
                    typeDeclaration = typeDeclaration.WithBaseList(
                        SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(existingBaseTypes)));
                }
                else
                {
                    typeDeclaration = typeDeclaration.WithBaseList(
                        SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(interfaceList)));
                }
            }

            if (string.IsNullOrWhiteSpace(constructorBody) || constructorParameters == null)
            {
                return typeDeclaration;
            }

            var parameters = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(constructorParameters.Select(p =>
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Item2))
                        .WithType(SyntaxFactory.ParseTypeName(p.Item1)))));

            var constructor = SyntaxFactory.ConstructorDeclaration(name)
                .WithParameterList(parameters)
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(constructorBody)));

            typeDeclaration = typeDeclaration switch
            {
                ClassDeclarationSyntax classDeclaration => classDeclaration.AddMembers(constructor),
                StructDeclarationSyntax structDeclaration => structDeclaration.AddMembers(constructor),
                _ => typeDeclaration
            };

            return typeDeclaration;
        }
    }
}