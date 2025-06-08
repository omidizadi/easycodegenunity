using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyFieldBuilder : EasyBasicBuilder
    {
        private string name;
        private string type;
        private SyntaxKind[] modifiers;
        private object initialValue;
        private bool initialValueSet = false;

        public EasyFieldBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Field name cannot be null or empty.", nameof(name));
            }

            this.name = name;
            return this;
        }

        public EasyFieldBuilder WithType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Type cannot be null or empty.", nameof(type));
            }

            this.type = type;
            return this;
        }

        public EasyFieldBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            this.modifiers = modifiers;
            return this;
        }

        public EasyFieldBuilder WithInitialValue<T>(T value)
        {
            initialValue = value;
            initialValueSet = true;
            return this;
        }

        protected override MemberDeclarationSyntax BuildDeclarationSyntax()
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new InvalidOperationException("Field type must be set before building.");
            }

            var fieldDeclaration = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(type))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(name)))));

            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    fieldDeclaration = fieldDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
                }
            }

            if (initialValueSet)
            {
                var initializer = SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(initialValue.ToString())));
                fieldDeclaration = fieldDeclaration.WithDeclaration(
                    fieldDeclaration.Declaration.WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            fieldDeclaration.Declaration.Variables[0].WithInitializer(initializer))));
            }

            return fieldDeclaration;
        }
    }
}