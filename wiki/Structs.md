# Generating Structs

Structs (structure types) are value types in C#. EasyCodeGen provides the `EasyStructBuilder` for generating struct
declarations, used via the `AddStruct` method on `EasyCodeBuilder`.

## Basic Struct Generation

To generate a struct, use `AddStruct` on an `EasyCodeBuilder` instance. This method takes a lambda expression where you
configure the struct using `EasyStructBuilder`.

Key methods on `EasyStructBuilder` (inherited from `EasyTypeBuilder`):

* `WithName(string name)`: Sets the name of the struct. (Required)
* `WithModifiers(params SyntaxKind[] modifiers)`: Sets access modifiers (e.g., `SyntaxKind.PublicKeyword`,
  `SyntaxKind.InternalKeyword`). You can also specify `SyntaxKind.ReadOnlyKeyword` for readonly structs.
* `WithInterfaces(params string[] interfaces)`: Specifies one or more interfaces that the struct implements.
* `Build()`: Finalizes the struct definition. This must be called at the end of the struct configuration.

### Example: Simple Public Struct

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Models")
    .AddStruct(sb => sb
        .WithName("Point")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddField(fb => fb.WithName("X").WithType("int").WithModifiers(SyntaxKind.PublicKeyword).Build()) // Add public field X
    .AddField(fb => fb.WithName("Y").WithType("int").WithModifiers(SyntaxKind.PublicKeyword).Build()) // Add public field Y
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Models
{
    public struct Point
    {
        public int X;
        public int Y;
    }
}
```

### Example: Readonly Struct

To create a readonly struct (C# 7.2+), include `SyntaxKind.ReadOnlyKeyword`.

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Models")
    .AddStruct(sb => sb
        .WithName("ImmutableVector")
        .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword) // Readonly struct
        .Build())
    .AddField(fb => fb
        .WithName("DeltaX")
        .WithType("double")
        .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword) // Readonly field
        .Build())
    .AddField(fb => fb.
        WithName("DeltaY")
        .WithType("double")
        .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword) // Readonly field
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Models
{
    public readonly struct ImmutableVector
    {
        public readonly double DeltaX;
        public readonly double DeltaY;
    }
}
```

## Implementing Interfaces

Structs can implement interfaces. Use `WithInterfaces`.

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_4_Fields.cs (adapted to show interface)
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Data")
    .AddStruct(structBuilder => structBuilder
        .WithName("Coordinate")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithInterfaces("IEquatable<Coordinate>") // Implementing an interface
        .Build())
    // ... add fields and methods for IEquatable<Coordinate>
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Data
{
    public struct Coordinate : IEquatable<Coordinate>
    {
        // Fields and methods for IEquatable<Coordinate> would be added here
    }
}
```

## Adding Members

Structs can have fields, properties, methods, and constructors. You add these using the respective builder methods on
`EasyCodeBuilder` after the `AddStruct` call.

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_4_Fields.cs (struct part)
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Data")
    .AddStruct(structBuilder => structBuilder
        .WithName("SampleFieldsStruct") // Renamed for clarity
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddField(fieldBuilder => fieldBuilder
        .WithName("Id")
        .WithType("int")
        .WithModifiers(SyntaxKind.PublicKeyword) // Struct fields often public or properties used
        .Build())
    .AddField(fieldBuilder => fieldBuilder
        .WithName("Value")
        .WithType("string")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Data
{
    public struct SampleFieldsStruct
    {
        public int Id;
        public string Value;
    }
}
```

### Constructors in Structs

Structs can have constructors. Prior to C# 10, if you define a constructor, you must explicitly initialize all instance
fields of the struct within that constructor.

`EasyConstructorBuilder` can be used with `AddConstructor`:

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Models")
    .AddStruct(sb => sb
        .WithName("ComplexNumber")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddField(fb => fb.WithName("Real").WithType("double").WithModifiers(SyntaxKind.PublicKeyword).Build())
    .AddField(fb => fb.WithName("Imaginary").WithType("double").WithModifiers(SyntaxKind.PublicKeyword).Build())
    .AddConstructor(cb => cb
        .WithClassName("ComplexNumber") // Name of the struct for the constructor
        .WithParameters(("double", "real"), ("double", "imaginary"))
        .WithBody(
            "this.Real = real;",
            "this.Imaginary = imaginary;"
        )
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .Generate()
    .Save();
```

this generates:

```csharp
namespace MyApplication.Models
{
    public struct ComplexNumber
    {
        public double Real;
        public double Imaginary;

        public ComplexNumber(double real, double imaginary)
        {
            this.Real = real;
            this.Imaginary = imaginary;
        }
    }
}
```

For more details on adding specific members, refer to their respective documentation pages:

* [Fields](Fields.md)
* [Properties](Properties.md)
* [Methods](Methods.md)

For adding comments and attributes to structs, see [Comments](Comments.md) and [Attributes](Attributes.md).

