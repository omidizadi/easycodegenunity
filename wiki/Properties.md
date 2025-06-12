# Generating Properties

Properties in C# provide flexible access to class or struct data. EasyCodeGen uses `EasyPropertyBuilder` to generate
property declarations, typically via the `AddProperty` method on `EasyCodeBuilder`.

## Basic Property Generation

To add a property, use `AddProperty` on `EasyCodeBuilder`. This method takes a lambda expression for configuring the
property with `EasyPropertyBuilder`.

Key methods on `EasyPropertyBuilder`:

* `WithName(string name)`: Sets the name of the property. (Required)
* `WithType(string type)`: Sets the type of the property. (Required)
* `WithModifiers(params SyntaxKind[] modifiers)`: Sets access modifiers for the property (e.g.,
  `SyntaxKind.PublicKeyword`).
* `WithoutGetter()`: Removes the getter, making the property write-only (if a setter exists).
* `WithoutSetter()`: Removes the setter, making the property read-only (if a getter exists).
* `Build()`: Finalizes the property definition. (Required)

### Example: Simple Auto-Property

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_7_AdvancedProperty.cs (SimpleProperty)
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Models")
    .AddClass(typeBuilder => typeBuilder.WithName("DataModel").WithModifiers(SyntaxKind.PublicKeyword).Build())
    .AddProperty(propertyBuilder => propertyBuilder
        .WithName("SimpleProperty")
        .WithType("int")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Models
{
    public class DataModel
    {
        public int SimpleProperty { get; set; }
    }
}
```

### Example: Read-Only Auto-Property

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_6_Property.cs
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Models")
    .AddClass(typeBuilder => typeBuilder.WithName("ImmutableData").WithModifiers(SyntaxKind.PublicKeyword).Build())
    .AddProperty(propertyBuilder => propertyBuilder
        .WithName("SampleProperty")
        .WithType("int")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithoutSetter()    // Then remove the setter to make it { get; }
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Models
{
    public class ImmutableData
    {
        public int SampleProperty { get; }
    }
}
```

## Custom Accessors (Getters and Setters)

You can define custom logic for getters and setters.

* `WithGetterBody(params string[] statements)`: Defines a block body for the getter.
* `WithSetterBody(params string[] statements)`: Defines a block body for the setter.
* `WithExpressionBodiedGetter(string expression)`: Defines an expression body for the getter.
* `WithExpressionBodiedSetter(string expression)`: Defines an expression body for the setter.
* `WithGetterModifiers(params SyntaxKind[] modifiers)`: Sets specific access modifiers for the getter (e.g.,
  `SyntaxKind.PrivateKeyword` for a private get).
* `WithSetterModifiers(params SyntaxKind[] modifiers)`: Sets specific access modifiers for the setter (e.g.,
  `SyntaxKind.PrivateKeyword` for a private set).

### Example: Property with Backing Field and Custom Logic

This example uses a private field `_internalCounter` and provides custom getter/setter logic for the `Counter` property.

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_7_AdvancedProperty.cs (Counter property)
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Models")
    .AddClass(typeBuilder => typeBuilder.WithName("DataTracker").WithModifiers(SyntaxKind.PublicKeyword).Build())
    .AddField(fieldBuilder => fieldBuilder // The backing field
        .WithName("_internalCounter")
        .WithType("int")
        .WithModifiers(SyntaxKind.PrivateKeyword)
        .Build())
    .AddProperty(propertyBuilder => propertyBuilder
        .WithName("Counter")
        .WithType("int")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithExpressionBodiedGetter("_internalCounter") // get => _internalCounter;
        .WithExpressionBodiedSetter("_internalCounter = value < 0 ? 0 : value") // set => _internalCounter = ...;
        .Build())
    .Generate()
    .Save();
```

This generates:

```csharp
namespace MyApplication.Models
{
    public class DataTracker
    {
        private int _internalCounter;

        public int Counter
        {
            get => _internalCounter;
            set => _internalCounter = value < 0 ? 0 : value;
        }
    }
}
```

### Example: Property with Private Setter

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_7_AdvancedProperty.cs (ReadMostlyProperty)
.AddProperty(propertyBuilder => propertyBuilder
    .WithName("ReadMostlyProperty")
    .WithType("string")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithSetterModifiers(SyntaxKind.PrivateKeyword) // public string ReadMostlyProperty { get; private set; }
    .Build())
```

This generates:

```csharp
namespace MyApplication.Models
{
    public class DataModel
    {
        public string ReadMostlyProperty { get; private set; }
    }
}
```

## Properties with Backing Fields (Simplified)

`EasyPropertyBuilder` can simplify properties that use a backing field. If `WithBackingField(string fieldName)` is
called, and no custom getter/setter bodies are provided, it defaults to `get => fieldName;` and
`set => fieldName = value;`.

* `WithBackingField(string fieldName)`: Specifies the name of the private field to use for storage.

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_7_AdvancedProperty.cs (BackedProperty)
.AddField(fieldBuilder => fieldBuilder
    .WithName("_backingValue")
    .WithType("double")
    .WithModifiers(SyntaxKind.PrivateKeyword)
    .Build())
.AddProperty(propertyBuilder => propertyBuilder
    .WithName("BackedProperty")
    .WithType("double")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithBackingField("_backingValue") // Automatically creates getter and setter for _backingValue
    .Build())
```

This generates:

```csharp
namespace MyApplication.Models
{
    public class DataModel
    {
        private double _backingValue;

        public double BackedProperty
        {
            get => _backingValue;
            set => _backingValue = value;
        }
    }
}
```

## Using Templates for Accessors

You can source getter and setter bodies from a template class.

* `WithGetterFromTemplate(string propertyNameInTemplate)`: Copies the getter body from the specified property in the
  template.
* `WithSetterFromTemplate(string propertyNameInTemplate)`: Copies the setter body from the specified property in the
  template.
* `ReplaceInGetterBody(string search, string replace)`: Modifies the copied getter body.
* `ReplaceInSetterBody(string search, string replace)`: Modifies the copied setter body.

### Example: Property Accessors from Template

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_10_TemplateUsage.cs
// Assuming EasyCodeBuilder was initialized with .WithTemplate<BasicTemplate>()
// and a private field "name" exists in the target class.

.AddProperty(propertyBuilder => propertyBuilder
    .WithName("DisplayName")
    .WithType("string")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithGetterFromTemplate(nameof(BasicTemplate.DisplayName)) // Uses BasicTemplate.DisplayName's getter
    .WithSetterFromTemplate(nameof(BasicTemplate.DisplayName)) // Uses BasicTemplate.DisplayName's setter
    .ReplaceInGetterBody("_NAME_PLACEHOLDER_", "name")
    .ReplaceInSetterBody("_NAME_PLACEHOLDER_", "name")
    .Build())
```

This generates:

```csharp
namespace MyApplication.Models
{
    public class DataModel
    {
        private string name;

        public string DisplayName
        {
            get => name.ToUpper(); // Example logic from BasicTemplate.DisplayName
            set => name = value.Trim(); // Example logic from BasicTemplate.DisplayName
        }
    }
}
```

For adding comments and attributes to properties, see [Comments](Comments.md) and [Attributes](Attributes.md).

