using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// In this example, we demonstrate how to create a struct with fields using the EasyCodeGenerator.
    /// </summary>
    public class _4_Fields : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddNamespace("easycodegenunity.Editor.Samples.BasicExamples.Generated")
                .AddStruct(structBuilder => structBuilder // Define a struct named SampleFields
                    .WithName("SampleFields")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                .AddField(fieldBuilder => fieldBuilder // Add a field named UNIQUE_KEY of type string, with an initial value and marked as private const
                    .WithName("UNIQUE_KEY")
                    .WithType("string")
                    .WithInitialValue("UNIQUE_KEY")
                    .WithModifiers(SyntaxKind.PrivateKeyword, SyntaxKind.ConstKeyword)
                    .Build())
                .AddField(fieldBuilder => fieldBuilder // Add a field named name of type string, marked as private
                    .WithName("age")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .WithType("int")
                    .Build())
                .AddField(fieldBuilder => fieldBuilder // Add a field with type of string[] and marked as private
                    .WithName("certificates")
                    .WithType("string[]")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .Build())
                .SetDirectory("Assets/Samples/BasicExamples/Generated")
                .SetFileName("SampleFields.cs")
                .Generate()
                .Save();
        }
    }
}