# Sleep Journal — API Reference

Public surface of the .NET MAUI application. All types live in namespace
`SleepJournal` (sub-namespaces noted).

## Models

### `SleepJournal.Models.JournalEntry`
SQLite-backed journal entry.

| Member      | Type        | Description                                  |
|-------------|-------------|----------------------------------------------|
| `Id`        | `int`       | Primary key (auto-increment).                |
| `Text`      | `string`    | Entry text, max 200 chars.                   |
| `Mood`      | `int`       | Mood rating 1-10.                            |
| `SocialAnxiety` | `int`    | Social comfort 1-10.                         |
| `Regretability` | `int`     | Regret level 1-10.                           |
| `CreatedAt` | `DateTime`  | Entry timestamp.                             |
| `EntryDate` | `DateTime`  | Computed: `CreatedAt.Date`.                  |

### `SleepJournal.Models.UserSettings`
Persisted user preferences.

| Member              | Type        | Description                              |
|---------------------|-------------|------------------------------------------|
| `UserName`          | `string?`   | Display name.                            |
| `EnableReminders`   | `bool`      | Daily bedtime reminder toggle.           |
| `ReminderTime`      | `TimeSpan`  | Reminder time (default 21:00).           |
| `IsAudioEnabled`    | `bool`      | Audio feedback toggle.                   |
| `AudioVolume`       | `float`     | Audio volume 0.0-1.0.                    |
| `IsBiometricEnabled`| `bool`      | Biometric auth toggle.                   |

### `SleepJournal.Models.Passage`
Predefined reflection prompt.

| Member  | Type     | Description              |
|---------|----------|--------------------------|
| `Id`    | `int`    | Primary key.             |
| `Text`  | `string` | Prompt text.             |
| `Mood`  | `int`    | Associated mood tag.     |

## Services

### `SleepJournal.Services.IDataService`
Data access contract.

```csharp
Task<List<JournalEntry>> GetEntriesAsync();
Task<JournalEntry?> GetJournalEntryByIdAsync(int id);
Task SaveEntryAsync(JournalEntry entry);
Task DeleteJournalEntryAsync(int id);
Task<List<JournalEntry>> GetJournalEntriesAsync();      // alias
Task<UserSettings> GetUserSettingsAsync();
Task SaveUserSettingsAsync(UserSettings settings);
```

### `SleepJournal.Services.SQLiteDataService`
Default `IDataService` implementation (SQLite via sqlite-net-pcl).
Thread-safe async lazy initialisation; `ILogger` injected.

### `SleepJournal.Services.IAudioService`
Audio feedback contract.

```csharp
Task PlayDropSoundAsync(CancellationToken ct = default);
Task PlayClickSoundAsync(CancellationToken ct = default);
float GetVolume();
void SetVolume(float volume);        // 0.0-1.0, throws ArgumentOutOfRangeException
bool IsAudioEnabled { get; }
void SetAudioEnabled(bool enabled);
byte[] GenerateWav(float frequencyHz, int durationMs, float volume);
```

### `SleepJournal.Services.AudioService`
PCM/WAV sine-wave synthesis (pure managed, deterministic, unit-testable).
`PlayWavAsync` currently logs the bytes; platform player routing is a documented TODO.

### `SleepJournal.Services.INotificationService`
Local push notifications.

```csharp
Task<bool> RequestPermissionsAsync();
Task ScheduleBedtimeReminderAsync(int hour, int minute, string message);
Task CancelAllNotificationsAsync();
```

### `SleepJournal.Services.NotificationService`
Wraps `Plugin.LocalNotification` v12. Channel ID: `sleepjournal_reminders`.

### `SleepJournal.Services.IExportService`
Data export contract.

```csharp
Task<string> ExportEntriesAsync(List<JournalEntry> entries);
```

### `SleepJournal.Services.CsvExportService`
RFC 4180 CSV export.

### `SleepJournal.Services.IBiometricService`
Biometric authentication.

```csharp
Task<bool> AuthenticateAsync(string reason);
Task<bool> IsAvailableAsync();
Task SaveBiometricPreferenceAsync(bool enabled);
Task<bool> GetBiometricPreferenceAsync();
```

### `SleepJournal.Services.BiometricService`
Uses `SecureStorage`; platform detection for Android/iOS/MacCatalyst; graceful degradation on unsupported platforms.

## ViewModels

All inherit from `CommunityToolkit.Mvvm.ObservableObject`.

### `MainPageViewModel`
- `Text`, `Mood`, `SocialAnxiety`, `Regretability` (bindable).
- `SaveCommand` (IAsyncRelayCommand) — validates input, saves via `IDataService`, plays drop sound.
- `PageTitle`, `SaveButtonText` — context-aware (create vs edit).

### `HistoryPageViewModel`
- `Entries`, `SearchText`, `IsFiltered`, `IsRefreshing`.
- `SearchCommand`, `ClearFiltersCommand`, `RefreshEntriesCommand`, `LoadMoreEntriesCommand`.
- `DeleteEntryCommand`, `EditEntryCommand` (swipe/tap on entry).

### `SettingsPageViewModel`
- `UserName`, `EnableReminders`, `ReminderTime`, `IsBiometricEnabled`, `IsAudioEnabled`, `AudioVolume`.
- `SaveCommand` — persists settings, schedules/cancels notifications.

### `StatisticsViewModel`
- `TotalEntries`, `EntriesLast7Days`, `EntriesLast30Days`, `HasData`, `IsLoading`, `NoData`, `HasError`.

### `WelcomePageViewModel`
- Welcome/onboarding state.

## Constants

### `SleepJournal.AppConstants`
Centralised settings: database filename, pagination (`PageSize=20`, threshold=5),
validation (`MaxTextLength=200`, rating 1-10), security (30-min session timeout),
defaults (`ReminderTime=21:00`).

## DI Registration (`MauiProgram.cs`)

Singleton: `SQLiteDataService`, `AudioService`, `NotificationService`,
`CsvExportService`, `BiometricService`.
Scoped/transient: ViewModels (resolved via `Shell` navigation).
