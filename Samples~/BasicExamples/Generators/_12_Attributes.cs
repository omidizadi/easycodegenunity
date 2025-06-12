using System;
using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// This example demonstrates how to add attributes to generated code elements such as
    /// classes, methods, properties, and fields.
    /// </summary>
    public class _12_Attributes : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddUsingStatement("System")
                .AddUsingStatement("UnityEngine")
                .AddUsingStatement("System.Diagnostics")
                .AddNamespace(BasicExampleHelper.GetNamespace) // Defining the namespace for the generated code

                // Add class with multiple attributes
                .AddClass(classBuilder => classBuilder
                    .WithName("AttributeExampleClass")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    // Add Serializable attribute
                    .WithAttribute("Serializable")
                    // Add attribute with named parameters
                    .WithAttribute("Obsolete", "(\"This class will be removed in future versions\", false)")
                    .WithSummaryComment("A class demonstrating various attributes")
                    .WithBaseType("MonoBehaviour")
                    .Build())

                // Add fields with attributes
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("_id")
                    .WithType("int")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    // Add attribute with parameters
                    .WithAttribute("Range", "(1, 1000)")
                    .Build())
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("_initialized")
                    .WithType("bool")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .WithInitialValue(false)
                    .Build())

                // Add property with generated attribute
                .AddProperty(propertyBuilder => propertyBuilder
                    .WithName("Description")
                    .WithType("string")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    // Add attribute with placeholder
                    .WithAttribute("Tooltip", "(\"Enter a description here\")")
                    .WithAttribute("field: SerializeField")
                    .Build())

                // Add method with attributes
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("Initialize")
                    .WithReturnType("void")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    // Add conditional attribute
                    .WithAttribute("Conditional", "(\"DEBUG\")")
                    // Add obsolete attribute with message
                    .WithAttribute("Obsolete", "(\"Use InitializeAsync instead\", true)")
                    .WithBody(
                        "_initialized = true;",
                        "Console.WriteLine(\"Initialized\");"
                    )
                    .Build())
                .SetDirectory(BasicExampleHelper.GetDirectory) // Setting the directory where the generated file will be saved
                .SetFileName("AttributeExampleClass.cs")
                .Generate()
                .Save();
        }
    }
}