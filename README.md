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

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or a later version

### Building the Project

Clone the repository and build the solution using the .NET CLI:

```bash
git clone https://github.com/alexwiese/HashStamp.git
cd HashStamp
dotnet restore
dotnet build
```

### Testing

Currently, the project uses a console application (`HashStamp.Test`) for validation rather than traditional unit tests:

```bash
# Run the test console application
dotnet run --project src/HashStamp.Test/HashStamp.Test.csproj

# Check for any formal unit tests (currently none)
dotnet test
```

### Code Formatting

The project uses `dotnet format` to maintain consistent code style:

```bash
# Check formatting
dotnet format --verify-no-changes

# Fix formatting issues
dotnet format
```

## Continuous Integration

This project includes a comprehensive CI/CD pipeline with the following workflows:

### Main CI Pipeline (`.github/workflows/ci.yml`)

Runs on all pushes to `main`/`master` and pull requests:

- ✅ **Code Formatting Check**: Ensures code follows consistent style using `dotnet format`
- ✅ **Build Validation**: Compiles the project in Release configuration
- ✅ **Functional Testing**: Runs the HashStamp.Test console application to verify source generator functionality
- ✅ **Unit Tests**: Runs any formal unit tests (currently none exist)

### Performance Diff Report (`.github/workflows/performance-diff.yml`)

Automatically runs on pull requests to generate performance comparisons:

- 📊 **Build Output Comparison**: Compares hash generation between PR and base branch
- 📊 **Assembly Size Tracking**: Monitors changes in generated assembly sizes
- 📊 **Runtime Performance**: Measures and compares execution times
- 💬 **Automated PR Comments**: Posts detailed performance reports as PR comments

### Release Validation (`.github/workflows/release.yml`)

Handles version management and releases:

- 🔢 **Version Increment Validation**: Checks if version was updated in PRs (warning only)
- 🏷️ **Tag Validation**: Ensures release tags match project versions
- 📦 **Automated Releases**: Creates GitHub releases when tags are pushed

### Creating a Release

To create a new release:

1. Update the version in `src/HashStamp/HashStamp.csproj`:
   ```xml
   <Version>1.1.0</Version>
   ```

2. Commit and push the version change

3. Create and push a tag:
   ```bash
   git tag v1.1.0
   git push origin v1.1.0
   ```

The release workflow will automatically validate the tag matches the project version and create a GitHub release.

## Contributing

Contributions are welcome! If you have suggestions, bug fixes, or new features to propose, please open an issue or submit a pull request. For detailed contribution guidelines, see our [Contributing Guidelines](CONTRIBUTING.md).

## License

This project is licensed under the [MIT License](LICENSE). Feel free to use and adapt HashStamp as needed in your own projects.

---

Happy coding�and may your method hashes always remain unique and secure!

---
