# HashStamp

**HashStamp** is a lightweight incremental source generator for .NET projects that leverages Roslyn's incremental generator API. It analyzes your source code during the build process and automatically generates unique hash values for each method's body. These hashes can be used for integrity checks, debugging, or any scenario that benefits from a "fingerprint" of your code.

## Features

- **Automatic Hash Generation:**  
  Automatically generates a `HashStamps` class containing properties that represent the hash values of each method's source code.

- **Dual Access Modes:**  
  Access hashes at compile time or perform dynamic lookup using a hierarchical structure organized by namespace, class, and method names.

- **Incremental Performance:**  
  Optimized for speed by only regenerating hashes for methods that have changed since the last build.

- **Easy Integration:**  
  Add it to your project via NuGet and enjoy seamless integration without complex configuration.

## Getting Started

### Installation

To start using HashStamp in your project, add it as a NuGet package. You can do this by including the following in your project file:

```xml
<PackageReference Include="HashStamp" Version="1.0.0" />
```

Or install it via the command line:

```bash
dotnet add package HashStamp --version 1.0.0
```

### Usage

After installation, the generator will create a `HashStamps` class that you can use in your code. Here are two common usage patterns:

#### Compile-Time Access

Access method hashes directly via the generated class:

```csharp
// Direct access to method hashes
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod1);
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod2);
```

#### Dynamic Lookup

Retrieve hashes dynamically using a lookup structure based on namespaces, classes, and methods:

```csharp
// Dynamic lookup by namespace, class, and method name
Console.WriteLine(HashStamps.Namespaces["HashStamp.Test"]
                              .Classes["TestClass1"]
                              .Methods["TestMethod1"].Hash);
```

These examples illustrate how HashStamp seamlessly integrates with your code to provide both compile-time and runtime access to method hashes.

## Development

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or a later version

### Building the Project

Clone the repository and build the solution using the .NET CLI:

```bash
git clone https://github.com/alexwiese/HashStamp.git
cd src/HashStamp
dotnet build
```

### Testing

Run the tests to ensure everything works as expected:

```bash
dotnet test
```

## Contributing

Contributions are welcome! If you have suggestions, bug fixes, or new features to propose, please open an issue or submit a pull request. For detailed contribution guidelines, see our [Contributing Guidelines](CONTRIBUTING.md).

## License

This project is licensed under the [MIT License](LICENSE). Feel free to use and adapt HashStamp as needed in your own projects.

---

Happy coding—and may your method hashes always remain unique and secure!

---
