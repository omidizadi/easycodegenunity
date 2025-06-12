# Querying Types (Classes, Structs, Interfaces, Enums)

EasyCodeGen provides a fluent API for querying types within your current application domain (including your project's assemblies and referenced assemblies). This is primarily achieved using the static methods on `EasyQuery` and the chainable methods on `EasyQueryCollection`.

This capability is useful for generators that need to make decisions based on existing code, such as finding all classes implementing a specific interface or all types adorned with a certain attribute.

## Starting a Query

All queries start with a static method call on `EasyQuery`.

### Key `EasyQuery` Static Methods for Types:

*   `EasyQuery.WithAttribute<T>()`: Finds all types (classes, structs, interfaces, enums) that have the attribute `T`.
    *   Returns: `EasyQueryCollection`
*   `EasyQuery.WithInheritingFrom<T>()`: Finds all types that inherit from class `T` (excluding `T` itself).
    *   Returns: `EasyQueryCollection`
*   `EasyQuery.WithImplementing<T>()`: Finds all types (classes, structs) that implement interface `T`.
    *   Throws `ArgumentException` if `T` is not an interface.
    *   Returns: `EasyQueryCollection`
*   `EasyQuery.WithAllTypesInAssembly(string assemblyName)`: Gets all types within a specific assembly (e.g., "Assembly-CSharp").
    *   Returns: `EasyQueryCollection`
*   `EasyQuery.WithAllTypesInNamespace(string namespaceName)`: Gets all types within a specific namespace across all loaded assemblies.
    *   Returns: `EasyQueryCollection`
*   `EasyQuery.WithType<T>()`: Creates an `EasyQueryCollection` containing just the single type `T`.
    *   Useful for starting a chain or focusing on one type.
    *   Returns: `EasyQueryCollection`

## `EasyQueryResult`

Each item found by a query is represented by an `EasyQueryResult` struct. It contains:

*   `Name`: `string` - The simple name of the type (e.g., "MyClass").
*   `FullName`: `string` - The fully qualified name of the type (e.g., "MyNamespace.MyClass").
*   `Type`: `System.Type` - The actual `Type` object.
*   `FriendlyTypeName`: `string` - A user-friendly display name for the type, especially for generic types (e.g., "List<string>" instead of "List`1[System.String]").
*   `Namespace`: `string` - The namespace of the type.

## Chaining Queries with `EasyQueryCollection`

The `EasyQueryCollection` (which is a `List<EasyQueryResult>`) provides methods to further refine or operate on the query results. These methods return a new `EasyQueryCollection`, allowing for fluent chaining.

### Key `EasyQueryCollection` Methods for Filtering:

*   `.WithAttribute<T>()`: Filters the current collection to types that also have attribute `T`.
*   `.WithInheritingFrom<T>()`: Filters to types that also inherit from `T`.
*   `.WithImplementing<T>()`: Filters to types that also implement interface `T`.
*   `.Where(Func<EasyQueryResult, bool> predicate)`: Filters based on a custom predicate.
*   `.OrderBy<TKey>(Func<EasyQueryResult, TKey> keySelector)`: Sorts the collection.
*   `.Select(Func<EasyQueryResult, EasyQueryResult> selector)`: Projects each element (less common for basic type filtering, more for transformation).

## Examples

### Example 1: Find all public classes in "MyGame.Features" namespace that implement `IController`

```csharp
// Inside an IEasyCodeGenerator Execute() method or similar context

EasyQueryCollection controllers = EasyQuery.WithAllTypesInNamespace("MyGame.Features")
    .WithImplementing<IController>()
    .Where(result => result.Type.IsClass && result.Type.IsPublic);

foreach (EasyQueryResult controllerInfo in controllers)
{
    Console.WriteLine($"Found controller: {controllerInfo.FullName}");
    // Now you can use controllerInfo.Type for reflection or controllerInfo.Name for generation
}
```

### Example 2: Find all structs with `[NetworkMessage]` attribute in the main game assembly

```csharp
// Assuming [NetworkMessage] is an attribute you've defined
EasyQueryCollection networkMessages = EasyQuery.WithAllTypesInAssembly("Assembly-CSharp")
    .WithAttribute<NetworkMessageAttribute>() // Use the attribute's type
    .Where(result => result.Type.IsValueType && !result.Type.IsEnum); // Ensure it's a struct

foreach (EasyQueryResult msgInfo in networkMessages)
{
    Console.WriteLine($"Network message struct: {msgInfo.Name}");
}
```

### Example 3: Get a specific type and then find its members (see [Querying Members](QueryingMembers.md))

```csharp
EasyQueryCollection myClassQuery = EasyQuery.WithType<MySpecificClass>();
EasyQueryResult myClassResult = myClassQuery.FirstOrDefault(); // Or loop if expecting multiple from a broader query

if (myClassResult.Type != null)
{
    // Proceed to query members of myClassResult.Type
    List<EasyQueryResult> methods = myClassResult.WithMembers<MyMethodAttribute>();
}
```

## Usage in Generators

Querying types is powerful for:

*   **Automated Registrations**: Find all services implementing `IService` and generate registration code.
*   **Data-Driven Generation**: Find all types marked with `[DataObject]` and generate CRUD operations or UI for them.
*   **Validation/Analysis**: Check if types adhere to certain architectural rules (e.g., all handlers inherit from `BaseHandler`).
*   **Extensibility**: Discover plugins or modules based on implemented interfaces or attributes.

Remember to handle cases where queries might return no results (e.g., by checking `Count` on the `EasyQueryCollection` or using `FirstOrDefault()`).

