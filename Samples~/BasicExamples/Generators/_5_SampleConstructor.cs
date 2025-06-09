using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    public class _5_SampleConstructor : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddUsingStatement("System")
                .AddNamespace("easycodegenunity.Editor.Samples.BasicExamples.Generated")
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
                .AddConstructor(constructorBuilder => constructorBuilder
                    .WithClassName("SampleClassWithConstructor")
                    .WithBody(
                        @"Console.WriteLine(""Constructor called!"");",
                        @"this.name = name;",
                        @"this.age = age;")
                    .WithParameters(("string", "name"), ("int", "age"))
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                .SetDirectory("Assets/easycodegenunity/Editor/Samples/BasicExamples/Generated")
                .SetFileName("SampleClassWithConstructor.cs")
                .Generate()
                .Save();
        }
    }
}