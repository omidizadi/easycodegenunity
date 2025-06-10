using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyMethodBuilder : EasyBasicBuilder<EasyMethodBuilder>
    {
        private string name;
        private string returnType;
        private (string, string)[] parameters;
        private SyntaxKind[] modifiers;
        private string[] statements;
        private bool useExpressionBody = false;
        private string expressionBody;

        public EasyMethodBuilder(SyntaxNode templateRoot = null) : base(templateRoot)
        {
        }

        public EasyMethodBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Method name cannot be null or empty.", nameof(name));
            }

            this.name = name;
            return this;
        }

        public EasyMethodBuilder WithReturnType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Return type cannot be null or empty.", nameof(type));
            }

            returnType = type;
            return this;
        }

        public EasyMethodBuilder WithBody(params string[] bodyStatements)
        {
            if (bodyStatements == null || bodyStatements.Length == 0)
            {
                throw new ArgumentException("Method body statements cannot be null or empty.", nameof(bodyStatements));
            }

            this.statements = bodyStatements;
            this.useExpressionBody = false;
            this.expressionBody = null;
            return this;
        }

        public EasyMethodBuilder WithBodyLine(string statement)
        {
            if (string.IsNullOrWhiteSpace(statement))
            {
                throw new ArgumentException("Statement cannot be null or empty.", nameof(statement));
            }

            if (statements == null)
            {
                statements = new[] { statement };
            }
            else
            {
                var newStatements = new string[statements.Length + 1];
                Array.Copy(statements, newStatements, statements.Length);
                newStatements[statements.Length] = statement;
                statements = newStatements;
            }

            this.useExpressionBody = false;
            this.expressionBody = null;
            return this;
        }

        public EasyMethodBuilder WithBodyLines(params string[] bodyStatements)
        {
            return WithBody(bodyStatements);
        }

        public EasyMethodBuilder WithExpressionBody(string expression)
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

        public EasyMethodBuilder WithParameters(params (string, string)[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                throw new ArgumentException("Parameters cannot be null or empty.", nameof(parameters));
            }

            this.parameters = parameters;
            return this;
        }

        public EasyMethodBuilder WithParameter(string type, string name)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Type cannot be null or empty.", nameof(type));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }

            if (parameters == null)
            {
                parameters = new[] { (type, name) };
            }
            else
            {
                var newParams = new (string, string)[parameters.Length + 1];
                Array.Copy(parameters, newParams, parameters.Length);
                newParams[parameters.Length] = (type, name);
                parameters = newParams;
            }

            return this;
        }

        public EasyMethodBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            this.modifiers = modifiers;
            return this;
        }

        public EasyMethodBuilder WithBodyFromTemplate(string methodNameInTemplate)
        {
            if (string.IsNullOrWhiteSpace(methodNameInTemplate))
            {
                throw new ArgumentException("Method name in template cannot be null or empty.",
                    nameof(methodNameInTemplate));
            }

            if (methodNameInTemplate == null)
            {
                throw new ArgumentNullException(nameof(methodNameInTemplate), "Method body cannot be null.");
            }

            if (templateRoot == null)
            {
                throw new InvalidOperationException("Template root is not set. Please set a template first.");
            }

            var method = templateRoot.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == methodNameInTemplate);

            if (method == null)
            {
                throw new Exception($"Method '{methodNameInTemplate}' not found in template.");
            }

            if (method.ExpressionBody != null)
            {
                // Handle expression-bodied methods
                expressionBody = method.ExpressionBody.Expression.ToString();
                useExpressionBody = true;
                statements = null;
            }
            else if (method.Body != null)
            {
                // Handle normal block-bodied methods
                statements = method.Body.Statements
                    .Select(stmt => stmt.ToFullString())
                    .Where(stmt => !string.IsNullOrWhiteSpace(stmt))
                    .ToArray();
                useExpressionBody = false;
                expressionBody = null;
            }
            else
            {
                throw new Exception($"Method '{methodNameInTemplate}' has no body or expression body.");
            }

            return this;
        }

        public EasyMethodBuilder ReplaceInBody(string searchText, string replaceText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Search text cannot be null or empty.", nameof(searchText));
            }

            if (string.IsNullOrWhiteSpace(replaceText))
            {
                throw new ArgumentException("Replace text cannot be null or empty.", nameof(replaceText));
            }

            if (useExpressionBody)
            {
                if (expressionBody == null)
                {
                    throw new InvalidOperationException("Method expression body is not set.");
                }

                expressionBody = expressionBody.Replace(searchText, replaceText);
            }
            else
            {
                if (statements == null || statements.Length == 0)
                {
                    throw new InvalidOperationException("Method body is not set. Please set a body first.");
                }

                statements = statements.Select(stmt => stmt.Replace(searchText, replaceText)).ToArray();
            }

            return this;
        }

        protected override MemberDeclarationSyntax BuildDeclarationSyntax()
        {
            if (string.IsNullOrWhiteSpace(returnType))
            {
                throw new InvalidOperationException(
                    "Method declaration is not set. Please set a method name and return type first.");
            }

            MethodDeclarationSyntax methodDeclaration =
                SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), name);

            if (parameters != null)
            {
                var parameterList = SyntaxFactory.ParameterList(
                    SyntaxFactory.SeparatedList(parameters.Select(p =>
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Item2))
                            .WithType(SyntaxFactory.ParseTypeName(p.Item1)))));
                methodDeclaration = methodDeclaration.WithParameterList(parameterList);
            }

            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
                }
            }

            if (useExpressionBody)
            {
                if (string.IsNullOrWhiteSpace(expressionBody))
                {
                    throw new InvalidOperationException(
                        "Method expression body is not set. Please set an expression body first.");
                }

                // Create expression-bodied method
                methodDeclaration = methodDeclaration
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression(expressionBody)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }
            else
            {
                if (statements == null || statements.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Method body is not set. Please set a body first.");
                }

                // Create block-bodied method with multiple statements
                var syntaxStatements = statements
                    .Where(stmt => !string.IsNullOrWhiteSpace(stmt))
                    .Select(stmt => SyntaxFactory.ParseStatement(stmt.Trim()))
                    .ToArray();

                methodDeclaration = methodDeclaration.WithBody(SyntaxFactory.Block(syntaxStatements));
            }

            return methodDeclaration;
        }
    }
}