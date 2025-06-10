using easycodegenunity.Editor.Core;
using easycodegenunity.Editor.Core.Builders;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// This example demonstrates how to add various types of comments to generated code.
    /// It shows single-line comments, multi-line comments, XML documentation comments,
    /// and how to generate parameter comments dynamically.
    /// </summary>
    public class _11_Comments : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddUsingStatement("System")
                .AddUsingStatement("System.Collections.Generic")
                .AddNamespace("easycodegenunity.Editor.Samples.BasicExamples.Generated")

                // Add class with XML documentation comment
                .AddClass(classBuilder => classBuilder
                    .WithName("CommentedClass")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    // Add XML documentation summary comment
                    .WithSummaryComment("A sample class demonstrating different comment styles in generated code.")
                    // Add multi-line comment
                    .WithMultiLineComment(@"This class is generated automatically.
It contains examples of different types of comments
that can be applied to types and members.
Last generated: " + System.DateTime.Now.ToString("yyyy-MM-dd"))
                    .Build())

                // Add field with single line comment
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("_name")
                    .WithType("string")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .WithSingleLineComment("The name of the person")
                    .Build())

                // Add property with summary documentation
                .AddProperty(propertyBuilder => propertyBuilder
                    .WithName("Name")
                    .WithType("string")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithSummaryComment("Gets or sets the person's name")
                    .WithGetterBody("return _name;")
                    .WithSetterBody("_name = value;")
                    .Build())

                // Add constructor with parameters and dynamically generated param comments
                .AddConstructor(constructorBuilder =>
                {
                    var parameters = new[]
                    {
                        ("string", "name", "The person's name"),
                        ("int", "age", "The person's age"),
                        ("bool", "isActive", "Whether the person is active")
                    };

                    var builder = constructorBuilder
                        .WithClassName("CommentedClass")
                        .WithModifiers(SyntaxKind.PublicKeyword)
                        .WithSummaryComment("Initializes a new instance of the CommentedClass class")
                        .WithBody("_name = name;");

                    // Add each parameter and its corresponding param comment
                    foreach (var param in parameters)
                    {
                        builder.WithParameter(param.Item1, param.Item2);
                        builder.WithParamComment(param.Item2, param.Item3);
                    }

                    return builder.Build();
                })

                // Add method with returns comment
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("GetFormattedName")
                    .WithReturnType("string")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithSingleLineComment("Formats the name in uppercase")
                    .WithBody("return _name.ToUpper();")
                    .Build())

                // Add method with template comment that replaces placeholders
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("ProcessData")
                    .WithReturnType("void")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithParameters(("List<string>", "items"))
                    .WithSummaryComment("Processes the {DATA_TYPE} data in the provided collection")
                    .ReplaceInComment("{DATA_TYPE}", "string")
                    .WithParamComment("items", "The collection of {ITEM_TYPE} to process")
                    .ReplaceParamCommentText("items", "{ITEM_TYPE}", "strings")
                    .WithBodyLine("// Implementation goes here")
                    .Build())
                .SetDirectory("Assets/Samples/BasicExamples/Generated")
                .SetFileName("CommentedClass.cs")
                .Generate()
                .Save();
        }
    }
}