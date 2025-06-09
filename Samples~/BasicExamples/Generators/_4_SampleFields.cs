using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    public class _4_SampleFields : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddNamespace("easycodegenunity.Editor.Samples.BasicExamples.Generated")
                .AddStruct(structBuilder => structBuilder
                    .WithName("SampleFields")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("UNIQUE_KEY")
                    .WithType("string")
                    .WithInitialValue("UNIQUE_KEY")
                    .WithModifiers(SyntaxKind.PrivateKeyword, SyntaxKind.ConstKeyword)
                    .Build())
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("age")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .WithType("int")
                    .Build())
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("certificates")
                    .WithType("string[]")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .Build())
                .SetDirectory("Assets/easycodegenunity/Editor/Samples/BasicExamples/Generated")
                .SetFileName("SampleFields.cs")
                .Generate()
                .Save();
        }
    }
}