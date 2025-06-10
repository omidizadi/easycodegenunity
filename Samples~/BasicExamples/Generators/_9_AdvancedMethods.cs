using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// In this example, we demonstrate how to create various types of methods in a class using the EasyCodeGenerator.
    /// </summary>
    public class _9_AdvancedMethods : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddNamespace("easycodegenunity.Editor.Samples.BasicExamples.Generated")
                .AddUsingStatement("System")
                .AddUsingStatement("System.Collections.Generic")
                .AddClass(typeBuilder => typeBuilder
                    .WithName("AdvancedMethodDemo")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                
                // Example 1: Simple void method with no parameters
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("SimpleVoidMethod")
                    .WithReturnType("void")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithBody("Console.WriteLine(\"This is a simple void method\");")
                    .Build())
                
                // Example 2: Method with parameters and return value
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("Add")
                    .WithReturnType("int")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithParameters(("int", "a"), ("int", "b"))
                    .WithBody("return a + b;")
                    .Build())
                
                // Example 3: Method with complex body using multiple statements
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("ProcessData")
                    .WithReturnType("List<string>")
                    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                    .WithParameters(("string[]", "data"))
                    .WithBody(@"
                        var result = new List<string>();
                        
                        foreach (var item in data)
                        {
                            if (string.IsNullOrEmpty(item))
                                continue;
                                
                            var processed = item.Trim().ToUpper();
                            result.Add(processed);
                        }
                        
                        return result;")
                    .Build())
                
                // Example 4: Private helper method
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("ValidateInput")
                    .WithReturnType("bool")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .WithParameters(("string", "input"))
                    .WithBody(@"
                        if (string.IsNullOrEmpty(input))
                        {
                            Console.WriteLine(""Input is empty"");
                            return false;
                        }
                        
                        if (input.Length < 3)
                        {
                            Console.WriteLine(""Input is too short"");
                            return false;
                        }
                        
                        return true;")
                    .Build())
                
                // Example 5: Method that calls other methods
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("ProcessInput")
                    .WithReturnType("string")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithParameters(("string", "input"))
                    .WithBody(@"
                        if (!ValidateInput(input))
                            return string.Empty;
                        
                        var data = input.Split(',');
                        var processed = ProcessData(data);
                        
                        return string.Join(""-"", processed);")
                    .Build())
                
                // Example 6: Method with optional parameters and default values
                .AddMethod(methodBuilder => methodBuilder
                    .WithName("FormatText")
                    .WithReturnType("string")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithParameters(("string", "text"), ("bool", "uppercase = true"), ("bool", "trim = true"))
                    .WithBody(@"
                        if (string.IsNullOrEmpty(text))
                            return string.Empty;
                            
                        var result = text;
                        
                        if (trim)
                            result = result.Trim();
                            
                        if (uppercase)
                            result = result.ToUpper();
                            
                        return result;")
                    .Build())
                
                .SetDirectory("Assets/easycodegenunity/Samples/BasicExamples/Generated")
                .SetFileName("AdvancedMethodsSample.cs")
                .Generate()
                .CleanUpAndSave();
        }
    }
}
