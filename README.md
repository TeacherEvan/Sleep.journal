# Sleep Journal

A cross-platform mobile application for tracking daily sleep-related journal entries with mood, social anxiety, and regretability metrics. Built with .NET MAUI 9.0 following MVVM architecture and best practices.

## Features

- 📝 **Journal Entries**: Record daily thoughts and reflections (up to 200 characters)
- 😊 **Mood Tracking**: Rate your mood on a scale of 1-10
- 🤝 **Social Anxiety Monitoring**: Track social anxiety levels (1-10)
- 😔 **Regretability Assessment**: Measure regret levels (1-10)
- 💾 **Local Storage**: SQLite database for offline-first data persistence
- 🎯 **Input Validation**: Real-time validation with clear error messages
- 📱 **Cross-Platform**: Primary target Android, with iOS/Windows/Mac support available

## Architecture

### MVVM Pattern

- **Models** (`Models/`): Data entities with SQLite attributes
- **ViewModels** (`ViewModels/`): Business logic using CommunityToolkit.Mvvm
- **Views** (XAML): UI components with data binding
- **Services** (`Services/`): Data access layer with dependency injection

### Technology Stack

- **.NET MAUI 9.0**: Cross-platform UI framework
- **SQLite**: Local database via sqlite-net-pcl
- **CommunityToolkit.Mvvm**: MVVM helpers and source generators
- **xUnit, FluentAssertions, Moq**: Testing framework (70+ tests)

### Key Design Patterns

- **Async Initialization**: Thread-safe lazy loading for SQLite (no blocking `.Wait()`)
- **Dependency Injection**: Constructor injection for services and ViewModels
- **Repository Pattern**: Interface-based data access (`IDataService`)
- **Error Handling**: Comprehensive try-catch with structured logging

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 17.8+ or VS Code with C# Dev Kit
- Android SDK (for Android development)
- Optional: Xcode (for iOS), Windows SDK (for Windows)

### Installation

1. **Clone the repository**

   ```powershell
   git clone https://github.com/TeacherEvan/Sleep.journal.git
   cd Sleep.journal
   ```

2. **Restore dependencies**

   ```powershell
   dotnet restore
   ```

3. **Build the solution**

   ```powershell
   dotnet build
   ```

4. **Run on Android emulator**
   ```powershell
   dotnet build -t:Run -f net9.0-android
   ```

## Development

### Project Structure

```
Sleep.journal/
├── SleepJournal/                  # Main application
│   ├── Models/                    # Data models
│   │   ├── JournalEntry.cs
│   │   ├── Passage.cs
│   │   └── UserSettings.cs
│   ├── Services/                  # Data access layer
│   │   ├── IDataService.cs
│   │   └── SQLiteDataService.cs
│   ├── ViewModels/                # Business logic
│   │   └── MainPageViewModel.cs
│   ├── Views/                     # XAML UI
│   │   └── MainPage.xaml
│   ├── Platforms/                 # Platform-specific code
│   │   ├── Android/
│   │   ├── iOS/
│   │   ├── Windows/
│   │   └── MacCatalyst/
│   └── MauiProgram.cs            # DI configuration
├── SleepJournal.Tests/           # Test suite
│   ├── Models/                    # Model tests
│   ├── ViewModels/                # ViewModel tests
│   ├── Services/                  # Service tests
│   ├── Integration/               # E2E tests
│   └── Helpers/                   # Test utilities
└── .github/
    └── copilot-instructions.md   # AI agent guidelines
```

### Running Tests

```powershell
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run with code coverage
dotnet test /p:CollectCoverage=true

# Run specific test category
dotnet test --filter "FullyQualifiedName~ViewModel"
```

**Test Coverage**: 70+ tests across:

- Unit tests (ViewModels, Models)
- Integration tests (SQLite data access)
- End-to-end tests (complete user workflows)

See [SleepJournal.Tests/README.md](SleepJournal.Tests/README.md) for detailed test documentation.

### Key Conventions

#### ViewModels

Use CommunityToolkit.Mvvm source generators:

```csharp
[ObservableProperty]
private string text = "";

[RelayCommand(CanExecute = nameof(CanSave))]
private async Task SaveAsync(CancellationToken ct) { }
```

#### Service Layer

All async methods accept `CancellationToken`:

```csharp
public async Task SaveEntryAsync(JournalEntry entry,
    CancellationToken cancellationToken = default)
```

#### Async Initialization

**Never use `.Wait()` or `.Result()`** - causes MAUI deadlocks:

```csharp
// ❌ Bad - blocks UI thread
_db.CreateTableAsync().Wait();

// ✅ Good - lazy async init
private async Task EnsureInitializedAsync(CancellationToken ct = default)
{
    if (_isInitialized) return;
    await _initializationLock.WaitAsync(ct);
    // ... initialize
}
```

## Code Quality

### Recent Optimizations

- ✅ Fixed blocking `.Wait()` calls with async lazy initialization
- ✅ Implemented comprehensive error handling and logging
- ✅ Enhanced input validation with user-friendly messages
- ✅ Added `IAsyncDisposable` for proper resource cleanup
- ✅ Integrated `CancellationToken` support throughout

See [CODE_REVIEW_SUMMARY.md](CODE_REVIEW_SUMMARY.md) for detailed optimization report.

### Best Practices

- Follow MVVM pattern strictly
- Use dependency injection for all services
- Implement proper async/await patterns
- Write tests for all business logic
- Log important operations and errors
- Validate user input in ViewModels

## Platform Support

### Currently Supported

- ✅ **Android** (API 21+)

### Available (Uncomment in .csproj)

- 🔄 **iOS** (15.0+)
- 🔄 **Windows** (10.0.17763.0+)
- 🔄 **macOS** (via Mac Catalyst 15.0+)

## Database

- **Path**: `FileSystem.AppDataDirectory/sleepjournal.db`
- **Engine**: SQLite via sqlite-net-pcl
- **Tables**: JournalEntry, UserSettings, Passage
- **Initialization**: Lazy async pattern on first access

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Write tests for your changes
4. Ensure all tests pass (`dotnet test`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Development Guidelines

- Follow existing code patterns and conventions
- Add tests for new features
- Update documentation as needed
- Use meaningful commit messages
- Keep PRs focused and atomic

## Documentation

- **[.github/copilot-instructions.md](.github/copilot-instructions.md)**: AI agent development guide
- **[CODE_REVIEW_SUMMARY.md](CODE_REVIEW_SUMMARY.md)**: Optimization and review report
- **[SleepJournal.Tests/README.md](SleepJournal.Tests/README.md)**: Test suite documentation
- **[QUICK_START.md](QUICK_START.md)**: Quick start guide for testing

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with [.NET MAUI](https://dotnet.microsoft.com/apps/maui)
- MVVM helpers from [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- Database via [sqlite-net-pcl](https://github.com/praeclarum/sqlite-net)
- Testing with [xUnit](https://xunit.net/), [FluentAssertions](https://fluentassertions.com/), [Moq](https://github.com/moq/moq4)

## Contact

- **Repository**: [TeacherEvan/Sleep.journal](https://github.com/TeacherEvan/Sleep.journal)
- **Issues**: [GitHub Issues](https://github.com/TeacherEvan/Sleep.journal/issues)

---

**Version**: 1.0  
**Last Updated**: January 17, 2026  
**Status**: ✅ Production Ready
