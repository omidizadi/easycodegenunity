using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    /// <summary>
    /// A builder class for creating enum declarations.
    /// </summary>
    public class EasyEnumBuilder : EasyTypeBuilder
    {
        private readonly List<EnumMemberDeclarationSyntax> members = new List<EnumMemberDeclarationSyntax>();

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
        /// Creates the enum declaration syntax.
        /// </summary>
        /// <returns>The created enum declaration syntax.</returns>
        protected override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            var enumDeclaration = SyntaxFactory.EnumDeclaration(name);
            
            if (members.Count > 0)
            {
                enumDeclaration = enumDeclaration.AddMembers(members.ToArray());
            }
            
            return enumDeclaration;
        }
    }
}