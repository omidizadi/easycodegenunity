![Header](https://github.com/user-attachments/assets/08d67d33-acce-4c64-8fe3-2500614f79c7)

# Easy Code Gen for Unity 🚀

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity](https://img.shields.io/badge/Unity-2021.3+-black.svg)](https://unity.com)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](http://makeapullrequest.com)
[![Made with C#](https://img.shields.io/badge/Made%20with-C%23-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-blue.svg)](https://www.linkedin.com/in/omidrezaizadi/)
![GitHub Repo size](https://img.shields.io/github/repo-size/omidizadi/easycodegenunity)
![GitHub Repo stars](https://img.shields.io/github/stars/omidizadi/easycodegenunity?style=social)

A powerful code generation tool for Unity that simplifies creating boilerplate code, repetitive code structures, and
streamlines your development workflow. ✨

## 📋 Table of Contents

- [Why Easy Code Gen?](#-why-easy-code-gen)
- [Features](#-features)
- [Installation](#-installation)
- [Quick Start Examples](#-quick-start-examples)
- [Where Code Generation Shines](#-where-code-generation-shines)
- [Optional Samples](#-optional-samples)
- [Documentation](#-documentation)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Why Easy Code Gen?

Game development often involves writing repetitive code patterns across many files. As your project scales, maintaining
consistency becomes challenging. Easy Code Gen solves this by:

- **Eliminating Boilerplate**: Generate repeatable code structures with minimal effort
- **Ensuring Consistency**: Maintain uniform code patterns across your entire codebase
- **Accelerating Development**: Reduce time spent on repetitive coding tasks
- **Improving Maintainability**: Update code patterns in one place, apply everywhere
- **Reducing Errors**: Minimize bugs that come from manual copy-pasting

## 🛠️ Features

- 📊 **Type-Safe API** - No more string manipulation or regex errors
- 🧩 **Template System** - Create and reuse code templates
- 🔍 **Code Analysis** - Query and modify your existing codebase
- ⚡ **Fast Generation** - Generate hundreds of files in seconds
- 🔄 **Auto-Update** - Keep generated code in sync with your templates
- 🧪 **Unit Test Support** - Test your generators, not just the generated code

## 📥 Installation

### Option 1: Installation via Git URL

1. Open the Unity Package Manager window (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter:

```text
https://github.com/omidizadi/easycodegenunity.git
```

5. Click "Add"

### Option 2: Installation via local folder

1. Clone this repository to your local machine
2. Open the Unity Package Manager window (Window > Package Manager)
3. Click the "+" button in the top-left corner
4. Select "Add package from disk..."
5. Navigate to and select the folder containing the package

## 🚀 Quick Start Examples

### Example 1: Generation API

Easily generate new code with a fluent API:

```csharp
// Generate a simple MonoBehaviour that logs "Hello, World!"
new EasyCodeBuilder()
    .AddUsingStatement("UnityEngine")
    .AddNamespace("MyGame.Generated")
    .AddClass(cb => cb
        .WithName("HelloWorldBehavior")
        .WithBaseType("MonoBehaviour")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddMethod(mb => mb
        .WithName("Start")
        .WithReturnType("void")
        .WithModifiers(SyntaxKind.PrivateKeyword)
        .WithBody("Debug.Log(\"Hello, World!\");")
        .Build())
    .SetDirectory("Assets/Scripts/Generated")
    .SetFileName("HelloWorldBehavior.cs")
    .Generate()
    .Save();
```

The result:

```csharp
using UnityEngine;
namespace MyGame.Generated
{
    public class HelloWorldBehavior : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("Hello, World!");
        }
    }
}
```

### Example 2: Template-Based Code Generation 📝

Create code from existing templates:

```csharp
// Generate code using a template
new EasyCodeBuilder()
    .WithTemplate<GameDataEventTemplate>()
    .AddUsingStatement("UnityEngine")
    .AddNamespace("MyGame.Systems")
    .AddClass(cb => cb
        .WithName("PlayerHealthSystem")
        .WithBaseType("MonoBehaviour")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .Build())
    .AddMethod(mb => mb
        .WithName("TakeDamage")
        .WithReturnType("void")
        .WithModifiers(SyntaxKind.PublicKeyword)
        .WithParameters(("int", "amount"))
        .WithBodyFromTemplate(nameof(GameDataEventTemplate.TakeDamage))
        .ReplaceInBody("_TARGET_FIELD_", "health")
        .Build())
    .SetDirectory("Assets/Scripts/Generated")
    .SetFileName("PlayerHealthSystem.cs")
    .Generate()
    .Save();
```

### Example 3: Querying and Modifying Existing Code 🔍

Easy Code Gen can analyze existing code with the query system:

```csharp
// Query existing code with attributes
foreach (var queryResult in EasyQuery.WithAttribute<GameData>())
{
    // Build a new code file based on query results
    var builder = new EasyCodeBuilder();
    builder
        .WithTemplate<GameDataControllerTemplate>()
        .AddUsingStatement("System")
        .AddUsingStatement("UnityEngine")
        .AddNamespace(queryResult.Namespace)
        .AddClass(cb => cb
            .WithName(queryResult.Name + "Controller")
            .WithModifiers(SyntaxKind.PublicKeyword)
            .Build());
    
    // Query for specific members with attributes
    var members = queryResult.WithMembers<GameDataField>();
    
    // Generate methods for each member
    foreach (var memberQueryResult in members)
    {
        builder
            .AddMethod(mb => mb
                .WithName($"Process{memberQueryResult.Name.ToPascalCase()}")
                .WithReturnType("void")
                .WithModifiers(SyntaxKind.PublicKeyword)
                .WithBodyFromTemplate(nameof(GameDataControllerTemplate.ProcessMember))
                .ReplaceInBody("_MEMBER_NAME_", memberQueryResult.Name)
                .Build());
    }
    
    // Save the generated code
    builder
        .SetDirectory("Assets/Generated")
        .SetFileName(queryResult.Name + "Controller.cs")
        .Generate()
        .Save();
}
```

## 🚀 How to Run Generators?

To run code generators, use the EasyCodeGen Editor Window:

1. Open the window via `Window` > `EasyCodeGen`.
2. Select the generators you want to run.
3. Click the "Generate Code" button.

<img width="507" alt="EasyCodeGen Window" src="https://github.com/user-attachments/assets/81b277d3-f47b-449c-8682-6ae23cf4fa48" />

## 🌟 Where Code Generation Shines

### Game Systems Scaling 📈

As your game grows, you'll likely need similar patterns across many systems:

- **🎮 Event Systems**: Generate consistent event handling code
- **💾 Data Management**: Create data models, serialization, and validation
- **🖥️ UI Elements**: Generate UI controllers with standard patterns
- **⚙️ State Machines**: Build state machine classes with common structure
- **🔧 Editor Tools**: Create custom editors and property drawers

### Benefits of Code Generation with EasyCodeGen ✨

EasyCodeGen provides several advantages over traditional code generation approaches:

- 🔍 **Smart Code Querying**: Easily query your codebase to find types or members with specific attributes, interfaces,
  or base classes to generate code for

- 🌊 **Fluent and Intuitive API**: Clear, chainable methods make code generation readable and maintainable - what you see
  is what you get

- 🏗️ **Compiled Class Templates**: Uses actual C# classes as templates instead of brittle string-based templates, giving
  you compile-time safety

- 🧩 **Roslyn-Powered Generation**: Built on the .NET Compiler Platform for robust code generation with full language
  understanding

- 🔄 **Low Template Maintenance**: No string templates means no escaping issues, no syntax errors in templates, and full
  IDE support

- 🧪 **Testable Generation Logic**: Unit test your generators just like any other code in your project

## 📚 Optional Samples

This package includes optional samples demonstrating how to use Easy Code Gen:

1. Open the Package Manager window (Window > Package Manager)
2. Select "Easy Code Gen" from the package list
3. Find the "Samples" section in the package details
4. Click "Import" on any sample you want to explore

## 📖 Documentation

You can find detailed documentation on how to use Easy Code Gen here: [Wiki](https://github.com/omidizadi/easycodegenunity/wiki)

## 🗺️ Roadmap

- [ ] Add more built-in templates for common game systems
- [ ] Improve code analysis capabilities
- [ ] Add more query options for existing code
- [ ] Enhance performance for large codebases
- [ ] Support for more complex code generation scenarios
- [ ] Improve documentation and examples
- [ ] Add more unit tests for code generation logic

## 👥 Contributing

We welcome contributions to Easy Code Gen! Whether it's bug reports, feature requests, or code contributions, please
feel free to make a pull request or open an issue.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This package is licensed under the terms found in the LICENSE file.

## 🏷️ Tags

`unity` `game-development` `code-generation` `tooling` `boilerplate` `productivity` `editor-tools` `c-sharp`
`template-engine` `code-analysis` `ast` `roslyn`
