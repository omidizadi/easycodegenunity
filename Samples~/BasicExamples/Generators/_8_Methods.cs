// filepath: /Users/omidrezaizadi/EasyCodeGen/Assets/easycodegenunity/Samples/BasicExamples/Generators/Methods.cs
using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// A simple example of generating a class with a method using EasyCodeGenUnity.
    /// </summary>
    public class _8_Methods : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddUsingStatement("System")
                .AddNamespace(BasicExampleHelper.GetNamespace) // Defining the namespace for the generated code
                .AddClass(typeBuilder => typeBuilder
                    .WithName("SampleClassWithMethod")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("SampleMethod")
                    .WithReturnType("void")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithBody("Console.WriteLine(\"This is a sample method\");")
                    .Build())
                .SetDirectory(BasicExampleHelper.GetDirectory) // Setting the directory where the generated file will be saved
                .SetFileName("SampleMethod.cs")
                .Generate()
                .Save();
        }
    }
}