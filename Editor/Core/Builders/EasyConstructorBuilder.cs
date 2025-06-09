using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyConstructorBuilder : EasyBasicBuilder
    {
        private string className;
        private (string, string)[] parameters;
        private SyntaxKind[] modifiers;
        private string[] statements;

        public EasyConstructorBuilder(SyntaxNode templateRoot = null) : base(templateRoot)
        {
        }

        public EasyConstructorBuilder WithClassName(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                throw new ArgumentException("Class name cannot be null or empty.", nameof(className));
            }

            this.className = className;
            return this;
        }

        public EasyConstructorBuilder WithBody(params string[] statements)
        {
            if (statements == null || statements.Length == 0)
            {
                throw new ArgumentException("At least one statement must be provided for the constructor body.",
                    nameof(statements));
            }

            this.statements = statements;
            return this;
        }

        public EasyConstructorBuilder WithParameters(params (string, string)[] parameters)
        {
            this.parameters = parameters ?? throw new ArgumentException("Parameters cannot be null.", nameof(parameters));
            return this;
        }

        public EasyConstructorBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            this.modifiers = modifiers;
            return this;
        }

        public EasyConstructorBuilder WithBodyFromTemplate(string constructorClassName = null)
        {
            string targetClassName = constructorClassName ?? className;

            if (string.IsNullOrWhiteSpace(targetClassName))
            {
                throw new ArgumentException("Class name cannot be null or empty when getting body from template.",
                    nameof(targetClassName));
            }

            if (templateRoot == null)
            {
                throw new InvalidOperationException("Template root is not set. Please set a template first.");
            }

            var constructor = templateRoot.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == targetClassName);

            if (constructor?.Body == null)
            {
                throw new InvalidOperationException($"Constructor for type '{targetClassName}' not found in the template.");
            }

            statements = constructor.Body.Statements
                .Select(stmt => stmt.ToFullString())
                .Where(stmt => !string.IsNullOrWhiteSpace(stmt))
                .ToArray();

            parameters = constructor.ParameterList.Parameters
                .Select(p => (p.Type?.ToString(), p.Identifier.Text)).ToArray();

            return this;
        }

        public EasyConstructorBuilder ReplaceInBody(string searchText, string replaceText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Search text cannot be null or empty.", nameof(searchText));
            }

            if (replaceText == null)
            {
                throw new ArgumentNullException(nameof(replaceText), "Replace text cannot be null.");
            }

            if (statements == null)
            {
                throw new InvalidOperationException("Constructor body is not set. Please set a body first.");
            }

            statements = statements.Select(stmt => stmt.Replace(searchText, replaceText)).ToArray();
            return this;
        }

        protected override MemberDeclarationSyntax BuildDeclarationSyntax()
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                throw new InvalidOperationException(
                    "Class name is not set. Please set a class name first.");
            }

            if (statements == null || statements.Length == 0)
            {
                throw new InvalidOperationException(
                    "Constructor body is not set. Please set a body first.");
            }

            var parameterList = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(parameters?.Select(p =>
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Item2))
                        .WithType(SyntaxFactory.ParseTypeName(p.Item1))) ?? Array.Empty<ParameterSyntax>()));

            var syntaxStatements = statements
                .Where(stmt => !string.IsNullOrWhiteSpace(stmt))
                .Select(stmt => SyntaxFactory.ParseStatement(stmt.Trim()))
                .ToArray();

            ConstructorDeclarationSyntax constructorDeclaration =
                SyntaxFactory.ConstructorDeclaration(className)
                    .WithParameterList(parameterList)
                    .WithBody(SyntaxFactory.Block(syntaxStatements));

            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    constructorDeclaration = constructorDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
                }
            }

            return constructorDeclaration;
        }
    }
}