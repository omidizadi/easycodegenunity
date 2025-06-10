using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// In this sample, we create a simple Unity MonoBehaviour script that logs "Hello, World!" to the console.
    /// You will learn how to generate a simple Unity script using the EasyCodeBuilder.
    /// </summary>
    public class _1_HelloWorld : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddUsingStatement("UnityEngine") // Importing UnityEngine namespace to use MonoBehaviour and Debug classes
                .AddNamespace("easycodegenunity.Editor.Samples.BasicExamples.Generated") // Defining the namespace for the generated code
                .AddClass(typeBuilder => typeBuilder
                    .WithName("HelloWorldSample") // Name of the class
                    .WithBaseType("MonoBehaviour") // Inheriting from MonoBehaviour to make it a Unity script
                    .WithModifiers(SyntaxKind.PublicKeyword) // Making the class public
                    .Build()) // you should call Build() to finalize the type definition
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("Start") // Name of the method
                    .WithReturnType("void") // Return type of the method
                    .WithBody("Debug.Log(\"Hello, World!\");") // Body of the method that logs "Hello, World!" to the console
                    .WithModifiers(SyntaxKind.PrivateKeyword) // Making the method private, as it is a Unity lifecycle method
                    .Build()) // Finalizing the method definition
                .SetDirectory("Assets/easycodegenunity/Editor/Samples/BasicExamples/Generated") // Setting the directory where the generated file will be saved
                .SetFileName("HelloWorldSample.cs") // Setting the name of the generated file
                .Generate() // Generating the code based on the defined structure
                .Save();
        }
    }
}