# Contributing to HashStamp

Thank you for your interest in contributing to HashStamp! We welcome contributions from the community and appreciate your help in making this .NET source generator even better.

## Table of Contents

- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Pull Request Process](#pull-request-process)
- [Code Standards](#code-standards)
- [Testing Guidelines](#testing-guidelines)
- [CI/CD Process](#cicd-process)
- [Release Process](#release-process)
- [Reporting Issues](#reporting-issues)
- [Community Guidelines](#community-guidelines)

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/) (optional but recommended)
- [Git](https://git-scm.com/)

### First Time Setup

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/hashstamp.git
   cd hashstamp
   ```
3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/alexwiese/hashstamp.git
   ```
4. **Install dependencies**:
   ```bash
   dotnet restore
   ```
5. **Build the project**:
   ```bash
   dotnet build
   ```
6. **Verify everything works**:
   ```bash
   dotnet run --project src/HashStamp.Test/HashStamp.Test.csproj
   ```

## Development Setup

### Building the Project

The project uses standard .NET build commands:

```bash
# Restore packages (first time or after package changes)
dotnet restore

# Build in Debug mode (default)
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Build with source generator debugging enabled
dotnet build --configuration "Debug Source Generator"
```

**Important:** Build times are typically 6-11 seconds. Never cancel builds early as they need time to complete properly.

### Running Tests

HashStamp uses a console application for primary testing rather than traditional unit tests:

```bash
# Run the main test console application
dotnet run --project src/HashStamp.Test/HashStamp.Test.csproj

# Run any formal unit tests (currently none exist)
dotnet test

# Run benchmarks for performance validation
dotnet run --project src/HashStamp.Benchmarks/HashStamp.Benchmarks.csproj --configuration Release -- --quick
```

**Expected output from test application:**
- "Hello, World!" message
- Multiple SHA-256 hash values displayed
- No compilation errors

## Pull Request Process

### Before You Start

1. **Check existing issues** to see if your contribution is already being worked on
2. **Create an issue** for new features or major changes to discuss the approach
3. **Keep changes focused** - one feature/fix per PR

### Creating a Pull Request

1. **Create a feature branch** from the main branch:
   ```bash
   git checkout main
   git pull upstream main
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following our [code standards](#code-standards)

3. **Test your changes** thoroughly:
   ```bash
   # Build and verify
   dotnet build
   dotnet run --project src/HashStamp.Test/HashStamp.Test.csproj
   
   # Check code formatting
   dotnet format --verify-no-changes
   
   # Run benchmarks if performance-related
   dotnet run --project src/HashStamp.Benchmarks/HashStamp.Benchmarks.csproj --configuration Release -- --quick
   ```

4. **Commit your changes** with clear, descriptive messages:
   ```bash
   git add .
   git commit -m "Add feature: brief description of what was added"
   ```

5. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create a Pull Request** on GitHub with:
   - Clear title describing the change
   - Detailed description of what was changed and why
   - Reference to any related issues (e.g., "Fixes #123")

### Pull Request Requirements

✅ **Your PR must:**
- Pass all CI checks (formatting, build, tests)
- Include appropriate tests for new functionality
- Follow the existing code style and patterns
- Have a clear description of changes made
- Reference related issues

✅ **Performance-related changes:**
- Will automatically trigger performance benchmarks
- Performance comparison will be posted as a PR comment
- Significant performance regressions may require discussion

## Code Standards

### Code Formatting

HashStamp uses `dotnet format` for consistent code formatting:

```bash
# Check if code follows formatting standards
dotnet format --verify-no-changes

# Automatically fix formatting issues
dotnet format
```

**All PRs must pass the formatting check.** The CI pipeline will fail if code is not properly formatted.

### Coding Guidelines

- Follow standard C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and reasonably sized
- Use appropriate access modifiers
- Handle edge cases and add appropriate error handling

### Source Generator Specific Guidelines

When working on the core source generator (`HashStampGenerator.cs`):

- Ensure the generator works with incremental compilation
- Test both compile-time and runtime hash access patterns
- Verify the generator handles various method signatures correctly
- Test with different namespace and class structures
- Ensure generated code compiles without warnings

## Testing Guidelines

### Manual Testing Scenarios

Always test these scenarios when modifying source generator logic:

1. **Compile-time access verification**:
   ```csharp
   // This should work without errors
   Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod1);
   ```

2. **Runtime access verification**:
   ```csharp
   // This should work without errors
   Console.WriteLine(HashStamps.Namespaces["HashStamp.Test"]
                                 .Classes["TestClass1"]  
                                 .Methods["TestMethod1"].Hash);
   ```

3. **Hash generation verification**:
   - Modify a method body in any test class
   - Rebuild and run the application
   - Verify the corresponding hash value changes
   - Restore the original method body

### Creating Tests

- Add new test methods to existing test classes in `src/HashStamp.Test/`
- Update `Program.cs` to exercise new functionality
- Ensure both access patterns (static and dynamic) work correctly

## CI/CD Process

Our CI/CD pipeline includes three workflows:

### Main CI Pipeline (`.github/workflows/ci.yml`)

Runs on all pushes and PRs to main/master:

1. **Code Formatting Check** - Ensures consistent style
2. **Build Validation** - Compiles in Release mode
3. **Functional Testing** - Runs the HashStamp.Test console application
4. **Unit Tests** - Runs formal unit tests (if any)
5. **Benchmark Validation** - Runs benchmarks to ensure functionality

### Performance Diff (`.github/workflows/performance-diff.yml`)

Runs automatically on PRs to compare performance:

- Compares PR performance with base branch
- Posts detailed performance analysis as PR comments
- Tracks changes in execution time and memory usage

### Release Validation (`.github/workflows/release.yml`)

Handles version management:

- Validates version increments in PRs (warning only)
- Creates GitHub releases when version tags are pushed
- Ensures release tags match project versions

## Release Process

### For Maintainers

To create a new release:

1. **Update the version** in `src/HashStamp/HashStamp.csproj`:
   ```xml
   <Version>1.1.0</Version>
   ```

2. **Commit and push** the version change:
   ```bash
   git add src/HashStamp/HashStamp.csproj
   git commit -m "Bump version to 1.1.0"
   git push origin main
   ```

3. **Create and push a version tag**:
   ```bash
   git tag v1.1.0
   git push origin v1.1.0
   ```

The release workflow will automatically:
- Validate the tag matches the project version
- Build and test the release
- Create a GitHub release with release notes

### Version Guidelines

- **Patch** (1.0.1): Bug fixes, minor improvements
- **Minor** (1.1.0): New features, backwards compatible
- **Major** (2.0.0): Breaking changes

## Reporting Issues

### Before Creating an Issue

- Search existing issues to avoid duplicates
- Ensure you're using the latest version
- Try to reproduce the issue with minimal code

### Issue Template

When creating an issue, please include:

**Bug Reports:**
- Clear description of the problem
- Steps to reproduce
- Expected vs actual behavior
- Environment details (.NET version, OS, etc.)
- Code samples that demonstrate the issue

**Feature Requests:**
- Clear description of the requested feature
- Use case and motivation
- Proposed implementation approach (if any)

**Performance Issues:**
- Benchmark results showing the performance problem
- Comparison with expected performance
- Environment and configuration details

## Community Guidelines

### Code of Conduct

- Be respectful and inclusive
- Welcome newcomers and help them learn
- Focus on constructive feedback
- Assume positive intent in communications

### Getting Help

- **Questions:** Use GitHub Discussions or create an issue
- **Bug reports:** Create a detailed issue
- **Feature requests:** Create an issue for discussion first
- **Documentation:** Contribute improvements via PRs

### Recognition

Contributors will be recognized in:
- Release notes for significant contributions
- README acknowledgments
- GitHub contributor graphs and statistics

## Development Tips

### Common Issues

- **"CS0103: The name 'HashStamps' does not exist"**: Source generator failed
  - Verify Roslyn version compatibility
  - Check analyzer reference in project files
  - Try a clean rebuild

- **Build timeouts**: Allow sufficient time for builds (30+ seconds)
- **Formatting failures**: Run `dotnet format` before committing

### Debugging Source Generators

- Use the "Debug Source Generator" build configuration
- Enable source generator debugging with conditional compilation
- Generated code is compiled but not persisted to disk

### Working with Forks

Keep your fork updated:
```bash
git fetch upstream
git checkout main
git merge upstream/main
git push origin main
```

Thank you for contributing to HashStamp! Your efforts help make this project better for everyone. 🚀