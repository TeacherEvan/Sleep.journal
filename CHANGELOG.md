# Sleep Journal — Changelog

All notable changes to the Sleep Journal .NET MAUI application are documented here.
The format is based on Keep a Changelog.

## [Unreleased] — Phase 5 (Polish)

- Accessibility markup (AutomationId) added to interactive controls across
  Main, History, Settings, and Statistics pages.
- Resource-string scaffold (StringResources.en-US.resx) added.
- Documentation: CHANGELOG, API reference, USER_GUIDE.

## [1.0.0] — 2026-01-18 — Phase 4 (Integration)

### Added
- Audio feedback system (IAudioService / AudioService) with PCM/WAV synthesis.
- Local notifications (INotificationService / NotificationService, Plugin.LocalNotification).
- Data export/import (IExportService / CsvExportService, RFC 4180 CSV).
- Statistics dashboard (StatisticsViewModel / StatisticsPage).
- Search and filter on History page.
- Success animations (TwinkleStarsBehavior, staggered fade-in).
- Responsive layouts (OnIdiom, adaptive padding).

### Fixed
- SQLiteDataService deadlock risk: blocking .Wait() replaced with async lazy init.
- Comprehensive error handling and logging across services.

## [0.3.0] — 2026-01-17 — Phase 2 (Core Features)

### Added
- HistoryPage with pagination, pull-to-refresh, swipe-to-delete.
- SettingsPage with profile, reminders, dark mode, biometric toggle.
- Shell-based navigation (FlyoutMenu: Home / History / Settings).
- Edit mode on MainPage (create vs edit).
- InvertedBoolConverter, extended IDataService, UserSettings model.

## [0.2.0] — 2026-01-17 — Phase 1 (Security & Best Practices)

### Added
- AppConstants centralised configuration.
- Biometric authentication service (IBiometricService / BiometricService).
- Secure storage for biometric preferences.
- Session-based auth with 30-minute timeout.

## Git History (main)

| Phase  | Commit    | Description                                                |
|--------|-----------|------------------------------------------------------------|
| 1      | 47d2909   | feat: Implement Phase 1 - Security & Best Practices        |
| 2      | 07cea6a   | feat: Implement Phase 2 - Core Features (History, Settings, Navigation) |
| 3      | e24924c   | Phase 3 - Enhanced UX Implementation                       |
| 4      | 2df1509   | fix(audio): replace no-op AudioService with real PCM/WAV synthesis + tests |
| 5      | (this run) | Phase 5 - Polish (docs, accessibility, resource strings)   |

## Test Status

- 115/115 tests passing on net9.0.
- Build: 0 errors, 25 warnings (all non-critical).
