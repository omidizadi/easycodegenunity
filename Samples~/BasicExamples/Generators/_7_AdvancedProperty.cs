using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generators
{
    /// <summary>
    /// Some advanced property examples demonstrating various property types and behaviors.
    /// </summary>
    public class _7_AdvancedProperty : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                .AddNamespace(BasicExampleHelper.GetNamespace) // Defining the namespace for the generated code
                .AddClass(typeBuilder => typeBuilder
                    .WithName("AdvancedPropertyDemo")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .Build())
                
                // Example 1: Auto-property (simple public property)
                .AddProperty(propertyBuilder => propertyBuilder
                    .WithName("SimpleProperty")
                    .WithType("int")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithAutoProperty()
                    .Build())
                
                // Example 2: Property with a private setter
                .AddProperty(propertyBuilder => propertyBuilder
                    .WithName("ReadMostlyProperty")
                    .WithType("string")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithSetterModifiers(SyntaxKind.PrivateKeyword)
                    .WithAutoProperty()
                    .Build())
                
                // Example 3: Property with a backing field
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("_backingValue")
                    .WithType("double")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .Build())
                .AddProperty(propertyBuilder => propertyBuilder
                    .WithName("BackedProperty")
                    .WithType("double")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithBackingField("_backingValue")
                    .Build())
                
                // Example 4: Property with custom getter/setter using expression bodies
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("_internalCounter")
                    .WithType("int")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .Build())
                .AddProperty(propertyBuilder => propertyBuilder
                    .WithName("Counter")
                    .WithType("int")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithExpressionBodiedGetter("_internalCounter")
                    .WithExpressionBodiedSetter("_internalCounter = value < 0 ? 0 : value")
                    .Build())
                
                // Example 5: Read-only property with complex getter using multiple statements
                .AddProperty(propertyBuilder => propertyBuilder
                    .WithName("IsValid")
                    .WithType("bool")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithoutSetter()
                    .WithGetterBody(
                        @"if (SimpleProperty < 0)",
                        @"    return false;",
                        @"if (string.IsNullOrEmpty(ReadMostlyProperty))",
                        @"    return false;",
                        @"return true;"
                    )
                    .Build())
                
                // Example 6: Property with multiple setter statements
                .AddField(fieldBuilder => fieldBuilder
                    .WithName("_validatedValue")
                    .WithType("int")
                    .WithModifiers(SyntaxKind.PrivateKeyword)
                    .Build())
                .AddProperty(propertyBuilder => propertyBuilder
                    .WithName("ValidatedValue")
                    .WithType("int")
                    .WithModifiers(SyntaxKind.PublicKeyword)
                    .WithExpressionBodiedGetter("_validatedValue")
                    .WithSetterBody(
                        @"if (value < 0)",
                        @"{",
                        @"    _validatedValue = 0;",
                        @"    return;",
                        @"}",
                        @"if (value > 100)",
                        @"{",
                        @"    _validatedValue = 100;",
                        @"    return;",
                        @"}",
                        @"_validatedValue = value;"
                    )
                    .Build())
                
                .SetDirectory(BasicExampleHelper.GetDirectory) // Setting the directory where the generated file will be saved
                .SetFileName("AdvancedPropertySample.cs")
                .Generate()
                .Save();
        }
    }
}
