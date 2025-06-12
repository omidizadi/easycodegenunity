# Generating Attributes

Attributes provide a way to add metadata to your code. They are declarative tags used to convey information to the runtime, compiler, or other tools about the behavior of code elements like assemblies, classes, methods, properties, etc.

EasyCodeGen allows adding attributes to various generated code elements using the `WithAttribute` method available on most builders (e.g., `EasyTypeBuilder`, `EasyMethodBuilder`, `EasyPropertyBuilder`, `EasyFieldBuilder`). These builders inherit this capability from `EasyBasicBuilder`.

## Adding Attributes

The `WithAttribute` method takes the attribute name and optionally its parameters.

*   `WithAttribute(string attributeName)`: Adds an attribute without parameters (e.g., `[Serializable]`)
*   `WithAttribute(string attributeName, string parameters)`: Adds an attribute with parameters. The parameters string should be enclosed in parentheses, e.g., `("Description for the tooltip")` or `(1, 100)`.

### Example: Attribute on a Class

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_12_Attributes.cs
new EasyCodeBuilder()
    .AddUsingStatement("System") // For Serializable
    .AddNamespace("MyApplication.Models")
    .AddClass(classBuilder => classBuilder
        .WithName("DataObject")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithAttribute("Serializable") // Adds [Serializable]
        .WithAttribute("Obsolete", "(\"This class is outdated, use NewDataObject instead.\", false)") // Adds [Obsolete(...)]
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
using System;

namespace MyApplication.Models
{
    [Serializable]
    [Obsolete("This class is outdated, use NewDataObject instead.", false)]
    public class DataObject
    {
    }
}
```

### Example: Attribute on a Method

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_12_Attributes.cs (Initialize method)
.AddMethod(methodBuilder => methodBuilder
    .WithName("InitializeData")
    .WithReturnType("void")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithAttribute("Conditional", "(\"DEBUG\")") // Adds [Conditional("DEBUG")]
    .WithBody("// Initialization logic...")
    .Build())
```

### Example: Attribute on a Property

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_12_Attributes.cs (Description property)
.AddProperty(propertyBuilder => propertyBuilder
    .WithName("Description")
    .WithType("string")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithAttribute("Tooltip", "(\"Enter a description here\")") // Adds [Tooltip("...")]
    .WithAttribute("field: SerializeField") // Adds [field: SerializeField]
    .WithAutoProperty()
    .Build())
```

### Example: Attribute on a Field

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_12_Attributes.cs (_id field)
.AddField(fieldBuilder => fieldBuilder
    .WithName("_id")
    .WithType("int")
    .WithModifiers(SyntaxKind.PrivateKeyword)
    .WithAttribute("Range", "(1, 1000)") // Adds [Range(1, 1000)]
    .Build())
```

## Attribute Targets

Sometimes, you need to specify the target of an attribute (e.g., `field:`, `property:`, `event:`). You can include the target specifier directly in the attribute name string passed to `WithAttribute`.

```csharp
// Example for a property where you want to target the backing field for serialization
.AddProperty(pb => pb
    .WithName("MyAutoProperty")
    .WithType("int")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithAutoProperty()
    .WithAttribute("field: SerializeField") // Targets the auto-generated backing field
    .Build())
```

## Multiple Attributes

To add multiple attributes to a single code element, simply call `WithAttribute` multiple times.

```csharp
.AddClass(cb => cb
    .WithName("MySpecialClass")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithAttribute("System.Serializable")
    .WithAttribute("System.Diagnostics.DebuggerDisplay", "(\"Count = {count}\")")
    .Build())
```

## Custom Attributes

If you have defined your own custom attributes, you can apply them using `WithAttribute` just like standard .NET attributes. Ensure that the namespace of the custom attribute is imported via `AddUsingStatement` if it's not in the current scope.

## Limitations

*   **Attribute Arguments**: The `parameters` argument in `WithAttribute(string attributeName, string parameters)` is a single string. You need to format this string correctly according to how attribute arguments are written in C# (e.g., `"(\"Hello\", NamedArg = true)"`). The builder does not parse or validate the argument syntax beyond basic parentheses.
*   **Complex Attribute Arguments**: For attributes with complex arguments like `typeof()` expressions or array initializers, you might need to ensure the string representation is exact. For example, for `typeof(MyType)`, you might pass `"(typeof(MyNamespace.MyType))"` as the parameters string.

For general information on comments, which can also be applied to attributes or the elements they decorate, see [Comments](Comments.md).

