using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyFieldBuilder : EasyBasicBuilder<EasyFieldBuilder>
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

            var variableDeclarator = SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(name));
            
            // Add initializer if an initial value was set
            if (initialValueSet)
            {
                variableDeclarator = variableDeclarator.WithInitializer(
                    SyntaxFactory.EqualsValueClause(CreateExpressionFromValue(initialValue)));
            }

            var fieldDeclaration = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(type))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList(variableDeclarator)));

            // Add modifiers if any
            if (modifiers != null)
            {
                fieldDeclaration = fieldDeclaration.AddModifiers(modifiers.Select(SyntaxFactory.Token).ToArray());
            }

            return fieldDeclaration;
        }

        private ExpressionSyntax CreateExpressionFromValue(object value)
        {
            return value switch
            {
                null => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression),
                string s => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s)),
                bool b => SyntaxFactory.LiteralExpression(b ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression),
                int i => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i)),
                float f => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(f)),
                double d => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d)),
                _ => SyntaxFactory.ParseExpression(value.ToString())
            };
        }
    }
}