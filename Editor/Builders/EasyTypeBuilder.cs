using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public abstract class EasyTypeBuilder : EasyBasicBuilder<EasyTypeBuilder>
    {
        private string baseType;
        private string[] interfaces;
        private SyntaxKind[] modifiers;
        private string constructorBody;
        private (string, string)[] constructorParameters;
        private EasyConstructorBuilder constructorBuilder;
        protected string name { get; set; }

        protected EasyTypeBuilder(SyntaxNode templateRoot = null) : base(templateRoot)
        {
        }

        public EasyTypeBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Type name cannot be null or empty.", nameof(name));
            }

            this.name = name;
            return this;
        }

        public EasyTypeBuilder WithBaseType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new InvalidOperationException(
                    "Base type cannot be null or empty.");
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

        protected override MemberDeclarationSyntax BuildDeclarationSyntax()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Type must be set before building.");
            }

            BaseTypeDeclarationSyntax typeDeclaration = CreateTypeDeclaration();

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

            return typeDeclaration;
        }

        protected abstract BaseTypeDeclarationSyntax CreateTypeDeclaration();

        protected bool HasConstructor()
        {
            return (!string.IsNullOrWhiteSpace(constructorBody) && constructorParameters != null) ||
                   constructorBuilder != null;
        }

        protected ConstructorDeclarationSyntax CreateConstructorDeclaration()
        {
            if (constructorBuilder != null)
            {
                return (ConstructorDeclarationSyntax)constructorBuilder.Build();
            }

            var parameters = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(constructorParameters.Select(p =>
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Item2))
                        .WithType(SyntaxFactory.ParseTypeName(p.Item1)))));

            return SyntaxFactory.ConstructorDeclaration(name)
                .WithParameterList(parameters)
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(constructorBody)));
        }
    }
}