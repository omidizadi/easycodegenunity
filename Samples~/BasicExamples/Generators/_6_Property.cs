using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// A simple example of generating a class with a property using EasyCodeGenerator.
    /// </summary>
    public class _6_Property : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddNamespace("easycodegenunity.Editor.Samples.BasicExamples.Generated")
                .AddClass(typeBuilder => typeBuilder
                    .WithName("SampleClassWithProperty")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                .AddProperty(propertyBuilder => propertyBuilder
                    .WithName("SampleProperty")
                    .WithType("int")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithoutSetter()
                    .Build())
                .SetDirectory("Assets/easycodegenunity/Samples/BasicExamples/Generated")
                .SetFileName("SampleProperty.cs")
                .Generate()
                .Save();
        }
    }
}