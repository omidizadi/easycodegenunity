# Getting Started

This guide will help you set up and start using EasyCodeGen.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Installation](#installation)
    - [Option 1: Installation via Git URL](#option-1-installation-via-git-url)
    - [Option 2: Installation via local folder](#option-2-installation-via-local-folder)
- [Quick Start Examples](#quick-start-examples)
- [How to Run Generators?](#how-to-run-generators)
- [Documentation](#documentation)

## Prerequisites

- Unity (2021.3 or later)

## Installation

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

## Quick Start Examples

First, we need to create a C# class that will act as the generator. This class will inherit from `IEasyCodeGenerator`
and implement the `Execute` method.

Then you can use the `EasyCodeBuilder` to define the structure of the code you want to generate.

```csharp
    public class MyGenerator : IEasyCodeGenerator
    {
        public void Execute()
        {
            new EasyCodeBuilder()
                // Importing UnityEngine namespace to use MonoBehaviour and Debug classes
                .AddUsingStatement("UnityEngine")
                // Defining the namespace for the generated code
                .AddNamespace("MyGame.Scripts")
                .AddClass(typeBuilder => typeBuilder
                    // Name of the class
                    .WithName("HelloWorldSample") 
                    // Inheriting from MonoBehaviour to make it a Unity script
                    .WithBaseType("MonoBehaviour")
                    // Making the class public
                    .WithModifiers(SyntaxKind.PublicKeyword) 
                    // you should call Build() to finalize the type definition
                    .Build()) 
                .AddMethod(methodBuilder => methodBuilder
                    // Name of the method
                    .WithName("Start") 
                    // Return type of the method
                    .WithReturnType("void") 
                    // Body of the method that logs "Hello, World!" to the console
                    .WithBody("Debug.Log(\"Hello, World!\");") 
                    // Making the method private, as it is a Unity lifecycle method
                    .WithModifiers(SyntaxKind.PrivateKeyword) 
                    // Finalizing the method definition
                    .Build()) 
                // Setting the directory where the generated file will be saved
                .SetDirectory("Assets/MyGame/Scripts/Generated")
                // Setting the name of the generated file
                .SetFileName("HelloWorldSample.cs") 
                // Generating the code based on the defined structure
                .Generate() 
                .Save();
        }
    }
```

This code will generate this class:

```csharp
using UnityEngine;
namespace MyGame.Scripts
{
    public class HelloWorldSample : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("Hello, World!");
        }
    }
}
```

## How to Run Generators?

To run code generators, use the EasyCodeGen Editor Window:

1. Open the window via `Window` > `EasyCodeGen`.
2. Select the generators you want to run.
3. Click the "Generate Code" button.

## Documentation

For more details, see the [Docs](Docs.md) page.
