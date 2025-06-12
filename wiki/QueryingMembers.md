# Querying Members (Properties, Methods, Fields, Events)

Once you have queried and identified specific types using `EasyQuery` and `EasyQueryCollection` (see [Querying Types](QueryingTypes.md)), you might need to inspect their members (properties, methods, fields, events). EasyCodeGen provides an extension method for `EasyQueryResult` to facilitate this.

## Querying Members of a Type

The primary method for querying members is an extension method on `EasyQueryResult`:

*   `WithMembers<TAttribute>(this EasyQueryResult resultClass)`: Finds all members (fields, properties, methods, events) of the type represented by `resultClass` that are decorated with the attribute `TAttribute`.
    *   **`resultClass`**: An `EasyQueryResult` instance obtained from a type query.
    *   **`TAttribute`**: The type of the attribute to search for on members.
    *   Returns: `List<EasyQueryResult>`. Each `EasyQueryResult` in this list represents a *member*, not a type.

### `EasyQueryResult` for Members

When `EasyQueryResult` represents a member, its fields are populated as follows:

*   `Name`: `string` - The name of the member (e.g., "MyProperty", "CalculateValue").
*   `FullName`: `string` - A string indicating the member type and name (e.g., "Property.MyProperty", "Method.CalculateValue"). This is a convention by `EasyQuery` and not a standard reflection format.
*   `Type`: `System.Type` - Represents the type of the member itself:
    *   For Properties: `PropertyInfo.PropertyType`
    *   For Fields: `FieldInfo.FieldType`
    *   For Methods: `MethodInfo.ReturnType` (Note: parameter types are not directly in `EasyQueryResult` but can be accessed via reflection on `MethodInfo` if you get the `MemberInfo` object).
    *   For Events: `EventInfo.EventHandlerType`
*   `FriendlyTypeName`: `string` - The friendly name of the member's type (as described above).
*   `Namespace`: `string` - The namespace of the *declaring type* of the member.

**Important**: To get the actual `MemberInfo` (like `PropertyInfo`, `MethodInfo`, etc.) for more detailed reflection, you would typically retrieve the `Type` from the original `EasyQueryResult` (that represents the class/struct) and then use standard reflection methods like `GetMembers()`, `GetProperties()`, etc., filtering by the member name obtained from the member query result.
The `WithMembers<TAttribute>` method itself uses `resultClass.Type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)` so it considers instance members, both public and non-public.

## Examples

### Example 1: Find all properties with `[SerializeField]` attribute in a class

```csharp
// Assume MyComponent is a class and SerializeFieldAttribute is the attribute
// First, get the EasyQueryResult for MyComponent
EasyQueryResult componentResult = EasyQuery.WithType<MyComponent>().FirstOrDefault();

if (componentResult.Type != null)
{
    // Now, query its members for [SerializeField]
    List<EasyQueryResult> serializedFieldsOrProperties = componentResult.WithMembers<SerializeFieldAttribute>();

    foreach (EasyQueryResult memberInfo in serializedFieldsOrProperties)
    {
        // memberInfo.Name will give the name of the field or property
        // memberInfo.Type will give its type (e.g., typeof(int), typeof(string))
        Console.WriteLine($"Member to serialize: {memberInfo.Name} of type {memberInfo.FriendlyTypeName}");

        // To distinguish between fields and properties, or get more details:
        MemberInfo[] actualMemberInfos = componentResult.Type.GetMember(memberInfo.Name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (actualMemberInfos.Length > 0)
        {
            MemberInfo actualMember = actualMemberInfos[0];
            if (actualMember is PropertyInfo)
            {
                Console.WriteLine("  (It is a property)");
            }
            else if (actualMember is FieldInfo)
            {
                Console.WriteLine("  (It is a field)");
            }
        }
    }
}
```

### Example 2: Find all methods with `[CommandHandler]` attribute

```csharp
// Assume CommandHandlerAttribute is defined
// Query for a class that might contain command handlers
EasyQueryResult serviceResult = EasyQuery.WithAllTypesInNamespace("MyApplication.Services")
                                     .Where(r => r.Name == "CommandProcessingService")
                                     .FirstOrDefault();

if (serviceResult.Type != null)
{
    List<EasyQueryResult> commandHandlers = serviceResult.WithMembers<CommandHandlerAttribute>();

    foreach (EasyQueryResult handlerMethod in commandHandlers)
    {
        Console.WriteLine($"Command handler method: {handlerMethod.Name}");
        Console.WriteLine($"  Return type: {handlerMethod.FriendlyTypeName}");

        // To get parameter info, you'd use reflection on the actual MethodInfo
        MethodInfo method = serviceResult.Type.GetMethod(handlerMethod.Name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null)
        {
            foreach (ParameterInfo param in method.GetParameters())
            {
                Console.WriteLine($"    Param: {param.Name} of type {param.ParameterType.Name}");
            }
        }
    }
}
```

## General Member Querying (Without Attribute Filter)

The current `EasyQuery` system is primarily designed to find members *with specific attributes*. There isn't a direct `EasyQuery` method like `GetAllMembers(EasyQueryResult resultClass)` that returns all members without an attribute filter into the `List<EasyQueryResult>` format.

If you need to list all members (or filter by criteria other than attributes, like name patterns, types, or accessibility) directly into `EasyQueryResult` objects, you would typically:

1.  Get the `System.Type` from an `EasyQueryResult` for a type.
2.  Use standard .NET reflection (e.g., `type.GetMembers()`, `type.GetProperties()`, `type.GetMethods()`, `type.GetFields()`) with appropriate `BindingFlags`.
3.  Manually create `EasyQueryResult` objects for each `MemberInfo` if you want to use them in the `EasyQueryCollection` ecosystem or for consistent data handling in your generator.

```csharp
// Example: Manually creating EasyQueryResult for all public instance properties
EasyQueryResult classResult = EasyQuery.WithType<MyClass>().FirstOrDefault();
var memberResults = new List<EasyQueryResult>();

if (classResult.Type != null)
{
    PropertyInfo[] properties = classResult.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
    foreach (PropertyInfo prop in properties)
    {
        memberResults.Add(new EasyQueryResult
        {
            Name = prop.Name,
            FullName = "Property." + prop.Name, // Convention

