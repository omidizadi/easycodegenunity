using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// This sample shows how to create a class with a constructor using EasyCodeGenerator.
    /// </summary>
    public class _5_Constructor : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddUsingStatement("System")
                .AddNamespace(BasicExampleHelper.GetNamespace) // Defining the namespace for the generated code
                .AddClass(classBuilder => classBuilder
                    .WithName("SampleClassWithConstructor")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("age")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .WithType("int")
                    .Build())
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("name")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .WithType("string")
                    .Build())
                .AddConstructor(constructorBuilder => constructorBuilder // define the constructor
                    .WithClassName("SampleClassWithConstructor")
                    .WithBody(
                        @"Console.WriteLine(""Constructor called!"");", // each statement of the body should be a string
                        @"this.name = name;",
                        @"this.age = age;")
                    .WithParameters(("string", "name"), ("int", "age"))
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                .SetDirectory(BasicExampleHelper.GetDirectory) // Setting the directory where the generated file will be saved
                .SetFileName("SampleClassWithConstructor.cs")
                .Generate()
                .Save();
        }
    }
}