using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyConstructorBuilder : EasyBasicBuilder<EasyConstructorBuilder>
    {
        private string className;
        private (string, string)[] parameters;
        private SyntaxKind[] modifiers;
        private string[] statements;
        private bool useExpressionBody = false;
        private string expressionBody;

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
            this.useExpressionBody = false;
            return this;
        }

        public EasyConstructorBuilder WithExpressionBody(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));
            }

            // Ensure we don't accidentally include a return statement in expression-bodied members
            string cleanExpression = expression.Replace("return ", "").Replace(";", "").Trim();
            this.expressionBody = cleanExpression;
            this.useExpressionBody = true;
            this.statements = null;
            return this;
        }

        public EasyConstructorBuilder WithParameter(string type, string name)
        {
            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Parameter type and name cannot be null or empty.");
            }

            parameters ??= Array.Empty<(string, string)>();

            if (parameters.Any(p => p.Item2 == name))
            {
                throw new InvalidOperationException($"Parameter '{name}' already exists.");
            }

            parameters = parameters.Append((type, name)).ToArray();
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

            if (constructor == null)
            {
                throw new InvalidOperationException($"Constructor for type '{targetClassName}' not found in the template.");
            }

            parameters = constructor.ParameterList.Parameters
                .Select(p => (p.Type?.ToString(), p.Identifier.Text)).ToArray();

            if (constructor.ExpressionBody != null)
            {
                // Handle expression-bodied constructors
                expressionBody = constructor.ExpressionBody.Expression.ToString();
                useExpressionBody = true;
                statements = null;
            }
            else if (constructor.Body != null)
            {
                // Handle normal block-bodied constructors
                statements = constructor.Body.Statements
                    .Select(stmt => stmt.ToFullString())
                    .Where(stmt => !string.IsNullOrWhiteSpace(stmt))
                    .ToArray();
                useExpressionBody = false;
            }
            else
            {
                throw new InvalidOperationException($"Constructor for type '{targetClassName}' has no body or expression body.");
            }

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

            if (useExpressionBody)
            {
                if (expressionBody == null)
                {
                    throw new InvalidOperationException("Constructor expression body is not set.");
                }

                expressionBody = expressionBody.Replace(searchText, replaceText);
            }
            else
            {
                if (statements == null)
                {
                    throw new InvalidOperationException("Constructor body is not set. Please set a body first.");
                }

                statements = statements.Select(stmt => stmt.Replace(searchText, replaceText)).ToArray();
            }

            return this;
        }

        protected override MemberDeclarationSyntax BuildDeclarationSyntax()
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                throw new InvalidOperationException(
                    "Class name is not set. Please set a class name first.");
            }

            var parameterList = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(parameters?.Select(p =>
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Item2))
                        .WithType(SyntaxFactory.ParseTypeName(p.Item1))) ?? Array.Empty<ParameterSyntax>()));

            ConstructorDeclarationSyntax constructorDeclaration =
                SyntaxFactory.ConstructorDeclaration(className)
                    .WithParameterList(parameterList);

            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    constructorDeclaration = constructorDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
                }
            }

            if (useExpressionBody)
            {
                if (string.IsNullOrWhiteSpace(expressionBody))
                {
                    throw new InvalidOperationException(
                        "Constructor expression body is not set. Please set an expression body first.");
                }

                constructorDeclaration = constructorDeclaration
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression(expressionBody)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }
            else
            {
                if (statements == null || statements.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Constructor body is not set. Please set a body first.");
                }

                var syntaxStatements = statements
                    .Where(stmt => !string.IsNullOrWhiteSpace(stmt))
                    .Select(stmt => SyntaxFactory.ParseStatement(stmt.Trim()))
                    .ToArray();

                constructorDeclaration = constructorDeclaration.WithBody(SyntaxFactory.Block(syntaxStatements));
            }

            return constructorDeclaration;
        }
    }
}