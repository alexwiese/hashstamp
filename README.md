# HashStamp

HashStamp is an incremental source generator built for .NET projects. It leverages Roslyn's incremental generator API to efficiently generate code during the build process.

## Features

- Automatically generates a `HashStamps` class containing properties that represent the hash values of each method's source code body.
- Supports both compile-time access and dynamic lookup of method hashes.

## Getting Started

### Installation

To use HashStamp in your project, add it as a NuGet package reference:

```
<PackageReference Include="HashStamp" Version="1.0.0" />
```

### Usage

```csharp
// Compile time access to method hashes
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod1);
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod2);

// Or dynamically/lookup by string
Console.WriteLine(HashStamps.Namespaces["HashStamp.Test"].Classes["TestClass1"].Methods["TestMethod1"].Hash);
```

## Development

### Prerequisites

- .NET SDK
- Visual Studio 2022 or later

### Building the Project

Clone the repository and build the solution:

```
dotnet build
```

### Testing

Run the tests to ensure everything is working as expected:

```
dotnet test
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the MIT License. See the LICENSE file for details.