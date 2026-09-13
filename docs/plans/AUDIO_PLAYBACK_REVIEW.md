# Code Review — Audio Playback Wiring (Sleep Journal)

**Date:** 2026-09-13 | **Scope:** 5 files (IAudioPlayer.cs, PlatformAudioPlayer.cs, AudioService.cs, MauiProgram.cs, AudioServiceTests.cs) | **Verdict:** APPROVE (0 required, 3 nits)

## Context
Plan `docs/plans/TODO.md` (10 objectives, all ticked) wired `AudioService.PlayWavAsync` bytes to an injected `IAudioPlayer`. Verified: `dotnet build -f net9.0` 0 errors; `dotnet test` 120 passed, 0 failed; break-it check (removed try/catch in PlayDropSoundAsync) correctly fails the propagation test → tests are real, not tautologies.

## Correctness
- [x] Change matches spec: PlayWavAsync delegates to `_audioPlayer.PlayAsync`; no more no-op.
- [x] Edge cases: null WAV (<=44 bytes) rejected by PlatformAudioPlayer; volume range validated; muted → zero player calls.
- [x] Error paths: exceptions caught at public entry points (PlayDropSoundAsync/PlayClickSoundAsync), logged, swallowed — never thrown to ViewModel.
- [x] Tests cover the change: 5 new tests (player-called, muted-no-call, player-throws-no-propagation x2, volume-forwarded) + break-it check confirms they catch the regression.

## Readability
- [x] Names clear and consistent (IAudioPlayer, PlatformAudioPlayer, PlayAsync, PlayWavAsync).
- [x] Logic straightforward; no nested ternaries or deep callbacks.
- [x] Comments explain the platform-switch intent and the net9.0 stub behaviour.

## Architecture
- [x] Follows existing DI pattern (singleton services in MauiProgram).
- [x] Clean boundary: AudioService orchestrates (synth + mute + volume), player owns transport. No circular deps.
- [x] IAudioPlayer depends only on System.Threading + byte[] — no MAUI coupling in the test seam.

## Security
- [x] No secrets, no credentials, no network calls. Audio is local-only.
- [x] No input from untrusted sources at the playback boundary.

## Performance
- [x] No N+1, no unbounded loops. WAV synthesis is bounded by duration (25–90ms feedback sounds).
- [x] No synchronous-over-async hazards.

## Findings (all NIT — no action required)

| # | Severity | File:line | Finding | Proposed move |
|---|----------|-----------|---------|---------------|
| NIT-1 | LOW | PlatformAudioPlayer.cs:58 | Android `AudioTrack` buffer size = full PCM length (`pcmLength`). For short feedback sounds this is fine, but a long WAV could allocate a large streaming buffer. | Use a fixed streaming buffer (e.g. 8192) and loop-write in chunks. Out of scope for this plan (feedback sounds ≤90ms). |
| NIT-2 | LOW | PlatformAudioPlayer.cs:108 | Windows playback uses hardcoded `Task.Delay(500)` instead of polling `player.State`. | Poll `player.PlaybackState` until `Stopped`/`Paused` with a timeout. Platform-only, not testable on this host. |
| NIT-3 | INFO | PlatformAudioPlayer.cs:88 | `PlayAppleAsync` uses `player.Duration * 1000` for the delay; Duration may be 0 if not loaded yet. | Same pattern as NIT-2 — poll state. Apple-only, not testable here. |

## Verification
- `dotnet build SleepJournal/SleepJournal.csproj -f net9.0` → 0 errors (23 warnings, all pre-existing XamlC/NU1603/NETSDK1206).
- `dotnet test` → 120 passed, 0 failed, 0 skipped.
- Break-it check: removed try/catch → `PlayDropSoundAsync_PlayerThrows_DoesNotPropagate` FAILS (1/15). Restored → 120/120 green. Tests are load-bearing.
- `git status` clean; no uncommitted source changes; audit scratch gitignored.

## New orchestration jobs
None. No CRITICAL or required findings; all 3 nits are platform-only, untestable on this host, and out of scope for the audio wiring plan. Close the loop.
