# Generating Classes

Classes are fundamental building blocks in C#. EasyCodeGen provides the `EasyClassBuilder` to facilitate the generation
of class declarations. You typically use it within the `EasyCodeBuilder`.

## Basic Class Generation

To generate a class, you use the `AddClass` method on an `EasyCodeBuilder` instance. This method takes a lambda
expression where you configure the class using `EasyClassBuilder`.

Key methods on `EasyClassBuilder` (via `EasyTypeBuilder`):

* `WithName(string name)`: Sets the name of the class. (Required)
* `WithModifiers(params SyntaxKind[] modifiers)`: Sets the access modifiers (e.g., `SyntaxKind.PublicKeyword`,
  `SyntaxKind.InternalKeyword`) and other modifiers like `SyntaxKind.StaticKeyword`, `SyntaxKind.AbstractKeyword`,
  `SyntaxKind.SealedKeyword`.
* `WithBaseType(string type)`: Sets the base class from which this class inherits.
* `WithInterfaces(params string[] interfaces)`: Specifies one or more interfaces that the class implements.
* `Build()`: Finalizes the class definition. This must be called at the end of the class configuration.

### Example: Simple Public Class

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Core")
    .AddClass(cb => cb
        .WithName("MyService")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Core
{
    public class MyService
    {
    }
}
```

### Example: Static Class

To create a static class, include `SyntaxKind.StaticKeyword` in the modifiers.

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_2_StaticClass.cs
new EasyCodeBuilder()
    .AddUsingStatement("System")
    .AddNamespace("MyApplication.Utils")
    .AddClass(typeBuilder => typeBuilder
        .WithName("UtilityClass")
        .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword) // Making the class public and static
        .Build())
    .AddMethod(methodBuilder => methodBuilder // Example of adding a static method
        .WithName("PrintMessage")
        .WithReturnType("void")
        .WithBody("Console.WriteLine(\"Hello from UtilityClass!\");")
        .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
        .Build())
    .SetDirectory("GeneratedCode")
    .SetFileName("UtilityClass.cs")
    .Generate()
    .Save();
```

This generates:

```csharp
using System;
namespace MyApplication.Utils
{
    public static class UtilityClass
    {
        public static void PrintMessage()
        {
            Console.WriteLine("Hello from UtilityClass!");
        }
    }
}
```

## Inheritance and Interfaces

### Inheriting from a Base Class

Use `WithBaseType("BaseClassName")` to specify a base class.

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_1_HelloWorld.cs (adapted)
new EasyCodeBuilder()
    .AddUsingStatement("UnityEngine")
    .AddNamespace("MyGame.Scripts")
    .AddClass(typeBuilder => typeBuilder
        .WithName("PlayerController")
        .WithBaseType("MonoBehaviour") // Inheriting from MonoBehaviour
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .SetDirectory("GeneratedCode")
    .SetFileName("PlayerController.cs")
    .Generate()
    .Save();
```

This generates:

```csharp
using UnityEngine;
namespace MyGame.Scripts
{
    public class PlayerController : MonoBehaviour
    {
    }
}
```

### Implementing Interfaces

Use `WithInterfaces("IInterfaceA", "IInterfaceB")` to implement one or more interfaces.

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_3_Interfaces.cs (adapted)
new EasyCodeBuilder()
    .AddUsingStatement("UnityEngine")
    .AddNamespace("MyGame.Entities")
    .AddClass(typeBuilder => typeBuilder
        .WithName("InteractiveObject")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithBaseType("MonoBehaviour")
        .WithInterfaces("IInteractable", "IDamageable") // Implementing interfaces
        .Build())
    // ... (methods implementing interface members)
    .SetDirectory("GeneratedCode")
    .SetFileName("InteractiveObject.cs")
    .Generate()
    .Save();
```

This generates:

```csharp
using UnityEngine;
namespace MyGame.Entities
{
    public class InteractiveObject : MonoBehaviour, IInteractable, IDamageable
    {
        // Methods implementing IInteractable and IDamageable would go here
    }
}
```

## Adding Members

You can add methods, properties, fields, and constructors to your class using other builder methods on `EasyCodeBuilder`
after defining the class. These are typically chained after the `AddClass` call.

```csharp
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Models")
    .AddClass(cb => cb
        .WithName("User")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddField(fb => fb // Added after AddClass
        .WithName("_name")
        .WithType("string")
        .WithModifiers(SyntaxKind.PrivateKeyword)
        .Build())
    .AddProperty(pb => pb // Added after AddClass
        .WithName("Name")
        .WithType("string")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithExpressionBodiedGetter("_name")
        .WithExpressionBodiedSetter("_name = value")
        .Build())
    .Generate()
    .Save();
```

For more details on adding specific members, refer to their respective documentation pages:

* [Methods](Methods.md)
* [Properties](Properties.md)
* [Fields](Fields.md)
* [Constructors](Constructors.md)

## Using Templates

You can use `WithTemplate<T>()` on `EasyCodeBuilder` to leverage pre-defined structures or members from a template
class. See `_10_TemplateUsage.cs` for examples. Class-specific template functionalities are often handled by member
builders (e.g., method body from template).

For adding comments and attributes to classes, see [Comments](Comments.md) and [Attributes](Attributes.md).

