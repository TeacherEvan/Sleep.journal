# CODEBASE-STATE — Sleep Journal (baseline 2026-09-12)

## Run metadata
- Repo: TeacherEvan/Sleep.journal (origin https://github.com/TeacherEvan/Sleep.journal.git)
- Branch: main, clean working tree, HEAD = af46463 "fix: add missing flyout menu icons"
- Host: ewaldt-N95, dotnet SDK 10.0.112; Java 21 present; no Android SDK/workload.

## Tech stack
- .NET 9 MAUI single-project (net9.0 + net9.0-android), C# 12, ImplicitUsings, Nullable enabled.
- DI via MauiProgram (singleton services, transient ViewModels, singleton pages).
- SQLite via sqlite-net-pcl; CommunityToolkit.Mvvm source generators; xunit + Moq + FluentAssertions tests.

## Relevant structure
- SleepJournal/Services/: IAudioService + AudioService (the target), INotificationService, IExportService, IBiometricService, IDataService + SQLiteDataService, CsvExportService.
- SleepJournal/ViewModels/: MainPageViewModel (calls PlayDropSoundAsync at save, line 106), SettingsPageViewModel (volume/toggle), HistoryPageViewModel, StatisticsViewModel, WelcomePageViewModel.
- SleepJournal/Platforms/: Android, iOS, MacCatalyst, Windows, Tizen — MainActivity/Program/AppDelegate stubs only; no audio player exists anywhere.
- SleepJournal.Tests/: 8 test files, 115 tests, all passing.

## Baseline gates
- `dotnet build SleepJournal/SleepJournal.csproj -f net9.0` -> 0 errors (verified this run).
- `dotnet test` -> 115 passed, 0 failed, 0 skipped (verified this run).
- `net9.0-android` build is NOT runnable here (no Android SDK) -> platform code is syntax-checked only.

## Known issues (pre-existing, not this run's scope)
- NU1603: Plugin.LocalNotification 11.1.5 not found, 12.0.0 resolved instead (warning only).
- CS0618: Application.MainPage deprecated in HistoryPageViewModel (warning only).
- XC0022/XC0025: XamlC binding-compilation warnings (warning only).
- NETSDK1202: net9.0-android workload out of support (warning only).

## Open gap this run targets
- AudioService.PlayWavAsync (AudioService.cs:155) generates bytes then no-ops:
  `_logger.LogDebug(...); await Task.CompletedTask;`. Bytes never reach a speaker.
