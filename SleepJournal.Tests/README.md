# Sleep Journal Test Suite Documentation

## Overview

This comprehensive test suite provides end-to-end (e2e) coverage for the Sleep Journal MAUI application, following industry best practices for .NET MAUI development, MVVM patterns, and SQLite data access.

## Test Project Structure

```
SleepJournal.Tests/
├── Models/
│   └── JournalEntryTests.cs          # Model validation tests
├── ViewModels/
│   └── MainPageViewModelTests.cs     # ViewModel business logic tests
├── Services/
│   └── SQLiteDataServiceTests.cs     # Data service integration tests
├── Integration/
│   └── EndToEndTests.cs              # Complete user journey tests
└── SleepJournal.Tests.csproj         # Test project configuration
```

## Test Categories

### 1. Model Tests (`Models/JournalEntryTests.cs`)

- **Purpose**: Validate data model integrity and constraints
- **Coverage**:
  - Default values initialization
  - Property setters and getters
  - Data type validation
  - Boundary value testing
- **Test Count**: 8 tests

### 2. ViewModel Tests (`ViewModels/MainPageViewModelTests.cs`)

- **Purpose**: Test business logic, validation, and command execution
- **Coverage**:
  - Property initialization and updates
  - Input validation (text length, numeric ranges)
  - Save command execution
  - Error handling and messaging
  - Form reset behavior
  - Command CanExecute logic
  - Cancellation token support
  - IsSaving state management
- **Test Count**: 30+ tests
- **Key Test Scenarios**:
  - Valid data saves successfully
  - Invalid data shows appropriate errors
  - Form resets after successful save
  - Concurrent save operations
  - Boundary value testing

### 3. Service Tests (`Services/SQLiteDataServiceTests.cs`)

- **Purpose**: Integration testing of data persistence layer
- **Coverage**:
  - Database initialization (async pattern)
  - CRUD operations
  - Data integrity preservation
  - Concurrent access handling
  - Error handling and logging
  - Cancellation token support
  - Special character and Unicode handling
- **Test Count**: 20+ tests
- **Test Infrastructure**:
  - Uses in-memory SQLite databases
  - IAsyncLifetime for proper setup/teardown
  - Isolated test databases for each test

### 4. End-to-End Tests (`Integration/EndToEndTests.cs`)

- **Purpose**: Validate complete user workflows
- **Coverage**:
  - Complete user journeys (create, validate, save, retrieve)
  - Multi-entry scenarios
  - Error recovery flows
  - Data persistence verification
  - Performance benchmarking
  - Concurrent operations
- **Test Count**: 15+ tests
- **Key Scenarios**:
  - User creates and saves journal entry
  - User receives validation errors
  - User corrects errors and successfully saves
  - Multiple entries saved in correct order
  - Special characters and emojis preserved
  - Performance within acceptable limits

## Testing Frameworks and Tools

### Core Frameworks

- **xUnit**: Primary testing framework
  - Industry-standard for .NET testing
  - Excellent parallel execution support
  - Clean test isolation

### Assertion Libraries

- **FluentAssertions**: Readable assertions
  - Natural language syntax
  - Detailed failure messages
  - Extensive assertion methods

### Mocking

- **Moq**: Mock framework for dependencies
  - Mock IDataService for ViewModel tests
  - Mock ILogger for tracking log calls
  - Verify method calls and arguments

### Coverage

- **coverlet.collector**: Code coverage analysis
  - Track test coverage metrics
  - Identify untested code paths

## Running the Tests

### From Command Line

```powershell
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~MainPageViewModelTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### From Visual Studio

1. Open Test Explorer (Test > Test Explorer)
2. Click "Run All" or right-click specific tests
3. View results in Test Explorer window

### From VS Code

1. Install .NET Test Explorer extension
2. Use Test Explorer sidebar
3. Click play button next to tests

## Test Best Practices Implemented

### 1. AAA Pattern (Arrange-Act-Assert)

All tests follow the Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task SaveCommand_WithValidData_ShouldCallDataService()
{
    // Arrange - Set up test data and dependencies
    _viewModel.Text = "Test entry";

    // Act - Execute the action being tested
    await _viewModel.SaveCommand.ExecuteAsync(null);

    // Assert - Verify the expected outcome
    _mockDataService.Verify(/*...*/);
}
```

### 2. Test Isolation

- Each test is independent
- No shared state between tests
- Proper setup and teardown using IAsyncLifetime

### 3. Descriptive Test Names

- Test names clearly describe what is being tested
- Format: `MethodName_Scenario_ExpectedBehavior`
- Example: `SaveCommand_WithEmptyText_ShouldSetErrorMessage`

### 4. Theory-Based Testing

- Use `[Theory]` and `[InlineData]` for parameterized tests
- Reduces code duplication
- Tests multiple scenarios efficiently

### 5. Async Testing

- Properly handles async/await patterns
- Tests cancellation token support
- Verifies async initialization

## Code Optimizations Implemented

### SQLiteDataService Enhancements

1. **Async Initialization**
   - Removed blocking `.Wait()` calls
   - Implemented async lazy initialization pattern
   - Thread-safe with SemaphoreSlim

2. **Error Handling**
   - Try-catch blocks around all operations
   - Detailed logging for debugging
   - Proper exception propagation

3. **Resource Management**
   - Implements IAsyncDisposable
   - Proper cleanup of database connections
   - Semaphore disposal

4. **Cancellation Support**
   - All async methods accept CancellationToken
   - Proper cancellation propagation

### MainPageViewModel Enhancements

1. **Enhanced Validation**
   - Comprehensive input validation
   - Clear error messages
   - Separate validation method

2. **Command Improvements**
   - CanExecute properly implemented
   - IsSaving state for UI feedback
   - NotifyCanExecuteChangedFor attribute

3. **Error Handling**
   - Try-catch for save operations
   - Specific handling for OperationCanceledException
   - User-friendly error messages

4. **Logging Integration**
   - Constructor injection of ILogger
   - Informational and error logging
   - Debugging support

### MauiProgram Enhancements

1. **Logging Configuration**
   - Different log levels for Debug/Release
   - Proper DI registration
   - Clean service registration

## Coverage Metrics

### Target Coverage

- **ViewModel Logic**: 100% (all business logic tested)
- **Service Layer**: 95%+ (covers all data operations)
- **Models**: 100% (simple data classes)
- **Integration**: 80%+ (critical user paths)

### Untested Areas (Intentional)

- Platform-specific code (Android/iOS/Windows)
- XAML UI code (would require UI testing framework)
- Third-party library code

## Continuous Integration Recommendations

### CI/CD Pipeline Steps

```yaml
steps:
  - name: Restore dependencies
    run: dotnet restore

  - name: Build solution
    run: dotnet build --no-restore

  - name: Run tests
    run: dotnet test --no-build --verbosity normal /p:CollectCoverage=true

  - name: Publish coverage
    run: reportgenerator -reports:coverage.opencover.xml -targetdir:coverage
```

### Quality Gates

- Minimum 80% code coverage
- All tests must pass
- No critical code analysis warnings

## Future Test Enhancements

### Recommended Additions

1. **UI Tests**: Add MAUI UI testing for visual validation
2. **Performance Tests**: More comprehensive load testing
3. **Accessibility Tests**: Validate screen reader support
4. **Security Tests**: Input sanitization validation
5. **Migration Tests**: Database schema migration testing

### Tools to Consider

- **Appium**: Cross-platform UI testing
- **BenchmarkDotNet**: Performance benchmarking
- **SpecFlow**: Behavior-driven development
- **Stryker.NET**: Mutation testing

## Troubleshooting

### Common Issues

#### Issue: "Database locked" errors

**Solution**: Ensure proper disposal in test cleanup

```csharp
public async Task DisposeAsync()
{
    await _dataService.DisposeAsync();
    await Task.Delay(100); // Give time for cleanup
}
```

#### Issue: Flaky tests with timing issues

**Solution**: Use proper async/await, avoid Thread.Sleep

```csharp
// Bad
Thread.Sleep(1000);

// Good
await Task.Delay(100);
```

#### Issue: Tests fail on CI but pass locally

**Solution**: Check for:

- Absolute vs relative paths
- Time zone dependencies
- Platform-specific behavior

## Contributing

### Adding New Tests

1. Follow existing naming conventions
2. Use AAA pattern
3. Add descriptive comments for complex scenarios
4. Ensure tests are isolated and repeatable
5. Update this documentation

### Test Review Checklist

- [ ] Test name is descriptive
- [ ] AAA pattern followed
- [ ] No hardcoded values (use constants)
- [ ] Proper async/await usage
- [ ] Mocks properly verified
- [ ] Assertions are specific
- [ ] Test is isolated and repeatable

## Summary

This test suite provides comprehensive coverage of the Sleep Journal application:

- **✅ Unit Tests**: Validate individual components in isolation
- **✅ Integration Tests**: Validate data persistence and retrieval
- **✅ E2E Tests**: Validate complete user workflows
- **✅ Performance Tests**: Ensure acceptable performance
- **✅ Error Handling**: Validate error scenarios and recovery
- **✅ Best Practices**: Follow .NET MAUI and testing best practices

The suite is designed to be:

- **Maintainable**: Clear structure and naming
- **Reliable**: Isolated, repeatable tests
- **Fast**: Efficient execution with parallel support
- **Comprehensive**: High coverage of critical paths
- **Professional**: Production-ready quality

Total Tests: **70+ tests** across all categories
