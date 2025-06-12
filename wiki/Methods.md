# Generating Methods

Methods are blocks of code that perform a specific task. EasyCodeGen's `EasyMethodBuilder` helps in generating method
declarations within classes, structs, or interfaces (for default implementations).

## Basic Method Generation

To add a method, you use the `AddMethod` method on an `EasyCodeBuilder` instance. This method takes a lambda expression
where you configure the method using `EasyMethodBuilder`.

Key methods on `EasyMethodBuilder`:

* `WithName(string name)`: Sets the name of the method. (Required)
* `WithReturnType(string type)`: Sets the return type of the method (e.g., "void", "int", "string"). (Required)
* `WithModifiers(params SyntaxKind[] modifiers)`: Sets access modifiers (e.g., `SyntaxKind.PublicKeyword`,
  `SyntaxKind.PrivateKeyword`) and other modifiers like `SyntaxKind.StaticKeyword`, `SyntaxKind.AsyncKeyword`,
  `SyntaxKind.VirtualKeyword`, `SyntaxKind.OverrideKeyword`.
* `WithBody(params string[] bodyStatements)`: Defines the method's body with one or more statements. Each string is a
  separate statement.
* `WithExpressionBody(string expression)`: Defines the method using an expression body (e.g., `"a + b"`). The `return`
  keyword and semicolon are handled automatically.
* `Build()`: Finalizes the method definition. (Required)

### Example: Simple Public Void Method

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_8_Methods.cs
new EasyCodeBuilder()
    .AddUsingStatement("System")
    .AddNamespace("MyApplication.Services")
    .AddClass(typeBuilder => typeBuilder
        .WithName("SampleService")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddMethod(methodBuilder => methodBuilder
        .WithName("LogMessage")
        .WithReturnType("void")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithBody("Console.WriteLine(\"This is a sample method\");")
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
using System;
namespace MyApplication.Services
{
    public class SampleService
    {
        public void LogMessage()
        {
            Console.WriteLine("This is a sample method");
        }
    }
}
```

## Method Parameters

You can add parameters to a method using `WithParameters` or `WithParameter`.

* `WithParameters(params (string type, string name)[] parameters)`: Adds multiple parameters at once.
* `WithParameter(string type, string name)`: Adds a single parameter.

### Example: Method with Parameters

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_9_AdvancedMethods.cs (Add method)
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Utils")
    .AddClass(typeBuilder => typeBuilder.WithName("Calculator").WithModifiers(SyntaxKind.PublicKeyword).Build())
    .AddMethod(methodBuilder => methodBuilder
        .WithName("Add")
        .WithReturnType("int")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithParameters(("int", "a"), ("int", "b")) // Two integer parameters
        .WithBody("return a + b;")
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
using System;
namespace MyApplication.Utils
{
    public class Calculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
```

## Expression-Bodied Methods

For methods with a single expression, you can use an expression body with `WithExpressionBody`.

* `WithExpressionBody(string expression)`: Defines the method using an expression body (e.g., `"a + b"`). The `return`
  keyword and semicolon are handled automatically.

### Example: Expression-Bodied Method

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Utils")
    .AddClass(typeBuilder => typeBuilder.WithName("MathHelper").WithModifiers(SyntaxKind.PublicKeyword).Build())
    .AddMethod(methodBuilder => methodBuilder
        .WithName("Multiply")
        .WithReturnType("int")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithParameters(("int", "x"), ("int", "y"))
        .WithExpressionBody("x * y") // Expression body
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
using System;
namespace MyApplication.Utils
{
    public class MathHelper
    {
        public int Multiply(int x, int y) => x * y;
    }
}
```

## Complex Method Bodies

For methods with multiple statements or complex logic, provide each statement as a separate string to `WithBody`.

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_9_AdvancedMethods.cs (ProcessData method)
.AddMethod(methodBuilder => methodBuilder
    .WithName("ProcessData")
    .WithReturnType("List<string>")
    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
    .WithParameters(("string[]", "data"))
    .WithBody(
          @"var result = new List<string>();",
          @"foreach (var item in data)
            {  
              if (string.IsNullOrEmpty(item))
                 continue;
             var processed = item.Trim().ToUpper();
             result.Add(processed);
          }",
          @"return result;"
    )
    .Build())
```

This generates:

```csharp
        public static List<string> ProcessData(string[] data)
        {
            var result = new List<string>();
            foreach (var item in data)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                var processed = item.Trim().ToUpper();
                result.Add(processed);
            }

            return result;
        } 
```       

## Using Templates for Method Bodies

You can source the body of a method from a template class using `WithBodyFromTemplate` and then modify it if needed with
`ReplaceInBody`.

* `WithBodyFromTemplate(string methodNameInTemplate)`: Copies the body (block or expression) from the specified method
  in the template class (set via `EasyCodeBuilder.WithTemplate<T>()`).
* `ReplaceInBody(string searchText, string replaceText)`: Replaces occurrences of `searchText` with `replaceText` in the
  method's body (works for both regular and expression bodies).

### Example: Method Body from Template

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_10_TemplateUsage.cs
// Assuming EasyCodeBuilder was initialized with .WithTemplate<BasicTemplate>()

// ... fields "name", "items" are defined in the target class ...

.AddMethod(methodBuilder => methodBuilder
    .WithName("AddItem")
    .WithReturnType("bool")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithParameters(("string", "item"))
    .WithBodyFromTemplate(nameof(BasicTemplate.AddItem)) // Use method body from BasicTemplate.AddItem
    .ReplaceInBody("_ITEMS_PLACEHOLDER_", "items") // Replace placeholder in the copied body
    .Build())

.AddMethod(methodBuilder => methodBuilder
    .WithName("GetGreeting")
    .WithReturnType("string")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithBodyFromTemplate(nameof(BasicTemplate.GetGreeting)) // Use expression body from BasicTemplate.GetGreeting
    .ReplaceInBody("_NAME_PLACEHOLDER_", "name")
    .Build())
```

## Other Method Features

* **Static Methods**: Include `SyntaxKind.StaticKeyword` in `WithModifiers`.
* **Async Methods**: Include `SyntaxKind.AsyncKeyword` in `WithModifiers`.
* **Virtual/Override**: Use `SyntaxKind.VirtualKeyword` or `SyntaxKind.OverrideKeyword`.

For adding comments (like XML documentation) and attributes to methods, refer to the [Comments](Comments.md)
and [Attributes](Attributes.md) documentation pages.

