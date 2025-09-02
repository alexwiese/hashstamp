# HashStamp - .NET Source Generator

HashStamp is a .NET Roslyn incremental source generator that analyzes source code and generates SHA-256 hash values for method bodies. The generated hashes can be accessed at compile-time via static properties or at runtime via a dynamic lookup structure.

**Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Contributing Guidelines Reference

This repository includes comprehensive contributing guidelines in [CONTRIBUTING.md](../CONTRIBUTING.md) for external contributors. These copilot instructions focus on development workflow and technical details, while CONTRIBUTING.md covers the complete contribution process including PR requirements, community guidelines, and getting started information.

## Working Effectively

### Bootstrap and Build
- **Prerequisites**: .NET 8.0+ SDK is required and available
- **Bootstrap commands (run in order)**:
  ```bash
  dotnet restore
  dotnet build
  ```
- **Build times**: 
  - `dotnet restore`: ~8 seconds (first time with package downloads)
  - `dotnet build`: ~6 seconds for full solution - NEVER CANCEL, set timeout to 60+ seconds
  - Individual project builds: ~2 seconds - NEVER CANCEL, set timeout to 30+ seconds

### Available Build Configurations
- **Debug** (default): Standard debug build
- **Release**: Optimized release build  
- **"Debug Source Generator"**: Debug build with source generator debugging enabled
- **Build commands**:
  ```bash
  dotnet build                                    # Debug build
  dotnet build --configuration Release           # Release build  
  dotnet build --configuration "Debug Source Generator"  # Debug with SG debugging
  ```

### Run and Test
- **Run the test application**:
  ```bash
  dotnet run --project src/HashStamp.Test/HashStamp.Test.csproj
  ```
  - **Run time**: ~2 seconds - NEVER CANCEL, set timeout to 30+ seconds
  - **Expected output**: Console application displays "Hello, World!" followed by SHA-256 hash values

- **Run benchmarks**:
  ```bash
  dotnet run --project src/HashStamp.Benchmarks/HashStamp.Benchmarks.csproj --configuration Release -- --quick
  ```
  - **Run time**: ~120 seconds maximum - NEVER CANCEL, set timeout to 150+ seconds
  - **Expected output**: Performance metrics for hash generation and access patterns

- **Unit tests**: No unit tests exist in this repository
  - `dotnet test` completes immediately with no tests found
  - Primary validation is via running the HashStamp.Test console application and benchmarks

### Code Formatting and Quality
- **Format code**: `dotnet format .`
- **Verify formatting**: `dotnet format --verify-no-changes .`
- **ALWAYS run formatting before committing changes**

## Validation Scenarios

### Critical Validation Steps
**ALWAYS perform these validation steps after making any changes to ensure the source generator works correctly:**

1. **Full build and run validation**:
   ```bash
   dotnet build
   dotnet run --project src/HashStamp.Test/HashStamp.Test.csproj
   ```
   - Verify the application outputs hash values without errors
   - Confirm no "CS0103: The name 'HashStamps' does not exist" compilation errors

2. **Benchmark validation** (for performance-related changes):
   ```bash
   dotnet run --project src/HashStamp.Benchmarks/HashStamp.Benchmarks.csproj --configuration Release -- --quick
   ```
   - Verify benchmarks run without errors
   - Check performance metrics are reasonable

3. **Hash generation verification**:
   - Modify a method body in any test class (e.g., change `"Hello, World!"` to `"Hello, Modified!"`)
   - Rebuild and run the application
   - Verify the corresponding hash value changes
   - Restore the original method body to revert the hash

4. **Code quality validation**:
   ```bash
   dotnet format --verify-no-changes .
   ```
   - Must pass with no formatting issues

### Manual Testing Scenarios
**ALWAYS test these scenarios when modifying source generator logic:**

- **Compile-time access**: Verify `HashStamps.HashStamp_Test.TestClass1.TestMethod1` works
- **Runtime access**: Verify `HashStamps.Namespaces["HashStamp.Test"].Classes["TestClass1"].Methods["TestMethod1"].Hash` works  
- **Method overloads**: Verify methods with same name but different parameters generate distinct hashes
- **Multiple namespaces**: Verify classes in different namespaces are handled correctly

## Common Issues and Solutions

### Build Failures
- **"CS0103: The name 'HashStamps' does not exist"**: Source generator failed to run
  - Check for Roslyn version compatibility issues
  - Verify analyzer reference is correct in project files
  - Current working versions: Microsoft.CodeAnalysis.CSharp 4.8.0

- **"CS1503: Argument 1: cannot convert from 'string' to 'char'"**: Fixed syntax error in generator
  - Use `TrimEnd('_')` not `TrimEnd("_")`

### Known Working Configuration
- **.NET SDK**: 8.0.119 (minimum .NET 8.0+ required)
- **Roslyn Version**: Microsoft.CodeAnalysis.CSharp 4.8.0
- **Target Frameworks**: 
  - HashStamp library: netstandard2.0
  - HashStamp.Test: net8.0

## Project Structure

### Repository Layout
```
├── src/
│   ├── HashStamp/                    # Source generator library
│   │   ├── HashStamp.csproj         # .NET Standard 2.0 project
│   │   └── HashStampGenerator.cs    # Main generator implementation
│   ├── HashStamp.Test/              # Test console application
│   │   ├── HashStamp.Test.csproj    # .NET 8.0 console project
│   │   ├── Program.cs               # Test application entry point
│   │   ├── TestClass1.cs            # Sample class with methods
│   │   ├── TestClass2.cs            # Sample class with methods
│   │   └── TestClass2b.cs           # Sample class in different namespace
│   └── HashStamp.Benchmarks/        # Performance benchmarking application
│       ├── HashStamp.Benchmarks.csproj  # .NET 8.0 console project
│       ├── Program.cs               # Benchmark runner entry point
│       └── QuickBenchmarks.cs       # Benchmark implementations
├── .github/
│   ├── workflows/                   # CI/CD GitHub Actions
│   │   ├── ci.yml                  # Main CI pipeline
│   │   ├── performance-diff.yml    # Performance comparison on PRs
│   │   └── release.yml             # Release validation and automation
│   └── copilot-instructions.md     # GitHub Copilot configuration
├── HashStamp.sln                    # Visual Studio solution file
├── README.md                        # Project documentation
├── LICENSE                          # MIT license
└── .gitignore                       # Git ignore rules
```

### Key Files to Monitor
- **HashStampGenerator.cs**: Core source generator logic - changes here require thorough validation
- **Test classes**: Changes to method bodies should trigger hash regeneration
- **Project files**: Analyzer references and configurations
- **Benchmark classes**: Performance test implementations that validate generator efficiency
- **CI workflows**: GitHub Actions that automate build, test, and release processes

## Development Workflow

For complete contributor guidelines including PR process, setup instructions, and community standards, see [CONTRIBUTING.md](../CONTRIBUTING.md). The sections below provide technical details for development.

### CI/CD Workflows
The repository includes automated workflows that validate all changes:

- **CI Pipeline (`.github/workflows/ci.yml`)**: 
  - Runs on all pushes to `main`/`master` and pull requests
  - Validates code formatting, builds in Release configuration, runs tests and benchmarks
  
- **Performance Diff (`.github/workflows/performance-diff.yml`)**: 
  - Automatically runs on pull requests to compare performance with base branch
  - Posts detailed performance analysis as PR comments
  
- **Release Validation (`.github/workflows/release.yml`)**:
  - Validates version increments on PRs to HashStamp.csproj
  - Automates GitHub releases when version tags are pushed

### Making Changes
1. **ALWAYS build first**: `dotnet build` to ensure current state is working
2. **Make minimal changes**: Focus on specific requirements only  
3. **Validate immediately**: Run validation scenarios after each change
4. **Format code**: `dotnet format .` before committing
5. **Final validation**: Complete all validation scenarios before PR submission

### Adding New Test Scenarios
- Add new methods to existing test classes in `src/HashStamp.Test/`
- Update `Program.cs` to exercise new functionality
- Verify both compile-time and runtime access patterns work
- Confirm hash values are generated and accessible

### Debugging Source Generators
- Use "Debug Source Generator" build configuration
- Source generator debugging is enabled via `#if DEBUG_SOURCE_GENERATOR` conditional compilation
- Generated source files are compiled into assemblies but not persisted to disk

Remember: The source generator runs during compilation and creates the `HashStamps` class with method hash constants and dynamic lookup structures. Always validate that both access patterns (static properties and dynamic lookups) work correctly after any changes.