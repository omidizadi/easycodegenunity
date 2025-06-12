# Generating Fields

Fields are variables declared directly in a class or struct. EasyCodeGen uses `EasyFieldBuilder` to generate field
declarations, typically via the `AddField` method on `EasyCodeBuilder`.

## Basic Field Generation

To add a field, use `AddField` on `EasyCodeBuilder`. This method takes a lambda expression for configuring the field
with `EasyFieldBuilder`.

Key methods on `EasyFieldBuilder`:

* `WithName(string name)`: Sets the name of the field. (Required)
* `WithType(string type)`: Sets the type of the field. (Required)
* `WithModifiers(params SyntaxKind[] modifiers)`: Sets access modifiers (e.g., `SyntaxKind.PublicKeyword`,
  `SyntaxKind.PrivateKeyword`) and other modifiers like `SyntaxKind.ConstKeyword`, `SyntaxKind.StaticKeyword`,
  `SyntaxKind.ReadOnlyKeyword`.
* `WithInitialValue<T>(T value)`: Sets an initial value for the field.
* `Build()`: Finalizes the field definition. (Required)

### Example: Simple Private Field

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_5_Constructor.cs (age field)
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Models")
    .AddClass(cb => cb
        .WithName("Person")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddField(fb => fb
        .WithName("age")
        .WithType("int")
        .WithModifiers(SyntaxKind.PrivateKeyword)
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Models
{
    public class Person
    {
        private int age;
    }
}
```

## Field Initialization

You can initialize a field with a value using `WithInitialValue`.

### Example: Field with Initial Value

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_4_Fields.cs (UNIQUE_KEY field)
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Config")
    .AddStruct(sb => sb
        .WithName("AppSettings")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddField(fieldBuilder => fieldBuilder
        .WithName("DEFAULT_TIMEOUT")
        .WithType("int")
        .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.ConstKeyword) // A public constant
        .WithInitialValue(5000) // Initialized to 5000
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Config
{
    public struct AppSettings
    {
        public const int DEFAULT_TIMEOUT = 5000;
    }
}
```

This generates a struct `AppSettings` with a public constant integer `DEFAULT_TIMEOUT` initialized to `5000`.

The `WithInitialValue` method supports various types:

* `string`: `SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s))`
* `bool`: `SyntaxFactory.LiteralExpression(b ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression)`
* `int`, `float`, `double`:
  `SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value))`
* `null`: `SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)`
* Other types are converted using `value.ToString()` and parsed as an expression.

## Field Modifiers

Various modifiers can be applied to fields:

* **Access Modifiers**: `SyntaxKind.PublicKeyword`, `SyntaxKind.PrivateKeyword`, `SyntaxKind.ProtectedKeyword`,
  `SyntaxKind.InternalKeyword`.
* **`const`**: `SyntaxKind.ConstKeyword`. Requires an initial value.
* **`static`**: `SyntaxKind.StaticKeyword`.
* **`readonly`**: `SyntaxKind.ReadOnlyKeyword`. Can be initialized at declaration or in a constructor.

### Example: `const` Field

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_4_Fields.cs (UNIQUE_KEY field)
.AddField(fieldBuilder => fieldBuilder
    .WithName("UNIQUE_KEY")
    .WithType("string")
    .WithInitialValue("XYZ123")
    .WithModifiers(SyntaxKind.PrivateKeyword, SyntaxKind.ConstKeyword) // private const string UNIQUE_KEY = "XYZ123";
    .Build())
```

This generates:

```csharp
namespace MyApplication.Config
{
    public struct AppSettings
    {
        private const string UNIQUE_KEY = "XYZ123";
    }
}
```

### Example: `static readonly` Field

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Core")
    .AddClass(cb => cb.WithName("Globals").WithModifiers(SyntaxKind.PublicKeyword).Build())
    .AddField(fb => fb
        .WithName("AppName")
        .WithType("string")
        .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword)
        .WithInitialValue("My Awesome App")
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Core
{
    public class Globals
    {
        public static readonly string AppName = "My Awesome App";
    }
}
```

## Array Fields

To declare an array field, simply use the array type syntax (e.g., `string[]`, `int[]`) in `WithType`.

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_4_Fields.cs (certificates field)
.AddField(fieldBuilder => fieldBuilder
    .WithName("certificates")
    .WithType("string[]") // Declaring a string array
    .WithModifiers(SyntaxKind.PrivateKeyword)
    .Build())
```

This generates:

```csharp
namespace MyApplication.Config
{
    public struct AppSettings
    {
        private string[] certificates;
    }
}
```

## Events

You can also declare fields that are event actions, typically used for Unity events or custom events.

```csharp
.AddField(fieldBuilder => fieldBuilder
    .WithName("OnDataChanged")
    .WithType("Action<int>") // Declaring an Action field
    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.EventKeyword) // Public event field
    .Build())
```

This generates:

```csharp
namespace MyApplication.Events
{
    public class DataNotifier
    {
        public event Action<int> OnDataChanged;
    }
}
```

Initialization of array fields can be done using `WithInitialValue` if the expression can be parsed, or more typically
within a constructor or method.

For adding comments and attributes to fields, see [Comments](Comments.md) and [Attributes](Attributes.md).

