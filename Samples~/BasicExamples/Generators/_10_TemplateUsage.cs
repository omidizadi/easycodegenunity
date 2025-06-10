using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;
using easycodegenunity.Editor.Samples.BasicExamples.Templates;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// This example demonstrates how to use templates for code generation.
    /// It shows using template body for a constructor, property getter/setter, and methods.
    /// </summary>
    public class _10_TemplateUsage : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .WithTemplate<BasicTemplate>() // Set the template for this generator
                .AddUsingStatement("System")
                .AddNamespace("easycodegenunity.Editor.Samples.BasicExamples.Generated")
                .AddClass(typeBuilder => typeBuilder
                    .WithName("PersonFromTemplate")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())

                // Add fields that will be used by methods from template
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("name")
                    .WithType("string")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .Build())
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("age")
                    .WithType("int")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .Build())
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("items")
                    .WithType("string[]")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .Build())

                // Add constructor using template's constructor body
                .AddConstructor(constructorBuilder => constructorBuilder
                    .WithClassName("PersonFromTemplate")
                    .WithParameters(("string", "name"), ("int", "age"))
                    .WithBodyFromTemplate(nameof(BasicTemplate)) // Use constructor from template
                    // Replace placeholders in the template with our field names
                    .ReplaceInBody("_NAME_PLACEHOLDER_", "name")
                    .ReplaceInBody("_AGE_PLACEHOLDER_", "age")
                    .ReplaceInBody("_ITEMS_PLACEHOLDER_", "items")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())

                // Add property using template property's accessors
                .AddProperty(propertyBuilder => propertyBuilder
                    .WithName("DisplayName")
                    .WithType("string")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithGetterFromTemplate(nameof(BasicTemplate.DisplayName)) // Use getter from template
                    .WithSetterFromTemplate(nameof(BasicTemplate.DisplayName)) // Use setter from template
                    // Replace placeholders in template with our field names
                    .ReplaceInGetterBody("_NAME_PLACEHOLDER_", "name")
                    .ReplaceInGetterBody("_AGE_PLACEHOLDER_", "age")
                    .ReplaceInSetterBody("_NAME_PLACEHOLDER_", "name")
                    .Build())

                // Add method using template method's body
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("AddItem")
                    .WithReturnType("bool")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithParameters(("string", "item"))
                    .WithBodyFromTemplate(nameof(BasicTemplate.AddItem)) // Use method body from template
                    .ReplaceInBody("_ITEMS_PLACEHOLDER_", "items") // Replace placeholder with our field name
                    .Build())

                // Add method with expression body from template
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("GetGreeting")
                    .WithReturnType("string")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithBodyFromTemplate(nameof(BasicTemplate.GetGreeting)) // Use method body from template
                    .ReplaceInBody("_NAME_PLACEHOLDER_", "name") // Replace placeholder with our field name
                    .Build())
                .SetDirectory("Assets/easycodegenunity/Samples/BasicExamples/Generated")
                .SetFileName("PersonFromTemplate.cs")
                .Generate()
                .Save();
        }
    }
}