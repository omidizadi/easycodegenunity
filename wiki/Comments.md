# Generating Comments

Comments are crucial for making code understandable and maintainable. EasyCodeGen provides methods to add various types of comments to generated code elements through its builders (e.g., `EasyTypeBuilder`, `EasyMethodBuilder`), inherited from `EasyBasicBuilder`.

## Types of Comments

`EasyBasicBuilder` (and its descendants) support:

*   **Single-Line Comments**: `WithSingleLineComment(string commentText)`
    *   Generates `// commentText`
*   **Multi-Line Comments**: `WithMultiLineComment(string commentText)`
    *   Generates `/* commentText */`
*   **XML Documentation Comments (Summary)**: `WithSummaryComment(string commentText)`
    *   Generates `/// <summary>
/// commentText
/// </summary>`
*   **XML Documentation Comments (Parameter)**: `WithParamComment(string paramName, string description)`
    *   Generates `/// <param name="paramName">description</param>`
*   **XML Documentation Comments (Returns)**: `WithReturnsComment(string description)`
    *   Generates `/// <returns>description</returns>`

## Adding Comments

You chain these methods on a specific builder instance before calling `Build()`.

### Example: Comments on a Class and Field

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_11_Comments.cs
new EasyCodeBuilder()
    .AddNamespace("MyApplication.Models")
    .AddClass(classBuilder => classBuilder
        .WithName("UserAccount")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithSummaryComment("Represents a user account in the system.")
        .WithMultiLineComment("This class handles user data and authentication details.\nLast review: 2024-01-15")
        .Build())
    .AddField(fieldBuilder => fieldBuilder
        .WithName("_username")
        .WithType("string")
        .WithModifiers(SyntaxKind.PrivateKeyword)
        .WithSingleLineComment("Stores the username.")
        .Build())
    .Generate()
    .Save();
```

### Example: Comments on a Method (including Parameters and Return Value)

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_11_Comments.cs (adapted)
.AddMethod(methodBuilder => methodBuilder
    .WithName("UpdateProfile")
    .WithReturnType("bool")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithParameters(("string", "newEmail"), ("string", "newPassword"))
    .WithSummaryComment("Updates the user's profile information.")
    .WithParamComment("newEmail", "The new email address for the user.")
    .WithParamComment("newPassword", "The new password for the user. Should be hashed.")
    .WithReturnsComment("True if the update was successful, false otherwise.")
    .WithBody("// Implementation details...", "return true;")
    .Build())
```

## Manipulating Comments

`EasyBasicBuilder` also provides methods to modify existing comments, which is particularly useful when working with templates.

*   `WithCommentFromTemplate(string memberName)`: Extracts and applies the leading comment from a specified member in a template class (set via `EasyCodeBuilder.WithTemplate<T>()`).
*   `ReplaceInComment(string oldValue, string newValue)`: Replaces text within the current comment being built for the element.
*   `ReplaceParamCommentText(string paramName, string oldValue, string newValue)`: Specifically targets a `<param>` tag for a given `paramName` within the XML documentation and replaces text in its description.

### Example: Using Template Comments and Replacing Text

```csharp
// From: /Assets/easycodegenunity/Samples~/BasicExamples/Generators/_11_Comments.cs (ProcessData method)
// Assume EasyCodeBuilder was initialized with .WithTemplate<MyTemplate>()
// and MyTemplate has a method ProcessData with comments.

.AddMethod(methodBuilder => methodBuilder
    .WithName("ProcessUserData")
    .WithReturnType("void")
    .WithModifiers(SyntaxKind.PublicKeyword)
    .WithParameters(("List<User>", "users"))
    // Option 1: Start with a fresh summary and then modify
    .WithSummaryComment("Processes the {USER_TYPE} data in the provided collection.")
    .ReplaceInComment("{USER_TYPE}", "User object")
    // Option 2: If template had a comment for a method named "ProcessTemplateData"
    // .WithCommentFromTemplate("ProcessTemplateData") // This would pull the whole comment block
    // .ReplaceInComment("template items", "user objects") // Then modify it

    .WithParamComment("users", "The collection of {ITEM_TYPE} to process.")
    .ReplaceParamCommentText("users", "{ITEM_TYPE}", "User instances") // Specifically targets the 'users' param comment
    .WithBodyLine("// Actual processing logic here...")
    .Build())
```

## Comment Order and Structure

*   When multiple XML documentation comments are added (e.g., `WithSummaryComment`, `WithParamComment`), they are typically arranged by the Roslyn `SyntaxFactory` in a standard order (summary, then params, then returns).
*   A single-line or multi-line comment will usually appear before the XML documentation block if both are applied to the same element, as the XML doc comments are attached as structured trivia.

For adding attributes, which can also have comments or be placed near commented elements, see [Attributes](Attributes.md).

