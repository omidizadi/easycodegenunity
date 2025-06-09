using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// This sample demonstrates how to create a static class with a method using EasyCodeGenerator.
    /// </summary>
    public class _2_SampleStaticClass : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddUsingStatement("System") // For Console.WriteLine
                .AddNamespace("easycodegenunity.Editor.Samples.BasicExamples.Generated") // Namespace for the generated class
                .AddClass(typeBuilder => typeBuilder
                    .WithName("UtilityClass") // Name of the static class
                    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword) // Making the class public and static
                    .Build()) // Finalizing the type definition
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("PrintMessage")
                    .WithReturnType("void")
                    .WithBody("Console.WriteLine(\"Hello from UtilityClass!\");")
                    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                    .Build())
                .SetDirectory("Assets/easycodegenunity/Editor/Samples/BasicExamples/Generated")
                .SetFileName("UtilityClass.cs")
                .Generate()
                .Save();
        }
    }
}