# Sleep Journal - AI Agent Instructions

## Architecture Overview

This is a **.NET MAUI 9.0 cross-platform sleep journaling app** targeting Android (primary), with iOS/Windows/MacCatalyst support available. Uses **MVVM pattern** with CommunityToolkit.Mvvm and **SQLite** for local data persistence.

### Core Components

- **Models** (`Models/`): Data entities with SQLite attributes (`[PrimaryKey]`, `[AutoIncrement]`)
- **Services** (`Services/`): Data layer with interface-based DI (`IDataService` → `SQLiteDataService`)
- **ViewModels** (`ViewModels/`): Business logic using `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`
- **Views** (XAML): UI in `MainPage.xaml` and `AppShell.xaml`

### Critical Pattern: Async Initialization

⚠️ **Never use `.Wait()` or `.Result()` in MAUI** - causes deadlocks on UI thread.

Instead, use lazy async initialization pattern:

```csharp
// SQLiteDataService uses SemaphoreSlim for thread-safe lazy init
private async Task EnsureInitializedAsync(CancellationToken ct = default)
{
    if (_isInitialized) return;
    await _initializationLock.WaitAsync(ct);
    try {
        await _db.CreateTableAsync<JournalEntry>();
        _isInitialized = true;
    } finally {
        _initializationLock.Release();
    }
}
```

### Dependency Injection

Register in `MauiProgram.CreateMauiApp()`:

- Services: `AddSingleton<IDataService, SQLiteDataService>()`
- ViewModels: `AddTransient<MainPageViewModel>()` (recreated per navigation)
- Always inject `ILogger<T>` for debugging/monitoring

## Key Conventions

### ViewModel Development

Use **CommunityToolkit.Mvvm source generators** - no manual INotifyPropertyChanged:

```csharp
[ObservableProperty]  // Generates Text property + notifications
private string text = "";

[RelayCommand(CanExecute = nameof(CanSave))]  // Generates SaveCommand
private async Task SaveAsync(CancellationToken ct) { }

private bool CanSave => !string.IsNullOrWhiteSpace(Text);
```

**CanExecute pattern**: Use `[NotifyCanExecuteChangedFor(nameof(SaveCommand))]` on properties that affect command availability.

### Validation Pattern

- Validate in ViewModel before calling service
- Set `ErrorMessage` property (bound to UI)
- Use `IsSaving` flag to disable UI during async operations
- Always trim user input: `Text.Trim()`

### Service Layer Pattern

- Interfaces in `Services/IDataService.cs`
- All async methods accept `CancellationToken cancellationToken = default`
- Implement `IAsyncDisposable` for cleanup (database connections)
- Wrap all operations in try-catch with `ILogger` calls
- `ArgumentNullException.ThrowIfNull(entry)` for parameter validation

### Database

- Path: `FileSystem.AppDataDirectory + "sleepjournal.db"`
- Tables auto-created on first access via `EnsureInitializedAsync()`
- Models: `JournalEntry`, `UserSettings`, `Passage`
- Query pattern: `await _db.Table<T>().OrderByDescending(...).ToListAsync()`

## Development Workflows

### Build & Run

```powershell
dotnet build SleepJournal/SleepJournal.csproj
dotnet build -t:Run -f net9.0-android  # Android emulator
```

### Testing (70+ tests)

```powershell
dotnet test                           # All tests
dotnet test --filter "FullyQualifiedName~ViewModel"  # Specific category
```

**Test structure** (see `SleepJournal.Tests/README.md`):

- **Unit tests**: Mock `IDataService` with Moq, test ViewModels in isolation
- **Integration tests**: Use in-memory SQLite (`GetTempDbPath()`)
- **E2E tests**: Real service + ViewModel, test complete workflows
- **Helpers**: `TestHelpers.cs` has builders (`JournalEntryBuilder`) and custom assertions

### Adding New Features

1. **Model**: Add to `Models/` with SQLite attributes, update `SQLiteDataService.EnsureInitializedAsync()`
2. **Service**: Extend `IDataService`, implement in `SQLiteDataService` with error handling + logging
3. **ViewModel**: Inherit `ObservableObject`, use source generators, inject dependencies
4. **Tests**: Follow AAA pattern, use `IAsyncLifetime` for async setup/teardown

## Project-Specific Details

### Platform Targeting

Currently `<TargetFrameworks>net9.0-android</TargetFrameworks>` only. To add iOS/Windows:

1. Uncomment in `.csproj`
2. Test platform-specific code in `Platforms/{Android|iOS|Windows}/`

### Logging

Debug builds: `LogLevel.Debug` with console output  
Release builds: `LogLevel.Information`  
Pattern: `_logger.LogInformation("Action completed. Field: {Value}", value)`

### Data Validation

- Text: 1-200 characters (validated in ViewModel)
- Mood/SocialAnxiety/Regretability: 1-10 range
- Always validate before database operations

### Common Pitfalls

❌ Don't call services directly from XAML code-behind  
❌ Don't use blocking calls (`.Wait()`, `.Result`, `.GetAwaiter().GetResult()`)  
❌ Don't register ViewModels as Singleton if they hold form state  
✅ Do use `CancellationToken` in all async methods  
✅ Do dispose `IAsyncDisposable` services properly  
✅ Do test with special characters/emojis (app supports Unicode)

## Key Files Reference

- **DI Setup**: `SleepJournal/MauiProgram.cs`
- **Async Pattern**: `Services/SQLiteDataService.cs` (EnsureInitializedAsync)
- **MVVM Example**: `ViewModels/MainPageViewModel.cs`
- **Test Examples**: `SleepJournal.Tests/ViewModels/MainPageViewModelTests.cs`
- **Architecture Docs**: `CODE_REVIEW_SUMMARY.md` (detailed optimizations)
- **Test Guide**: `SleepJournal.Tests/README.md` (comprehensive test patterns)

## Quick Commands

```powershell
# Clean build
dotnet clean && dotnet build

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Check git status (staged changes)
git status

# Stage optimized files
git add SleepJournal/Services/* SleepJournal/ViewModels/*
```
