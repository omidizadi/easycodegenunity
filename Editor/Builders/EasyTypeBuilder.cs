using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    /// <summary>
    /// An abstract builder class for creating type declarations (class, struct, interface).
    /// </summary>
    public abstract class EasyTypeBuilder : EasyBasicBuilder<EasyTypeBuilder>
    {
        private string baseType;
        private string[] interfaces;
        private SyntaxKind[] modifiers;
        private string constructorBody;
        private (string, string)[] constructorParameters;
        private EasyConstructorBuilder constructorBuilder;
        protected string name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyTypeBuilder"/> class.
        /// </summary>
        /// <param name="templateRoot">The template root.</param>
        protected EasyTypeBuilder(SyntaxNode templateRoot = null) : base(templateRoot)
        {
        }

        /// <summary>
        /// Sets the name of the type.
        /// </summary>
        /// <param name="name">The name of the type.</param>
        /// <returns>The EasyTypeBuilder instance for chaining.</returns>
        public EasyTypeBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Type name cannot be null or empty.", nameof(name));
            }

            this.name = name;
            return this;
        }

        /// <summary>
        /// Sets the base type of the type.
        /// </summary>
        /// <param name="type">The base type of the type.</param>
        /// <returns>The EasyTypeBuilder instance for chaining.</returns>
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

        /// <summary>
        /// Sets the interfaces of the type.
        /// </summary>
        /// <param name="interfaces">The interfaces of the type.</param>
        /// <returns>The EasyTypeBuilder instance for chaining.</returns>
        public EasyTypeBuilder WithInterfaces(params string[] interfaces)
        {
            if (interfaces == null || interfaces.Length == 0)
            {
                throw new ArgumentException("At least one interface must be specified.", nameof(interfaces));
            }

            this.interfaces = interfaces;
            return this;
        }

        /// <summary>
        /// Sets the modifiers of the type.
        /// </summary>
        /// <param name="modifiers">The modifiers of the type.</param>
        /// <returns>The EasyTypeBuilder instance for chaining.</returns>
        public EasyTypeBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            this.modifiers = modifiers;
            return this;
        }

        /// <summary>
        /// Builds the type declaration syntax.
        /// </summary>
        /// <returns>The created type declaration syntax.</returns>
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

        /// <summary>
        /// Creates the type declaration syntax.
        /// </summary>
        /// <returns>The created type declaration syntax.</returns>
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