# Quick Start Guide - Sleep Journal Test Suite

## Prerequisites

- .NET 9.0 SDK installed
- Visual Studio 2022, VS Code, or JetBrains Rider

## Running Tests

### Option 1: Command Line (PowerShell)

```powershell
# Navigate to solution directory
cd "C:\Users\eboth\Documents\Sleep.journal"

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity detailed

# Run tests with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test class
dotnet test --filter "FullyQualifiedName~MainPageViewModelTests"

# Run tests by category
dotnet test --filter "FullyQualifiedName~Integration"
```

### Option 2: Visual Studio

1. Open `Sleep.journal.sln`
2. Build the solution (Ctrl+Shift+B)
3. Open Test Explorer (Test > Test Explorer)
4. Click "Run All Tests" or right-click specific tests

### Option 3: VS Code

1. Install ".NET Test Explorer" extension
2. Open folder in VS Code
3. Click Testing icon in sidebar
4. Run tests from Test Explorer panel

## Expected Test Results

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    70, Skipped:     0, Total:    70
```

## Verifying Code Optimizations

### Check SQLiteDataService

```powershell
# View the optimized service
type SleepJournal\Services\SQLiteDataService.cs
```

**Look for**:

- ✅ `EnsureInitializedAsync()` method (async initialization)
- ✅ `IAsyncDisposable` implementation
- ✅ Try-catch blocks with logging
- ✅ CancellationToken parameters

### Check MainPageViewModel

```powershell
# View the enhanced ViewModel
type SleepJournal\ViewModels\MainPageViewModel.cs
```

**Look for**:

- ✅ `ValidateInput()` method
- ✅ `IsSaving` property
- ✅ `CanSave` property for CanExecute
- ✅ Enhanced error messages
- ✅ ILogger injection

## Common Issues

### Issue: Tests fail with "Database locked"

**Solution**: Ensure proper test isolation. Each test should use a unique database.

### Issue: Cannot find test project

**Solution**:

```powershell
# Verify test project exists
dir SleepJournal.Tests\*.csproj

# Restore NuGet packages
dotnet restore SleepJournal.Tests\SleepJournal.Tests.csproj
```

### Issue: Build errors in test project

**Solution**:

```powershell
# Clean and rebuild
dotnet clean
dotnet build SleepJournal.Tests\SleepJournal.Tests.csproj
```

## Next Steps

1. ✅ Review [CODE_REVIEW_SUMMARY.md](CODE_REVIEW_SUMMARY.md) for detailed findings
2. ✅ Read [SleepJournal.Tests/README.md](SleepJournal.Tests/README.md) for test documentation
3. ✅ Run tests to verify all optimizations work correctly
4. 🔄 Stage and commit optimized code
5. 🔄 Set up CI/CD pipeline for automated testing

## Continuous Integration

### GitHub Actions Example

```yaml
name: .NET Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: "9.0.x"

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal /p:CollectCoverage=true
```

## Test Coverage Report

After running tests with coverage:

```powershell
# Generate HTML report (requires reportgenerator tool)
dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator `
  -reports:"SleepJournal.Tests\coverage.opencover.xml" `
  -targetdir:"coverage-report" `
  -reporttypes:Html

# Open report
start coverage-report\index.html
```

## Support

For issues or questions:

- Review test documentation in `SleepJournal.Tests/README.md`
- Check code review summary in `CODE_REVIEW_SUMMARY.md`
- Review Microsoft Learn documentation on .NET MAUI testing

---

**Last Updated**: January 17, 2026  
**Test Suite Version**: 1.0  
**Total Tests**: 70+  
**Target Coverage**: 85%+
