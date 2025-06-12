# Generating Enums

Enums (enumerations) are a special kind of value type that defines a set of named constants. EasyCodeGen uses `EasyEnumBuilder` to create enum declarations, typically via the `AddEnum` method on `EasyCodeBuilder`.

## Basic Enum Generation

To generate an enum, use `AddEnum` on an `EasyCodeBuilder` instance. This method takes a lambda expression for configuring the enum with `EasyEnumBuilder`.

Key methods on `EasyEnumBuilder`:

*   `WithName(string name)`: Sets the name of the enum. (Required, via `EasyTypeBuilder`)
*   `WithBaseType(string baseType)`: Sets the underlying type of the enum (e.g., "byte", "int"). Defaults to `int` if not specified.
*   `WithModifiers(params SyntaxKind[] modifiers)`: Sets access modifiers (e.g., `SyntaxKind.PublicKeyword`). (Via `EasyTypeBuilder`)
*   `AddMember(string name)`: Adds an enum member with an automatically assigned underlying value (starts from 0).
*   `AddMember(string name, int value)`: Adds an enum member with an explicitly assigned integer value.
*   `Build()`: Finalizes the enum definition. (Required, via `EasyTypeBuilder`)

### Example: Simple Enum

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Core")
    .AddEnum(eb => eb
        .WithName("OperationStatus")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .AddMember("Pending")    // Value = 0
        .AddMember("InProgress") // Value = 1
        .AddMember("Completed")  // Value = 2
        .AddMember("Failed")     // Value = 3
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Core
{
    public enum OperationStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }
}
```

## Enum Members with Explicit Values

You can assign specific integer values to enum members.

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Core")
    .AddEnum(eb => eb
        .WithName("ErrorCode")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .AddMember("None", 0)
        .AddMember("NotFound", 404)
        .AddMember("Unauthorized", 401)
        .AddMember("ServerError", 500)
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Core
{
    public enum ErrorCode
    {
        None = 0,
        NotFound = 404,
        Unauthorized = 401,
        ServerError = 500
    }
}
```

## Enum with Base Type
Enums can have a specific underlying type, such as `byte`, `int`, or `long`. By default, the underlying type is `int`.

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Core")
    .AddEnum(eb => eb
        .WithName("ByteFlags")
        .WithBaseType("byte") // Underlying type is byte
        .WithModifiers(SyntaxKind.PublicKeyword)
        .AddMember("None", 0)
        .AddMember("OptionA", 1)
        .AddMember("OptionB", 2)
        .AddMember("OptionC", 4)
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Core
{
    public enum ByteFlags : byte
    {
        None = 0,
        OptionA = 1,
        OptionB = 2,
        OptionC = 4
    }
}
```

## Adding Comments and Attributes

Comments and attributes can be added to enums and their members.

*   **Enum Comments/Attributes**: Use `WithSummaryComment`, `WithAttribute`, etc., on the `EasyEnumBuilder` instance.
*   **Enum Member Comments/Attributes**: `EasyEnumBuilder` does not currently have a direct method to add attributes or XML comments to individual enum *members*. The `SyntaxFactory.EnumMemberDeclaration` can take attribute lists, but `EasyEnumBuilder.AddMember` doesn't expose this. This would be an area for potential enhancement.

### Example: Enum with Comments and Attributes (Enum-Level)

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Core")
    .AddEnum(eb => eb
        .WithName("TaskPriority")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithSummaryComment("Represents the priority levels for tasks.")
        .WithAttribute("System.Flags") // Example for a flags enum
        .AddMember("Low", 1)
        .AddMember("Medium", 2)
        .AddMember("High", 4)
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Core
{
    /// <summary>
    /// Represents the priority levels for tasks.
    /// </summary>
    [System.Flags]
    public enum TaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 4
    }
}
```

For general information on comments and attributes, see [Comments](Comments.md) and [Attributes](Attributes.md).


