using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    /// <summary>
    /// A builder class for creating enum declarations.
    /// </summary>
    public class EasyEnumBuilder : EasyBasicBuilder<EasyEnumBuilder>
    {
        private readonly List<EnumMemberDeclarationSyntax> members = new List<EnumMemberDeclarationSyntax>();
        private string name;
        private string baseType;
        private SyntaxKind[] modifiers;

        /// <summary>
        /// Sets the name of the enum.
        /// </summary>
        /// <param name="name">The name of the enum.</param>
        /// <returns>The EasyEnumBuilder instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the name is null or empty.</exception>
        public EasyEnumBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Enum name cannot be null or empty.", nameof(name));
            }

            this.name = name;
            return this;
        }

        /// <summary>
        /// Sets the base type of the enum.
        /// </summary>
        /// <param name="type">The base type of the enum.</param>
        /// <returns>The EasyEnumBuilder instance for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the base type is null or empty.</exception>
        public EasyEnumBuilder WithBaseType(string type)
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
        /// Sets the modifiers of the enum.
        /// </summary>
        /// <param name="modifiers">The modifiers of the type.</param>
        /// <returns>The EasyTypeBuilder instance for chaining.</returns>
        public EasyEnumBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            this.modifiers = modifiers;
            return this;
        }

        /// <summary>
        /// Adds a member to the enum.
        /// </summary>
        /// <param name="name">The name of the enum member.</param>
        /// <returns>The EasyEnumBuilder instance for chaining.</returns>
        public EasyEnumBuilder AddMember(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Enum member name cannot be null or empty.", nameof(name));
            }

            var member = SyntaxFactory.EnumMemberDeclaration(SyntaxFactory.Identifier(name));
            members.Add(member);
            return this;
        }

        /// <summary>
        /// Adds a member with a specific value to the enum.
        /// </summary>
        /// <param name="name">The name of the enum member.</param>
        /// <param name="value">The value of the enum member.</param>
        /// <returns>The EasyEnumBuilder instance for chaining.</returns>
        public EasyEnumBuilder AddMember(string name, int value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Enum member name cannot be null or empty.", nameof(name));
            }

            var equalsValue = SyntaxFactory.EqualsValueClause(
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    SyntaxFactory.Literal(value)));

            var member = SyntaxFactory.EnumMemberDeclaration(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.Identifier(name),
                equalsValue);

            members.Add(member);
            return this;
        }

        /// <summary>
        /// Builds the enum declaration syntax.
        /// </summary>
        /// <returns>The syntax for the enum declaration.</returns>
        protected override MemberDeclarationSyntax BuildDeclarationSyntax()
        {
            var enumDeclaration = SyntaxFactory.EnumDeclaration(name);
            
            if (!string.IsNullOrWhiteSpace(baseType))
            {
                enumDeclaration = enumDeclaration.WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(baseType)))));
            }

            if (modifiers is { Length: > 0 })
            {
                foreach (var modifier in modifiers)
                {
                    enumDeclaration = enumDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
                }
            }

            if (members.Count > 0)
            {
                enumDeclaration = enumDeclaration.AddMembers(members.ToArray());
            }

            return enumDeclaration;
        }
    }
}