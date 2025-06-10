using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// This sample demonstrates how to create a static class with a method using EasyCodeGenerator.
    /// </summary>
    public class _2_StaticClass : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddUsingStatement("System") // For Console.WriteLine
                .AddNamespace(BasicExampleHelper.GetNamespace) // Defining the namespace for the generated code
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
                .SetDirectory(BasicExampleHelper.GetDirectory) // Setting the directory where the generated file will be saved
                .SetFileName("UtilityClass.cs")
                .Generate()
                .Save();
        }
    }
}