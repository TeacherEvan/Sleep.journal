# Audio & Notification Features Implementation

## Overview

Added local storage (already existed via SQLite), notification support, and audio feedback for button interactions.

## New Features

### 1. **Audio Feedback System**

- **Service**: `IAudioService` / `AudioService`
- **Location**: `SleepJournal/Services/`
- **Functionality**:
  - Drop sound effects for button clicks
  - Volume control (0.0 - 1.0)
  - Graceful error handling

**Usage**:

```csharp
await _audioService.PlayDropSoundAsync();  // Water drop sound
await _audioService.PlayClickSoundAsync(); // Soft click sound
_audioService.SetVolume(0.7f);             // Set volume to 70%
```

### 2. **Local Notifications**

- **Service**: `INotificationService` / `NotificationService`
- **Package**: Plugin.LocalNotification v12.0
- **Permissions**: Added to AndroidManifest.xml
  - `POST_NOTIFICATIONS`
  - `SCHEDULE_EXACT_ALARM`
  - `USE_EXACT_ALARM`

**Features**:

- Daily bedtime reminders
- Permission request flow
- Notification scheduling with custom time
- Cancel all notifications

**Usage**:

```csharp
// Request permissions
var granted = await _notificationService.RequestPermissionsAsync();

// Schedule daily reminder at 9:00 PM
await _notificationService.ScheduleBedtimeReminderAsync(
    21, 0, "Time to write in your sleep journal! 💤");

// Cancel all notifications
await _notificationService.CancelAllNotificationsAsync();
```

### 3. **Integration Points**

#### MainPageViewModel

- Added `IAudioService` injection
- Plays drop sound on "Save Entry" button click
- Audio fires immediately (non-blocking) for instant feedback

#### SettingsPageViewModel

- Added `INotificationService` injection
- Notification scheduling when "Enable Reminders" is toggled
- Permission request flow integrated
- Automatic notification cancellation when disabled

## Files Modified

### Services

- ✨ **NEW** `Services/IAudioService.cs` - Audio interface
- ✨ **NEW** `Services/AudioService.cs` - Audio implementation
- ✨ **NEW** `Services/INotificationService.cs` - Notification interface
- ✨ **NEW** `Services/NotificationService.cs` - Notification implementation

### ViewModels

- **UPDATED** `ViewModels/MainPageViewModel.cs` - Added audio feedback
- **UPDATED** `ViewModels/SettingsPageViewModel.cs` - Added notification scheduling

### Configuration

- **UPDATED** `MauiProgram.cs` - Registered new services in DI
- **UPDATED** `SleepJournal.csproj` - Added Plugin.LocalNotification package
- **UPDATED** `Platforms/Android/AndroidManifest.xml` - Added notification permissions

### Resources

- ✨ **NEW** `Resources/Raw/drop.mp3` - Water drop sound (placeholder)
- ✨ **NEW** `Resources/Raw/click.mp3` - Click sound (placeholder)

## Audio Files

**Note**: The current audio files are placeholders (0 bytes). Replace them with actual MP3 files:

1. Download or record water drop sounds
2. Replace `SleepJournal/Resources/Raw/drop.mp3`
3. Replace `SleepJournal/Resources/Raw/click.mp3`
4. Recommended duration: 0.2-0.5 seconds
5. Format: MP3, 44.1kHz, mono

## Testing

### Audio Testing

```csharp
// In MainPageViewModel
// Click "Save Entry" button -> should play drop sound
```

### Notification Testing

1. Go to Settings page
2. Enable "Reminders"
3. Set reminder time
4. Save settings
5. Check system notification settings for Sleep Journal app
6. Wait for scheduled time or use Android Debug Bridge to test

## Android Notification Channels

The app creates a notification channel:

- **Channel ID**: `sleepjournal_reminders`
- **Priority**: High
- **Icon**: Default app icon (can be customized)

## Known Limitations

1. **Audio**: Current implementation is a stub (logs only). Full audio playback requires:
   - Actual audio files (not placeholders)
   - Platform-specific implementation for better control
2. **Notifications**:
   - Android 13+ requires runtime permission
   - Users may need to grant permission in system settings
   - Exact alarms may require additional user action on some devices

## Future Enhancements

- [ ] Add platform-specific audio implementations
- [ ] Add haptic feedback alongside audio
- [ ] Add notification sound customization
- [ ] Add notification categories (reminders, achievements, etc.)
- [ ] Add audio volume setting in Settings page
- [ ] Add audio on/off toggle in Settings

## Dependencies

```xml
<PackageReference Include="Plugin.LocalNotification" Version="12.0.0" />
```

## Platform Requirements

- **Android**: API 21+ (Android 5.0 Lollipop)
- **Notifications**: API 33+ requires POST_NOTIFICATIONS permission
- **Audio**: MAUI Media APIs (built-in)
