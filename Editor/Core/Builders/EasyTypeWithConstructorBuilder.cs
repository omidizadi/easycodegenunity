using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public abstract class EasyTypeWithConstructorBuilder : EasyTypeBuilder
    {
        private string constructorBody;
        private (string, string)[] constructorParameters;

        public EasyTypeWithConstructorBuilder WithConstructor(string body, params (string, string)[] parameters)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException("Constructor body cannot be null or empty.", nameof(body));
            }

            constructorBody = body;
            constructorParameters =
                parameters ?? throw new ArgumentException("Parameters cannot be null.", nameof(parameters));
            return this;
        }

        public EasyTypeWithConstructorBuilder WithConstructorFromTemplate()
        {
            // extract the constructor from the template root, if null, throw an exception
            if (templateRoot == null)
            {
                throw new InvalidOperationException("Template root is not set. Please set a template first.");
            }

            var constructor = templateRoot.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == Name);

            if (constructor?.Body == null)
            {
                throw new InvalidOperationException($"Constructor for type '{Name}' not found in the template.");
            }

            constructorBody = constructor.Body.ToString();
            constructorParameters = constructor.ParameterList.Parameters
                .Select(p => (p.Type?.ToString(), p.Identifier.Text)).ToArray();

            return this;
        }

        public EasyTypeWithConstructorBuilder ReplaceInConstructorBody(string search, string replace)
        {
            if (string.IsNullOrWhiteSpace(constructorBody))
            {
                throw new InvalidOperationException("Constructor body is not set. Please set it before replacing.");
            }

            if (string.IsNullOrWhiteSpace(search) || string.IsNullOrWhiteSpace(replace))
            {
                throw new ArgumentException("Search and replace strings cannot be null or empty.");
            }

            constructorBody = constructorBody.Replace(search, replace);
            return this;
        }
        
        protected bool HasConstructor()
        {
            return !string.IsNullOrWhiteSpace(constructorBody) && constructorParameters != null;
        }

        protected ConstructorDeclarationSyntax CreateConstructorDeclaration()
        {
            var parameters = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(constructorParameters.Select(p =>
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Item2))
                        .WithType(SyntaxFactory.ParseTypeName(p.Item1)))));

            return SyntaxFactory.ConstructorDeclaration(Name)
                .WithParameterList(parameters)
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(constructorBody)));
        }
    }
}
