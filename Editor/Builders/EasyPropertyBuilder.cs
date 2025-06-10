using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    /// <summary>
    /// A builder class for creating property declarations.
    /// </summary>
    public class EasyPropertyBuilder : EasyBasicBuilder<EasyPropertyBuilder>
    {
        private string name;
        private string type;
        private SyntaxKind[] modifiers;
        private SyntaxKind[] getterModifiers;
        private SyntaxKind[] setterModifiers;
        private string[] getterStatements;
        private string[] setterStatements;
        private bool hasGetter = true;
        private bool hasSetter = true;
        private string backingFieldName;
        private bool useExpressionBodiedGetter = false;
        private bool useExpressionBodiedSetter = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyPropertyBuilder"/> class.
        /// </summary>
        /// <param name="templateRoot">The template root.</param>
        public EasyPropertyBuilder(SyntaxNode templateRoot = null) : base(templateRoot)
        {
        }

        /// <summary>
        /// Sets the name of the property.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Property name cannot be null or empty.", nameof(name));
            }

            this.name = name;
            return this;
        }

        /// <summary>
        /// Sets the type of the property.
        /// </summary>
        /// <param name="type">The type of the property.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Type cannot be null or empty.", nameof(type));
            }

            this.type = type;
            return this;
        }

        /// <summary>
        /// Sets the modifiers of the property.
        /// </summary>
        /// <param name="modifiers">The modifiers of the property.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            this.modifiers = modifiers;
            return this;
        }

        /// <summary>
        /// Sets the getter modifiers of the property.
        /// </summary>
        /// <param name="modifiers">The getter modifiers of the property.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithGetterModifiers(params SyntaxKind[] modifiers)
        {
            this.getterModifiers = modifiers;
            return this;
        }

        /// <summary>
        /// Sets the setter modifiers of the property.
        /// </summary>
        /// <param name="modifiers">The setter modifiers of the property.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithSetterModifiers(params SyntaxKind[] modifiers)
        {
            this.setterModifiers = modifiers;
            return this;
        }

        /// <summary>
        /// Sets the getter body of the property.
        /// </summary>
        /// <param name="statements">The getter statements of the property.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithGetterBody(params string[] statements)
        {
            if (statements == null || statements.Length == 0)
            {
                throw new ArgumentException("Getter statements cannot be null or empty.", nameof(statements));
            }

            getterStatements = statements;
            useExpressionBodiedGetter = false;
            return this;
        }

        /// <summary>
        /// Sets the setter body of the property.
        /// </summary>
        /// <param name="statements">The setter statements of the property.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithSetterBody(params string[] statements)
        {
            if (statements == null || statements.Length == 0)
            {
                throw new ArgumentException("Setter statements cannot be null or empty.", nameof(statements));
            }

            setterStatements = statements;
            useExpressionBodiedSetter = false;
            return this;
        }

        /// <summary>
        /// Sets the expression bodied getter of the property.
        /// </summary>
        /// <param name="expression">The expression for the getter.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithExpressionBodiedGetter(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));
            }

            // Ensure we don't accidentally include a return statement in expression-bodied members
            string cleanExpression = expression.Replace("return ", "").Replace(";", "").Trim();
            getterStatements = new[] { cleanExpression };
            useExpressionBodiedGetter = true;
            return this;
        }

        /// <summary>
        /// Sets the expression bodied setter of the property.
        /// </summary>
        /// <param name="expression">The expression for the setter.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithExpressionBodiedSetter(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));
            }

            // Remove semicolon if present
            string cleanExpression = expression.Replace(";", "").Trim();
            setterStatements = new[] { cleanExpression };
            useExpressionBodiedSetter = true;
            return this;
        }

        /// <summary>
        /// Sets the backing field of the property.
        /// </summary>
        /// <param name="fieldName">The name of the backing field.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithBackingField(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentException("Backing field name cannot be null or empty.", nameof(fieldName));
            }

            backingFieldName = fieldName;
            return this;
        }

        /// <summary>
        /// Sets the property to be an auto property.
        /// </summary>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithAutoProperty()
        {
            getterStatements = null;
            setterStatements = null;
            return this;
        }

        /// <summary>
        /// Sets the getter from template.
        /// </summary>
        /// <param name="propertyNameInTemplate">The property name in template.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
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
                string body = getter.Body.ToString().TrimStart('{').TrimEnd('}').Trim();
                getterStatements = new[] { body };
                useExpressionBodiedGetter = false;
            }
            else if (getter.ExpressionBody != null)
            {
                string expression = getter.ExpressionBody.Expression.ToString();
                getterStatements = new[] { expression };
                useExpressionBodiedGetter = true;
            }
            else
            {
                getterStatements = null;
                useExpressionBodiedGetter = false;
            }

            return this;
        }

        /// <summary>
        /// Sets the setter from template.
        /// </summary>
        /// <param name="propertyNameInTemplate">The property name in template.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
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
                string body = setter.Body.ToString().TrimStart('{').TrimEnd('}').Trim();
                setterStatements = new[] { body };
                useExpressionBodiedSetter = false;
            }
            else if (setter.ExpressionBody != null)
            {
                string expression = setter.ExpressionBody.Expression.ToString();
                setterStatements = new[] { expression };
                useExpressionBodiedSetter = true;
            }
            else
            {
                setterStatements = null;
                useExpressionBodiedSetter = false;
            }

            return this;
        }

        /// <summary>
        /// Replaces text in the getter body.
        /// </summary>
        /// <param name="searchText">The text to search for.</param>
        /// <param name="replaceText">The text to replace with.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder ReplaceInGetterBody(string searchText, string replaceText)
        {
            if (getterStatements == null || getterStatements.Length == 0)
            {
                throw new InvalidOperationException("Getter body is not set. Please set a getter body first.");
            }

            for (int i = 0; i < getterStatements.Length; i++)
            {
                getterStatements[i] = getterStatements[i].Replace(searchText, replaceText);
            }
            
            return this;
        }

        /// <summary>
        /// Replaces text in the setter body.
        /// </summary>
        /// <param name="searchText">The text to search for.</param>
        /// <param name="replaceText">The text to replace with.</param>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder ReplaceInSetterBody(string searchText, string replaceText)
        {
            if (setterStatements == null || setterStatements.Length == 0)
            {
                throw new InvalidOperationException("Setter body is not set. Please set a setter body first.");
            }

            for (int i = 0; i < setterStatements.Length; i++)
            {
                setterStatements[i] = setterStatements[i].Replace(searchText, replaceText);
            }
            
            return this;
        }

        /// <summary>
        /// Removes the getter from the property.
        /// </summary>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithoutGetter()
        {
            hasGetter = false;
            return this;
        }

        /// <summary>
        /// Removes the setter from the property.
        /// </summary>
        /// <returns>The EasyPropertyBuilder instance for chaining.</returns>
        public EasyPropertyBuilder WithoutSetter()
        {
            hasSetter = false;
            return this;
        }

        /// <summary>
        /// Builds the property declaration syntax.
        /// </summary>
        /// <returns>The created property declaration syntax.</returns>
        protected override MemberDeclarationSyntax BuildDeclarationSyntax()
        {
            ValidateRequiredProperties();
            
            // Create property declaration
            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(type),
                SyntaxFactory.Identifier(name));

            // Add modifiers to the property
            propertyDeclaration = ApplyModifiers(propertyDeclaration);
            
            // Handle backing field default implementation
            HandleBackingFieldDefaults();

            // Handle different property types
            if (IsAutoProperty())
            {
                return BuildAutoProperty(propertyDeclaration);
            }
            else
            {
                return BuildCustomProperty(propertyDeclaration);
            }
        }

        private void ValidateRequiredProperties()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Property name must be set before building.");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new InvalidOperationException("Property type must be set before building.");
            }
        }

        private PropertyDeclarationSyntax ApplyModifiers(PropertyDeclarationSyntax declaration)
        {
            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    declaration = declaration.AddModifiers(SyntaxFactory.Token(modifier));
                }
            }
            
            return declaration;
        }

        private void HandleBackingFieldDefaults()
        {
            // If we're using backing field and no explicit getter/setter statements are provided, create default implementations
            if (!string.IsNullOrWhiteSpace(backingFieldName) && 
                (getterStatements == null || getterStatements.Length == 0) && 
                (setterStatements == null || setterStatements.Length == 0))
            {
                getterStatements = new[] { backingFieldName };
                useExpressionBodiedGetter = true;
                
                setterStatements = new[] { $"{backingFieldName} = value" };
                useExpressionBodiedSetter = true;
            }
        }

        private bool IsAutoProperty()
        {
            return (getterStatements == null || getterStatements.Length == 0) && 
                   (setterStatements == null || setterStatements.Length == 0) && 
                   string.IsNullOrWhiteSpace(backingFieldName);
        }

        private PropertyDeclarationSyntax BuildAutoProperty(PropertyDeclarationSyntax declaration)
        {
            List<AccessorDeclarationSyntax> accessors = new List<AccessorDeclarationSyntax>();

            if (hasGetter)
            {
                accessors.Add(CreateAutoAccessor(SyntaxKind.GetAccessorDeclaration, getterModifiers));
            }

            if (hasSetter)
            {
                accessors.Add(CreateAutoAccessor(SyntaxKind.SetAccessorDeclaration, setterModifiers));
            }

            var accessorList = SyntaxFactory.AccessorList(SyntaxFactory.List(accessors));
            return declaration.WithAccessorList(accessorList);
        }

        private PropertyDeclarationSyntax BuildCustomProperty(PropertyDeclarationSyntax declaration)
        {
            List<AccessorDeclarationSyntax> customAccessors = new List<AccessorDeclarationSyntax>();

            // Build getter
            if (hasGetter)
            {
                customAccessors.Add(CreateCustomAccessor(
                    SyntaxKind.GetAccessorDeclaration, 
                    getterStatements, 
                    useExpressionBodiedGetter,
                    getterModifiers));
            }

            // Build setter
            if (hasSetter)
            {
                customAccessors.Add(CreateCustomAccessor(
                    SyntaxKind.SetAccessorDeclaration, 
                    setterStatements, 
                    useExpressionBodiedSetter,
                    setterModifiers));
            }

            // Add accessors to the property
            var customAccessorList = SyntaxFactory.AccessorList(SyntaxFactory.List(customAccessors));
            return declaration.WithAccessorList(customAccessorList);
        }

        private AccessorDeclarationSyntax CreateAutoAccessor(SyntaxKind accessorKind, SyntaxKind[] modifiers)
        {
            var accessor = SyntaxFactory.AccessorDeclaration(accessorKind)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                
            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    accessor = accessor.AddModifiers(SyntaxFactory.Token(modifier));
                }
            }
            
            return accessor;
        }

        private AccessorDeclarationSyntax CreateCustomAccessor(
            SyntaxKind accessorKind, 
            string[] statements, 
            bool useExpressionBodied,
            SyntaxKind[] modifiers)
        {
            AccessorDeclarationSyntax accessor;
            
            if (useExpressionBodied && statements != null && statements.Length == 1)
            {
                // Create expression-bodied accessor
                accessor = SyntaxFactory.AccessorDeclaration(accessorKind)
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression(statements[0])))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }
            else
            {
                // Create block-bodied accessor
                accessor = SyntaxFactory.AccessorDeclaration(accessorKind);
                
                if (statements != null && statements.Length > 0)
                {
                    // If it's a single statement that might contain multiple lines (from a template),
                    // we need to parse it as a statement
                    if (statements.Length == 1)
                    {
                        accessor = accessor.WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(statements[0])));
                    }
                    else
                    {
                        // Create block with multiple statements
                        var parsedStatements = statements
                            .Select(stmt => SyntaxFactory.ParseStatement(stmt))
                            .ToArray();
                        
                        accessor = accessor.WithBody(SyntaxFactory.Block(parsedStatements));
                    }
                }
                else
                {
                    accessor = accessor.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                }
            }
            
            // Apply modifiers
            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    accessor = accessor.AddModifiers(SyntaxFactory.Token(modifier));
                }
            }
            
            return accessor;
        }
    }
}