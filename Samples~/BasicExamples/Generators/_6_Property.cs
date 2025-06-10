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
                .AddNamespace(BasicExampleHelper.GetNamespace) // Defining the namespace for the generated code
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
                .SetDirectory(BasicExampleHelper.GetDirectory) // Setting the directory where the generated file will be saved
                .SetFileName("SampleProperty.cs")
                .Generate()
                .Save();
        }
    }
}