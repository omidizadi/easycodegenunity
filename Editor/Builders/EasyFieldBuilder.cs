using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    /// <summary>
    /// A builder class for creating field declarations.
    /// </summary>
    public class EasyFieldBuilder : EasyBasicBuilder<EasyFieldBuilder>
    {
        private string name;
        private string type;
        private SyntaxKind[] modifiers;
        private object initialValue;
        private bool initialValueSet = false;

        /// <summary>
        /// Sets the name of the field.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The EasyFieldBuilder instance for chaining.</returns>
        public EasyFieldBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Field name cannot be null or empty.", nameof(name));
            }

            this.name = name;
            return this;
        }

        /// <summary>
        /// Sets the type of the field.
        /// </summary>
        /// <param name="type">The type of the field.</param>
        /// <returns>The EasyFieldBuilder instance for chaining.</returns>
        public EasyFieldBuilder WithType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Type cannot be null or empty.", nameof(type));
            }

            this.type = type;
            return this;
        }

        /// <summary>
        /// Sets the modifiers of the field.
        /// </summary>
        /// <param name="modifiers">The modifiers of the field.</param>
        /// <returns>The EasyFieldBuilder instance for chaining.</returns>
        public EasyFieldBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            this.modifiers = modifiers;
            return this;
        }

        /// <summary>
        /// Sets the initial value of the field.
        /// </summary>
        /// <param name="value">The initial value of the field.</param>
        /// <typeparam name="T">The type of the initial value.</typeparam>
        /// <returns>The EasyFieldBuilder instance for chaining.</returns>
        public EasyFieldBuilder WithInitialValue<T>(T value)
        {
            initialValue = value;
            initialValueSet = true;
            return this;
        }

        /// <summary>
        /// Builds the field declaration syntax.
        /// </summary>
        /// <returns>The created field declaration syntax.</returns>
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