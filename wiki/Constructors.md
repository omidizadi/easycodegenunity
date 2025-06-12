# Generating Constructors

Constructors are special methods used to initialize objects of a class or struct. EasyCodeGen's `EasyConstructorBuilder`
facilitates the generation of constructor declarations, typically used with `AddConstructor` on an `EasyCodeBuilder`
instance.

## Basic Constructor Generation

To add a constructor, use the `AddConstructor` method on `EasyCodeBuilder`. This method takes a lambda expression where
you configure the constructor using `EasyConstructorBuilder`.

Key methods on `EasyConstructorBuilder`:

* `WithClassName(string name)`: Sets the name of the class or struct for which the constructor is being created. This
  name is used for the constructor declaration (e.g., `public MyClass(...)`). (Required)
* `WithModifiers(params SyntaxKind[] modifiers)`: Sets access modifiers (e.g., `SyntaxKind.PublicKeyword`,
  `SyntaxKind.PrivateKeyword`, `SyntaxKind.ProtectedKeyword`) and `SyntaxKind.StaticKeyword` for static constructors.
* `WithBody(params string[] bodyStatements)`: Defines the constructor's body with one or more statements. Each string is
  a separate statement.
* `Build()`: Finalizes the constructor definition. (Required, via `EasyBasicBuilder`)

### Example: Simple Public Constructor

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_5_Constructor.cs
new EasyCodeBuilder()
    .AddUsingStatement("System")
    .AddNamespace("MyApplication.Models")
    .AddClass(classBuilder => classBuilder
        .WithName("SampleClassWithConstructor")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddField(fieldBuilder => fieldBuilder.WithName("name").WithType("string").WithModifiers(SyntaxKind.PrivateKeyword).Build())
    .AddField(fieldBuilder => fieldBuilder.WithName("age").WithType("int").WithModifiers(SyntaxKind.PrivateKeyword).Build())
    .AddConstructor(constructorBuilder => constructorBuilder
        .WithClassName("SampleClassWithConstructor") // Name of the class
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithParameters(("string", "name"), ("int", "age"))
        .WithBody(
            "Console.WriteLine(\"Constructor called!\");",
            "this.name = name;",
            "this.age = age;"
        )
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
using System;
namespace MyApplication.Models
{
    public class SampleClassWithConstructor
    {
        private string name;
        private int age;

        public SampleClassWithConstructor(string name, int age)
        {
            Console.WriteLine("Constructor called!");
            this.name = name;
            this.age = age;
        }
    }
}
```

## Constructor Parameters

You can add parameters to a constructor using `WithParameters` or `WithParameter`.

* `WithParameters(params (string type, string name)[] parameters)`: Adds multiple parameters at once.
* `WithParameter(string type, string name)`: Adds a single parameter.

(See the example above for `WithParameters` usage.)

## Expression-Bodied Constructors

For constructors with a single expression (less common for constructors but possible, especially with C# 7.0+ features
like `=> throw`), you can use `WithExpressionBody`.

* `WithExpressionBody(string expression)`: Defines the constructor using an expression body.

### Example: Expression-Bodied Constructor (Illustrative)

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Models")
    .AddClass(cb => cb.WithName("SimpleRecord").WithModifiers(SyntaxKind.PublicKeyword).Build())
    .AddField(fb => fb.WithName("Id").WithType("int").WithModifiers(SyntaxKind.PublicKeyword).Build()) // Assuming public field for simplicity
    .AddConstructor(con => con
        .WithClassName("SimpleRecord")
        .WithParameters(("int", "id"))
        .WithExpressionBody("this.Id = id") // Example: this.Id = id;
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Models
{
    public class SimpleRecord
    {
        public int Id;

        public SimpleRecord(int id) => this.Id = id;
    }
}
```

## Using Templates for Constructor Bodies

You can source the body of a constructor from a constructor in a template class using `WithBodyFromTemplate`. Parameters
are also sourced from the template constructor.

* `WithBodyFromTemplate(string constructorClassNameInTemplate = null)`: Copies the body and parameters from the
  constructor of `constructorClassNameInTemplate` in the template class (set via `EasyCodeBuilder.WithTemplate<T>()`).
  If `constructorClassNameInTemplate` is null or omitted, it uses the class name set by `WithClassName`.
* `ReplaceInBody(string searchText, string replaceText)`: Replaces occurrences of `searchText` with `replaceText` in the
  constructor's body (works for both regular and expression bodies).

### Example: Constructor Body from Template

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_10_TemplateUsage.cs
// Assuming EasyCodeBuilder was initialized with .WithTemplate<BasicTemplate>()
// and fields "name", "age", "items" are defined in the target class.

.AddConstructor(constructorBuilder => constructorBuilder
    .WithClassName("PersonFromTemplate") // Target class name
    // .WithParameters(("string", "name"), ("int", "age")) // Parameters will be taken from template
    .WithBodyFromTemplate(nameof(BasicTemplate)) // Use constructor from BasicTemplate
    .ReplaceInBody("_NAME_PLACEHOLDER_", "name")
    .ReplaceInBody("_AGE_PLACEHOLDER_", "age")
    .ReplaceInBody("_ITEMS_PLACEHOLDER_", "items")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .Build())
```

## Static Constructors

To create a static constructor, include `SyntaxKind.StaticKeyword` in `WithModifiers`. Static constructors do not have
parameters and cannot have access modifiers (they are implicitly public).

```csharp
new EasyCodeBuilder()
    .AddUsingStatement("System")
    .AddNamespace("MyApplication.Core")
    .AddClass(cb => cb.WithName("ConfigManager").WithModifiers(SyntaxKind.PublicKeyword).Build())
    .AddField(fb => fb.WithName("DefaultSetting").WithType("string").WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword).Build())
    .AddConstructor(con => con
        .WithClassName("ConfigManager")
        .WithModifiers(SyntaxKind.StaticKeyword) // Static constructor
        .WithBody(@"DefaultSetting = ""InitialValue"";", @"Console.WriteLine(""Static constructor executed."");")
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
using System;
namespace MyApplication.Core
{
    public class ConfigManager
    {
        public static string DefaultSetting;

        static ConfigManager()
        {
            DefaultSetting = "InitialValue";
            Console.WriteLine("Static constructor executed.");
        }
    }
}
```

For adding comments (like XML documentation) and attributes to constructors, refer to the [Comments](Comments.md)
and [Attributes](Attributes.md) documentation pages.

