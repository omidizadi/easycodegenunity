using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyEnumBuilder : EasyTypeBuilder
    {
        private readonly List<EnumMemberDeclarationSyntax> members = new List<EnumMemberDeclarationSyntax>();

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