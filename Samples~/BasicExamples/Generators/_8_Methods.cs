// filepath: /Users/omidrezaizadi/EasyCodeGen/Assets/easycodegenunity/Samples/BasicExamples/Generators/Methods.cs
using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    public class _8_Methods : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddUsingStatement("System")
                .AddNamespace("easycodegenunity.Editor.Samples.BasicExamples.Generated")
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
                .SetDirectory("Assets/easycodegenunity/Samples/BasicExamples/Generated")
                .SetFileName("SampleMethod.cs")
                .Generate()
                .Save();
        }
    }
}