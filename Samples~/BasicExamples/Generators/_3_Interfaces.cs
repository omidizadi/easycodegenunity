using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// This sample demonstrates how to create a class that inherits from MonoBehaviour and implements multiple interfaces using EasyCodeGenerator.
    /// </summary>
    public class _3_Interfaces : IEasyCodeGenerator
    {
        public void Execute()
        {
            // generate a class that inherits from MonoBehaviour and implements SampleInterface
            new EasyCodeBuilder()
                .AddUsingStatement("UnityEngine") // For MonoBehaviour
                .AddNamespace(BasicExampleHelper.GetNamespace) // Defining the namespace for the generated code
                .AddClass(typeBuilder => typeBuilder
                    .WithName("SampleClass") // Name of the class
                    .WithModifiers(SyntaxKind.PublicKeyword) // Making the class public
                    .WithBaseType("MonoBehaviour") // Inheriting from MonoBehaviour
                    .WithInterfaces("IInterfaceA", "IInterfaceB") // Implementing multiple interfaces
                    .Build()) // Finalizing the type definition
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("DoSomethingA")
                    .WithReturnType("void")
                    .WithBody(@"Debug.Log(""DoSomethingA!"");")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("DoSomethingB")
                    .WithReturnType("void")
                    .WithBody(@"Debug.Log(""DoSomethingB!"");")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                .SetDirectory(BasicExampleHelper.GetDirectory) // Setting the directory where the generated file will be saved
                .SetFileName("SampleInterfaces.cs")
                .Generate()
                .Save();
        }
    }
}