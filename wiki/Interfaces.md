# Generating Interfaces

Interfaces define contracts in C#. EasyCodeGen allows you to generate interface declarations using the
`EasyInterfaceBuilder`, typically accessed via the `AddInterface` method on an `EasyCodeBuilder` instance.

## Basic Interface Generation

To generate an interface, use `AddInterface` on `EasyCodeBuilder`. This method accepts a lambda expression for
configuring the interface with `EasyInterfaceBuilder`.

Key methods on `EasyInterfaceBuilder` (inherited from `EasyTypeBuilder`):

* `WithName(string name)`: Sets the name of the interface. (Required)
* `WithModifiers(params SyntaxKind[] modifiers)`: Sets access modifiers (e.g., `SyntaxKind.PublicKeyword`). Typically,
  interfaces are public.
* `WithInterfaces(params string[] baseInterfaces)`: Specifies one or more interfaces that this interface extends (
  inherits from).
* `Build()`: Finalizes the interface definition. This must be called at the end of the interface configuration.

### Example: Simple Public Interface

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Contracts")
    .AddInterface(ib => ib
        .WithName("IUserService")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Contracts
{
    public interface IUserService
    {
    }
}
```

## Interface Inheritance

Interfaces can inherit from other interfaces. Use `WithInterfaces` (or `WithBaseType`) for this.

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Contracts")
    .AddInterface(ib => ib
        .WithName("IAdminService")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithInterfaces("IUserService") // IAdminService extends IUserService
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Contracts
{
    public interface IAdminService : IUserService
    {
    }
}
```

## Adding Members to Interfaces

Interfaces typically contain method signatures, properties, events, and indexers. You add these members using the
respective `AddMethod`, `AddProperty`, etc., methods on `EasyCodeBuilder` after the `AddInterface` call.

### Properties in Interfaces

Properties in interfaces are typically auto-properties (defining just `get;` and/or `set;`). Use `WithAutoProperty()`
from `EasyPropertyBuilder`.

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Contracts")
    .AddInterface(ib => ib
        .WithName("IEntity")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddProperty(pb => pb
        .WithName("Id")
        .WithType("int")
        // No modifiers needed for interface members usually (implicitly public)
        .WithAutoProperty() // Creates { get; set; } or { get; } if WithoutSetter() is used
        .Build())
    .AddProperty(pb => pb
        .WithName("Name")
        .WithType("string")
        .WithAutoProperty()
        .WithoutSetter() // Read-only property in interface: { get; }
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Contracts
{
    public interface IEntity
    {
        int Id { get; set; }
        string Name { get; }
    }
}
```

### Methods in Interfaces

Methods in interfaces are traditionally just signatures ending with a semicolon (e.g., `void DoSomething();`).

The current `EasyMethodBuilder` is primarily designed for methods with bodies or expression bodies. Generating a method
signature like `void DoSomething();` directly might require workarounds or future enhancements to `EasyMethodBuilder` (
e.g., a `WithSignatureOnly()` option).

* **C# 8+ Default Implementations**: If you are targeting C# 8 or later, you can provide default implementations for
  methods in interfaces. In this case, `EasyMethodBuilder` can be used as usual with `WithBody` or `WithExpressionBody`.

  ```csharp
  // Example: Interface method with default C# 8+ implementation
  new EasyCodeBuilder() 
      .AddUsingStatement("System")
      .AddNamespace("MyApplication.Contracts")
      .AddInterface(ib => ib
          .WithName("ILogger")
          .WithModifiers(SyntaxKind.PublicKeyword)
          .Build())
      .AddMethod(mb => mb
          .WithName("LogMessage")
          .WithReturnType("void")
          .WithParameters(("string", "message"))
          .WithBody("Console.WriteLine($\"Default log: {message}\");") // Default implementation
          .Build())
      .Generate()
      .Save();
  ```

This generates:

```csharp
using System;

namespace MyApplication.Contracts
{
    public interface ILogger
    {
        void LogMessage(string message)
        {
            Console.WriteLine($"Default log: {message}");
        }
    }
}
```

* **Abstract Method Signatures (Limitation)**: For traditional abstract method signatures (e.g., `void Process();`),
  `EasyMethodBuilder` currently expects a body. A possible workaround could be to provide an empty body `WithBody("")`
  which would generate `void Process() {}`. This fulfills the contract but isn't a clean abstract signature. True
  abstract signature support may require builder enhancements.

For adding comments and attributes to interfaces and their members, refer to the [Comments](Comments.md)
and [Attributes](Attributes.md) documentation pages.

