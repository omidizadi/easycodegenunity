using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyFieldBuilder
    {
        private string name;
        private BaseFieldDeclarationSyntax fieldDeclaration;

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

            fieldDeclaration = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(type))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(name)))));
            return this;
        }

        public EasyFieldBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (fieldDeclaration == null)
            {
                throw new InvalidOperationException("Field type must be set before setting modifiers.");
            }

            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            foreach (var modifier in modifiers)
            {
                fieldDeclaration = fieldDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
            }

            return this;
        }

        public EasyFieldBuilder WithInitialValue<T>(T value)
        {
            if (fieldDeclaration == null)
            {
                throw new InvalidOperationException("Field type must be set before setting an initial value.");
            }

            var initializer = SyntaxFactory.EqualsValueClause(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(value.ToString())));
            fieldDeclaration = fieldDeclaration.WithDeclaration(
                fieldDeclaration.Declaration.WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        fieldDeclaration.Declaration.Variables[0].WithInitializer(initializer))));

            return this;
        }

        public BaseFieldDeclarationSyntax Build()
        {
            if (fieldDeclaration == null)
            {
                throw new InvalidOperationException("Field type must be set before building.");
            }

            return fieldDeclaration;
        }
    }
}