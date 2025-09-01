# HashStamp Enhancement Plan

This document outlines the strategic roadmap for enhancing HashStamp, a .NET Roslyn incremental source generator for method body hashing. The plan is organized by priority phases to ensure systematic development while maintaining backward compatibility.

## Current State Analysis

HashStamp currently provides:
- SHA-256 hash generation for method bodies at compile time
- Two access patterns: static properties and dynamic lookup
- Support for method overloading with qualified names
- Multi-namespace support
- Incremental compilation optimization

**Active Development**: Issue #1 (Roslyn APIs instead of strings) is already being addressed in PR #2.

## Enhancement Phases

### Phase 1: Foundation & Core Improvements (High Priority)

These enhancements strengthen the core functionality and developer experience.

#### 1.1 Enhanced Language Construct Support
**Goal**: Expand beyond methods to support more C# language constructs.

**Features**:
- **Properties**: Hash getter/setter bodies separately
  ```csharp
  // Generated access
  HashStamps.MyClass.MyProperty_get
  HashStamps.MyClass.MyProperty_set
  ```
- **Constructors**: Include constructor body hashing
- **Events**: Hash event accessor bodies
- **Local Functions**: Support hashing of local functions within methods
- **Expression-bodied members**: Better support for `=>` syntax

**Implementation Strategy**:
- Extend `CreateSyntaxProvider` to handle additional syntax node types
- Add new `ISyntaxReceiver` implementations for each construct type
- Update hash info data structures to accommodate new member types

#### 1.2 Comprehensive Testing Framework
**Goal**: Establish robust testing infrastructure for the source generator.

**Features**:
- **Unit Tests**: Test individual generator components
- **Integration Tests**: Test full generation scenarios
- **Regression Tests**: Ensure backward compatibility
- **Performance Tests**: Validate incremental compilation performance

**Implementation Strategy**:
- Create `src/HashStamp.Tests` project
- Use `Microsoft.CodeAnalysis.Testing` framework
- Set up test scenarios for different language constructs
- Add continuous integration validation

#### 1.3 Configuration via MSBuild Properties
**Goal**: Allow project-level configuration without code changes.

**Features**:
- **Hash Algorithm Selection**:
  ```xml
  <PropertyGroup>
    <HashStampAlgorithm>SHA256</HashStampAlgorithm> <!-- Default -->
    <HashStampAlgorithm>SHA512</HashStampAlgorithm>
    <HashStampAlgorithm>MD5</HashStampAlgorithm>
  </PropertyGroup>
  ```
- **Output Customization**:
  ```xml
  <PropertyGroup>
    <HashStampClassName>MyHashes</HashStampClassName>
    <HashStampNamespace>MyProject.Hashes</HashStampNamespace>
  </PropertyGroup>
  ```
- **Include/Exclude Patterns**:
  ```xml
  <PropertyGroup>
    <HashStampIncludeNamespaces>MyProject.Core;MyProject.Services</HashStampIncludeNamespaces>
    <HashStampExcludeNamespaces>MyProject.Tests</HashStampExcludeNamespaces>
  </PropertyGroup>
  ```

**Implementation Strategy**:
- Add `AnalyzerConfigOptionsProvider` support
- Parse MSBuild properties in generator initialization
- Create configuration classes for different options

#### 1.4 NuGet Package Production
**Goal**: Create a professional NuGet package for easy distribution.

**Features**:
- **Package Metadata**: Proper description, tags, icon
- **Multi-targeting**: Support .NET Standard 2.0, .NET 6+, .NET 8+
- **Semantic Versioning**: Proper version management
- **Release Documentation**: Release notes and upgrade guides

**Implementation Strategy**:
- Update `.csproj` with package properties
- Set up GitHub Actions for automated publishing
- Create package icon and documentation assets

### Phase 2: User Experience & Advanced Features (Medium Priority)

These enhancements improve usability and add powerful new capabilities.

#### 2.1 Attribute-Based Control
**Goal**: Fine-grained control over hash generation using attributes.

**Features**:
- **Inclusion Control**:
  ```csharp
  [HashStamp] // Force inclusion even if globally excluded
  public void MyMethod() { }
  
  [NoHashStamp] // Exclude specific method
  public void SkipThisMethod() { }
  ```
- **Custom Hash Names**:
  ```csharp
  [HashStamp("CustomMethodName")]
  public void VeryLongMethodName() { }
  ```
- **Hash Algorithm Override**:
  ```csharp
  [HashStamp(Algorithm = HashAlgorithm.SHA512)]
  public void SecureMethod() { }
  ```

**Implementation Strategy**:
- Create attribute classes in a separate assembly
- Update syntax provider to check for attributes
- Implement attribute parsing in the generator

#### 2.2 Change Detection & Comparison Utilities
**Goal**: Provide utilities to detect and analyze changes between builds.

**Features**:
- **Change Detection**:
  ```csharp
  var changes = HashStamps.CompareWith(previousBuildHashes);
  var changedMethods = changes.ChangedMethods;
  var newMethods = changes.NewMethods;
  var removedMethods = changes.RemovedMethods;
  ```
- **Hash History**: Optional storage of hash history across builds
- **Diff Reports**: Generate reports showing what changed

**Implementation Strategy**:
- Add comparison APIs to generated `HashStamps` class
- Create optional hash persistence mechanism
- Implement change detection algorithms

#### 2.3 Enhanced Diagnostics & Error Handling
**Goal**: Provide better debugging and error reporting experience.

**Features**:
- **Detailed Error Messages**: Clear explanations when generation fails
- **Diagnostic Information**: Optional verbose output about generation process
- **Hash Collision Detection**: Warn about potential hash collisions
- **Performance Metrics**: Optional timing information

**Implementation Strategy**:
- Use `SourceProductionContext.ReportDiagnostic` for warnings/errors
- Add configurable verbosity levels
- Implement hash collision detection algorithms

#### 2.4 Multiple Hash Algorithms Support
**Goal**: Support different cryptographic hash algorithms based on needs.

**Features**:
- **Algorithm Options**: MD5, SHA1, SHA256 (default), SHA512
- **Custom Algorithms**: Extensibility for custom hash implementations
- **Performance vs Security**: Choose appropriate algorithm for use case

**Implementation Strategy**:
- Create `IHashAlgorithmProvider` interface
- Implement providers for different algorithms
- Add algorithm selection configuration

### Phase 3: Integration & Advanced Tooling (Lower Priority)

These enhancements provide advanced integration capabilities and tooling.

#### 3.1 Testing Framework Integration
**Goal**: Seamless integration with testing frameworks for verification.

**Features**:
- **Test Fixtures**: Generate test data based on method hashes
- **Change Verification**: Automated tests to verify expected/unexpected changes
- **Snapshot Testing**: Compare hashes against golden files

**Implementation Strategy**:
- Create test utility packages
- Integrate with popular testing frameworks (xUnit, NUnit, MSTest)
- Provide MSBuild targets for test scenarios

#### 3.2 Visual Studio Integration
**Goal**: Enhanced IDE experience for developers.

**Features**:
- **IntelliSense**: Better autocomplete for hash access
- **Code Lens**: Show hash values in editor
- **Quick Actions**: Generate hash comparison code

**Implementation Strategy**:
- Create Visual Studio extension
- Implement language service features
- Add custom debugging visualizers

#### 3.3 Output Format Extensions
**Goal**: Support multiple output formats for different use cases.

**Features**:
- **JSON Export**: Export hashes as JSON for external tools
- **XML Export**: XML format for documentation systems
- **Database Integration**: Direct database storage options
- **Custom Templates**: User-defined output formats

**Implementation Strategy**:
- Create pluggable output system
- Implement standard format exporters
- Add template engine for custom formats

#### 3.4 Performance Optimizations
**Goal**: Optimize for large-scale enterprise codebases.

**Features**:
- **Parallel Processing**: Multi-threaded hash calculation
- **Memory Optimization**: Reduce memory footprint
- **Caching Improvements**: Better incremental compilation
- **Lazy Loading**: On-demand hash calculation

**Implementation Strategy**:
- Profile performance on large codebases
- Implement parallel processing with `System.Threading.Tasks`
- Optimize memory usage patterns
- Add configurable parallelism options

## Implementation Roadmap

### Milestone 1: Foundation (Months 1-2)
- Complete Issue #1 (Roslyn APIs) 
- Implement comprehensive testing framework
- Add basic MSBuild configuration
- Prepare NuGet package infrastructure

### Milestone 2: Core Features (Months 3-4)
- Enhanced language construct support
- Attribute-based control system
- Multiple hash algorithm support
- Improved error handling and diagnostics

### Milestone 3: Advanced Features (Months 5-6)
- Change detection utilities
- Output format extensions
- Performance optimizations
- Visual Studio integration basics

### Milestone 4: Polish & Integration (Months 7-8)
- Testing framework integration
- Advanced tooling features
- Documentation and samples
- Community feedback integration

## Success Metrics

### Technical Metrics
- **Build Performance**: Generator should not add >10% to build time
- **Memory Usage**: <50MB memory increase during generation
- **Test Coverage**: >90% code coverage for generator logic
- **Compatibility**: Support for .NET Standard 2.0 to .NET 8+

### User Experience Metrics
- **Adoption**: NuGet download count and community usage
- **Documentation**: Comprehensive API docs and samples
- **Issue Resolution**: <48 hour response time for issues
- **Breaking Changes**: Minimize breaking changes with clear migration guides

## Backward Compatibility Strategy

All enhancements must maintain backward compatibility:

1. **API Compatibility**: Existing `HashStamps` class structure unchanged
2. **Default Behavior**: New features opt-in by default
3. **Migration Guides**: Clear upgrade paths for major changes
4. **Deprecation Policy**: 2-version deprecation cycle for removed features

## Community & Contribution Guidelines

### Open Source Development
- **GitHub Issues**: Use issues for feature discussion and bug reports
- **Pull Requests**: Welcome community contributions following guidelines
- **Documentation**: Maintain comprehensive docs for contributors
- **Code Reviews**: All changes reviewed for quality and compatibility

### Release Management
- **Semantic Versioning**: Follow semver for all releases
- **Release Notes**: Detailed changelog for each version
- **Beta Releases**: Pre-release versions for community testing
- **Long Term Support**: LTS versions for enterprise users

## Conclusion

This enhancement plan transforms HashStamp from a basic method hashing tool into a comprehensive code analysis and integrity platform. The phased approach ensures steady progress while maintaining stability and backward compatibility.

The plan prioritizes foundational improvements first, then builds toward advanced features that provide significant value for different user scenarios from individual developers to large enterprise teams.

Success depends on community engagement, thorough testing, and careful attention to performance and usability throughout the implementation process.