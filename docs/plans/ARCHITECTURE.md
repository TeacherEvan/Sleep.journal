# ARCHITECTURE — Audio Playback Wiring

## Current state
- `AudioService` (single class, no platform split) owns synthesis (`GenerateWav`)
  and a no-op `PlayWavAsync`. DI registers it as singleton `IAudioService`.
- `MainPageViewModel` fires `PlayDropSoundAsync` on save; `SettingsPageViewModel`
  exposes volume + enabled toggle. Both depend only on `IAudioService`.
- No `IAudioPlayer` abstraction exists; no platform audio code exists under
  `Platforms/`.

## Target state
Introduce a thin `IAudioPlayer` interface and a default implementation that
routes WAV bytes to a real player. Keep `AudioService` as the orchestrator:
it synthesises, checks mute/volume, then hands the bytes to the injected player.

```
IAudioPlayer (interface)                <- new, test seam
  ├── PlatformAudioPlayer (default)     <- new, #if-guarded per-platform impl
  └── Mock<IAudioPlayer> (tests)        <- new tests

AudioService : IAudioService            <- edited
  ctor(ILogger, IAudioPlayer? player = null)
  PlayWavAsync -> _player.PlayAsync(wav, volume, ct)
```

## Areas being edited
| Area | Change |
|------|--------|
| SleepJournal/Services/IAudioPlayer.cs | NEW interface: `Task PlayAsync(byte[] wav, float volume, CancellationToken ct)` |
| SleepJournal/Services/PlatformAudioPlayer.cs | NEW default impl; per-platform `#if ANDROID/iOS/WINDOWS/MACCATALYST` bodies; non-platform body logs "no player". |
| SleepJournal/Services/AudioService.cs | EDIT: accept `IAudioPlayer` in ctor (default `PlatformAudioPlayer`); `PlayWavAsync` delegates to player; exceptions caught at the public entry points. |
| SleepJournal/MauiProgram.cs | EDIT: register `PlatformAudioPlayer` (optional). |
| SleepJournal.Tests/Services/AudioServiceTests.cs | EXTEND: player-mock injection, muted-no-call, player-throws-no-propagation, bytes-forwarded. |

## Interfaces / dependencies
- `IAudioService` unchanged (public contract stable).
- `IAudioPlayer` depends on nothing outside System.Threading and the WAV byte[].
- `AudioService` depends on `ILogger` + `IAudioPlayer`.
- `PlatformAudioPlayer` depends only on platform APIs; on net9.0 (non-MAUI build) it is a logging stub so the classlib still compiles.

## Data / control flow
1. ViewModel calls `PlayDropSoundAsync` / `PlayClickSoundAsync`.
2. AudioService checks `_audioEnabled`; if muted, returns (no player call).
3. AudioService calls `GenerateWav(...)` with `_volume`.
4. AudioService awaits `_player.PlayAsync(wav, _volume, ct)`.
5. Player routes bytes to the platform audio API; on failure the exception is
   caught by AudioService's try/catch, logged, and swallowed (never thrown to VM).

## Security boundaries
- No network, no secrets, no permissions beyond what AndroidManifest already declares.
- Audio playback is local-only.

## AC mapping
AC-001 -> build; AC-002 -> test count; AC-003/004/005/006 -> AudioServiceTests facts.
