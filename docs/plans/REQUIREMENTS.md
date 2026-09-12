# REQUIREMENTS — Audio Playback Wiring (Sleep Journal)

**Date:** 2026-09-12
**Request context:** `IMPLEMENTATION_STATUS.md` Phase 4 records AudioService as
COMPLETE with a `[x]` tick, but the note self-corrects: "Real PCM/WAV synthesis
+ 10 unit tests added ... Platform audio player routing still TODO (see
`PlayWavAsync` in `AudioService.cs`); bytes are produced but not yet routed to
speakers." That is the open work. This run wires the bytes to a speaker.

## Functional Requirements

| ID | Requirement | Source |
|----|-------------|--------|
| FR-001 | `AudioService.PlayWavAsync` must route generated WAV bytes to a runnable platform audio player instead of logging and returning. | IMPLEMENTATION_STATUS.md Phase 4 note |
| FR-002 | Playback must be non-blocking and fire-and-forget-safe: a dropped sound must never throw out of the ViewModel save path. | MainPageViewModel.cs:106 |
| FR-003 | The service must degrade gracefully when no audio hardware/player is available (log, do not throw). | Cross-platform MAUI contract |
| FR-004 | Volume (0.0-1.0) persisted in UserSettings must drive actual playback amplitude, not only the synthesis peak. | IMPLEMENTATION_STATUS.md Phase 4 |
| FR-005 | Audio on/off toggle in Settings must mute real playback, not just skip synthesis. | SettingsPageViewModel.cs |

## Non-Functional Requirements

| ID | Requirement |
|----|-------------|
| NFR-001 | No new dependencies beyond what SleepJournal.csproj already references. |
| NFR-002 | net9.0 build must stay green (0 errors). net9.0-android is unbuildable on this host (no Android SDK); platform code must compile under Android target syntax without requiring the SDK. |
| NFR-003 | Existing 115 tests must remain green; new tests must be real (assert on observable behaviour, not tautologies). |
| NFR-004 | No secrets, no credentials, no network calls. |

## Constraints

- C# 12 / .NET 9. MAUI single-project.
- Host has dotnet 10 SDK; Android workload absent. Android-target code is compile-checked by syntax only, never executed here.
- Do not modify TeacherEvan/TeacherEvan profile repo.
- Do not open issues/PRs/merges; push only if PUSH=1.

## Assumptions

- A1: Xamarin.Essentials is available transitively via Microsoft.Maui.Controls on Android/iOS/Windows/MacCatalyst. If not, fall back to platform-specific AudioTrack/AVAudioPlayer/SoundPlayer/NSSound calls guarded by #if ANDROID etc.
- A2: The host can build net9.0; it cannot build net9.0-android, so Android code is validated by syntax + dotnet build -f net9.0 only.

## Acceptance Criteria

| ID | Criterion | Verification |
|----|-----------|--------------|
| AC-001 | dotnet build -f net9.0 exits 0 with 0 errors. | dotnet build |
| AC-002 | dotnet test exits 0, >=125 tests, no regressions from the 115 baseline. | dotnet test |
| AC-003 | A new test proves PlayWavAsync no longer no-ops: it invokes a real playback path (mocked) and does not swallow the exception when the player throws. | New AudioServiceTests facts |
| AC-004 | AudioService constructor accepts an optional IAudioPlayer (default: platform player); tests inject a mock player and assert it is called with the generated WAV bytes. | New test + interface |
| AC-005 | Muted state (SetAudioEnabled(false)) causes the player mock to receive zero calls. | New test |
| AC-006 | When the player throws, PlayDropSoundAsync/PlayClickSoundAsync log and return normally (no exception propagates). | New test |
