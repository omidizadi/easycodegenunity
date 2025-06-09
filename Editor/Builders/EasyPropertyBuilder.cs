using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyPropertyBuilder : EasyBasicBuilder
    {
        private string name;
        private string type;
        private SyntaxKind[] modifiers;
        private SyntaxKind[] getterModifiers;
        private SyntaxKind[] setterModifiers;
        private string getterBody;
        private string setterBody;
        private bool hasGetter = true;
        private bool hasSetter = true;

        public EasyPropertyBuilder(SyntaxNode templateRoot = null) : base(templateRoot)
        {
        }

        public EasyPropertyBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Property name cannot be null or empty.", nameof(name));
            }

            this.name = name;
            return this;
        }

        public EasyPropertyBuilder WithType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Type cannot be null or empty.", nameof(type));
            }

            this.type = type;
            return this;
        }

        public EasyPropertyBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            this.modifiers = modifiers;
            return this;
        }

        public EasyPropertyBuilder WithGetterModifiers(params SyntaxKind[] modifiers)
        {
            this.getterModifiers = modifiers;
            return this;
        }

        public EasyPropertyBuilder WithSetterModifiers(params SyntaxKind[] modifiers)
        {
            this.setterModifiers = modifiers;
            return this;
        }

        public EasyPropertyBuilder WithGetterBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException("Getter body cannot be null or empty.", nameof(body));
            }

            this.getterBody = body;
            return this;
        }

        public EasyPropertyBuilder WithSetterBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException("Setter body cannot be null or empty.", nameof(body));
            }

            this.setterBody = body;
            return this;
        }

        public EasyPropertyBuilder WithGetterFromTemplate(string propertyNameInTemplate)
        {
            if (string.IsNullOrWhiteSpace(propertyNameInTemplate))
            {
                throw new ArgumentException("Property name in template cannot be null or empty.",
                    nameof(propertyNameInTemplate));
            }

            if (templateRoot == null)
            {
                throw new InvalidOperationException("Template root is not set. Please set a template first.");
            }

            var property = templateRoot.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == propertyNameInTemplate);

            if (property == null)
            {
                throw new Exception($"Property '{propertyNameInTemplate}' not found in template.");
            }

            var getter =
                property.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
            if (getter == null)
            {
                throw new Exception($"Getter not found in property '{propertyNameInTemplate}'.");
            }

            if (getter.Body != null)
            {
                getterBody = getter.Body.ToString().TrimStart('{').TrimEnd('}').Trim();
            }
            else if (getter.ExpressionBody != null)
            {
                getterBody = $"return {getter.ExpressionBody.Expression.ToString()};";
            }
            else
            {
                getterBody = string.Empty;
            }

            return this;
        }

        public EasyPropertyBuilder WithSetterFromTemplate(string propertyNameInTemplate)
        {
            if (string.IsNullOrWhiteSpace(propertyNameInTemplate))
            {
                throw new ArgumentException("Property name in template cannot be null or empty.",
                    nameof(propertyNameInTemplate));
            }

            if (templateRoot == null)
            {
                throw new InvalidOperationException("Template root is not set. Please set a template first.");
            }

            var property = templateRoot.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == propertyNameInTemplate);

            if (property == null)
            {
                throw new Exception($"Property '{propertyNameInTemplate}' not found in template.");
            }

            var setter =
                property.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
            if (setter == null)
            {
                throw new Exception($"Setter not found in property '{propertyNameInTemplate}'.");
            }

            if (setter.Body != null)
            {
                setterBody = setter.Body.ToString().TrimStart('{').TrimEnd('}').Trim();
            }
            else if (setter.ExpressionBody != null)
            {
                setterBody = $"{setter.ExpressionBody.Expression.ToString()};";
            }
            else
            {
                setterBody = string.Empty;
            }

            return this;
        }

        public EasyPropertyBuilder ReplaceInGetterBody(string searchText, string replaceText)
        {
            if (string.IsNullOrWhiteSpace(getterBody))
            {
                throw new InvalidOperationException("Getter body is not set. Please set a getter body first.");
            }

            getterBody = getterBody.Replace(searchText, replaceText);
            return this;
        }

        public EasyPropertyBuilder ReplaceInSetterBody(string searchText, string replaceText)
        {
            if (string.IsNullOrWhiteSpace(setterBody))
            {
                throw new InvalidOperationException("Setter body is not set. Please set a setter body first.");
            }

            setterBody = setterBody.Replace(searchText, replaceText);
            return this;
        }

        public EasyPropertyBuilder WithoutGetter()
        {
            hasGetter = false;
            return this;
        }

        public EasyPropertyBuilder WithoutSetter()
        {
            hasSetter = false;
            return this;
        }

        protected override MemberDeclarationSyntax BuildDeclarationSyntax()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Property name must be set before building.");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new InvalidOperationException("Property type must be set before building.");
            }

            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(type),
                SyntaxFactory.Identifier(name));

            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    propertyDeclaration = propertyDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
                }
            }

            List<AccessorDeclarationSyntax> accessors = new List<AccessorDeclarationSyntax>();

            if (hasGetter)
            {
                var getAccessorDeclaration = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);

                if (getterModifiers != null)
                {
                    foreach (var modifier in getterModifiers)
                    {
                        getAccessorDeclaration = getAccessorDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
                    }
                }

                if (!string.IsNullOrWhiteSpace(getterBody))
                {
                    getAccessorDeclaration =
                        getAccessorDeclaration.WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(getterBody)));
                }
                else
                {
                    getAccessorDeclaration =
                        getAccessorDeclaration.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                }

                accessors.Add(getAccessorDeclaration);
            }

            if (hasSetter)
            {
                var setAccessorDeclaration = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration);

                if (setterModifiers != null)
                {
                    foreach (var modifier in setterModifiers)
                    {
                        setAccessorDeclaration = setAccessorDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
                    }
                }

                if (!string.IsNullOrWhiteSpace(setterBody))
                {
                    setAccessorDeclaration =
                        setAccessorDeclaration.WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(setterBody)));
                }
                else
                {
                    setAccessorDeclaration =
                        setAccessorDeclaration.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                }

                accessors.Add(setAccessorDeclaration);
            }

            if (accessors.Count > 0)
            {
                var accessorList = SyntaxFactory.AccessorList(SyntaxFactory.List(accessors));
                propertyDeclaration = propertyDeclaration.WithAccessorList(accessorList);
            }

            return propertyDeclaration;
        }
    }
}