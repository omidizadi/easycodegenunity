using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyMethodBuilder
    {
        private SyntaxNode templateRoot;
        private string name;
        private BaseMethodDeclarationSyntax methodDeclaration;
        private string body;

        public EasyMethodBuilder(SyntaxNode templateRoot)
        {
            this.templateRoot = templateRoot;
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

            methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(type), name);

            return this;
        }

        public EasyMethodBuilder WithBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException("Method body cannot be null or empty.", nameof(body));
            }

            this.body = body;
            return this;
        }

        public EasyMethodBuilder WithParameters(params (string, string)[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                throw new ArgumentException("Parameters cannot be null or empty.", nameof(parameters));
            }

            if (methodDeclaration == null)
            {
                throw new InvalidOperationException(
                    "Method name and return type must be set before adding parameters.");
            }

            var parameterList = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(parameters.Select(p =>
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Item2))
                        .WithType(SyntaxFactory.ParseTypeName(p.Item1)))));
            methodDeclaration = methodDeclaration.WithParameterList(parameterList);

            return this;
        }

        public EasyMethodBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            if (methodDeclaration == null)
            {
                throw new InvalidOperationException("Method name and return type must be set before adding modifiers.");
            }

            foreach (var modifier in modifiers)
            {
                methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
            }

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
                throw new Exception("Method body cannot be null.");
            }

            body = method.Body?.ToString() ?? string.Empty;
            body = body.TrimStart('{').TrimEnd('}');
            body = body.Trim();
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

            if (body == null)
            {
                throw new InvalidOperationException("Method body is not set. Please set a body first.");
            }

            body = body.Replace(searchText, replaceText);
            return this;
        }

        public BaseMethodDeclarationSyntax Build()
        {
            if (methodDeclaration == null)
            {
                throw new InvalidOperationException(
                    "Method declaration is not set. Please set a method name and return type first.");
            }

            var bodySyntax = SyntaxFactory.Block(SyntaxFactory.ParseStatement(body));
            methodDeclaration = methodDeclaration.WithBody(bodySyntax);

            return methodDeclaration;
        }
    }
}