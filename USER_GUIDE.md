# Sleep Journal — User Guide

A cross-platform sleep journaling app. Built with .NET MAUI 9.0.

## Getting Started

1. Open the app. You land on the **Main** page (the journal form).
2. Fill in your reflection for the day:
   - **Mood** — slide from 1 (low) to 10 (high).
   - **Social Comfort** — how comfortable did you feel socially?
   - **Reflection** — how do you feel about today choices?
   - **Text** — optional free-form notes (max 200 characters).
3. Tap **Save Entry** (or **Update Entry** when editing). A drop sound plays if audio is on.

## Navigation

The app has a flyout menu (☰) with four destinations:

| Destination     | What it does                                       |
|-----------------|----------------------------------------------------|
| Home            | Journal form (create or edit an entry).           |
| History         | Browse, search, filter, and delete past entries.  |
| Statistics      | Dashboard: totals for last 7 / 30 days.           |
| Settings        | Profile, reminders, dark mode, audio, biometrics. |

## History Page

- **Search bar** — type to filter entries by text.
- **Active Filters** banner — appears when filters are applied; tap **Clear Filters** to reset.
- **Pull down** on the list to refresh.
- **Swipe left** on an entry to delete it.
- **Tap** an entry to open it in edit mode on the Main page.

## Settings Page

### Profile
- Set your display name.

### Notifications
- Toggle **Enable daily reminders**.
- Pick a reminder time (default 21:00).
- When enabled, the app requests notification permission and schedules a daily reminder.

### Appearance
- **Dark mode** toggle applies immediately.

### Audio
- **Audio feedback** toggle (drop/click sounds on button press).
- **Audio volume** slider (0.0–1.0).

### Security
- **Biometric authentication** toggle (face ID / fingerprint). Requires platform support.

## Statistics Page

Shows aggregate counts:
- **Total entries** ever recorded.
- **Last 7 days** and **Last 30 days** entry counts.

Appears empty until you have at least one saved entry.

## Data

All entries are stored locally in a SQLite database. No cloud sync yet.
Use the export feature (Settings → Export) to get a CSV of your entries.

## Accessibility

Interactive controls carry AutomationId attributes for screen-reader and
test-automation tooling. If you rely on a screen reader, labels and hints are
provided on all form fields.

## Troubleshooting

| Issue                              | Fix                                              |
|------------------------------------|--------------------------------------------------|
| No reminder notification           | Check system settings; grant notification permission. |
| Audio not playing                 | Verify Audio toggle is on in Settings.          |
| App feels slow on open             | First open builds the database; subsequent opens are fast. |
| Can not find the flyout menu       | Swipe from the left edge, or tap the hamburger icon. |

## Support

This is an open-source project. See the repository for source code and issues.
