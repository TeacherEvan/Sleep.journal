using System;
using Microsoft.Extensions.Logging;

namespace SleepJournal.Services;

/// <summary>
/// Platform-agnostic audio service for playing feedback sounds.
///
/// Sound generation is pure-managed PCM synthesis (sine wave) so the bytes are
/// deterministic and unit-testable on every target framework. The generated
/// WAV is the payload a platform-specific player consumes; on platforms with
/// no player wired yet the bytes are produced but not routed to speakers, which
/// is logged explicitly rather than silently disguised as "audio works".
/// </summary>
public class AudioService : IAudioService
{
    private const int SampleRate = 44100;
    private const int BitsPerSample = 16;
    private const int Channels = 1;

    private readonly ILogger<AudioService> _logger;
    private float _volume = 0.7f; // Default volume at 70%
    private bool _audioEnabled = true; // Default audio on

    private readonly IAudioPlayer _audioPlayer;

    public AudioService(ILogger<AudioService> logger, IAudioPlayer? audioPlayer = null)
    {
        _logger = logger;
        _audioPlayer = audioPlayer ?? new PlatformAudioPlayer();
    }

    public async Task PlayDropSoundAsync(CancellationToken cancellationToken = default)
    {
        if (!_audioEnabled)
        {
            _logger.LogDebug("Drop sound skipped: audio disabled");
            return;
        }

        try
        {
            // Water drop: short descending tone.
            var wav = GenerateWav(frequencyHz: 320, durationMs: 90, volume: _volume);
            await PlayWavAsync(wav, _volume, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to play drop sound");
        }
    }

    public async Task PlayClickSoundAsync(CancellationToken cancellationToken = default)
    {
        if (!_audioEnabled)
        {
            _logger.LogDebug("Click sound skipped: audio disabled");
            return;
        }

        try
        {
            // Soft click: very short higher tone at half volume.
            var wav = GenerateWav(frequencyHz: 960, durationMs: 25, volume: _volume * 0.5f);
            await PlayWavAsync(wav, _volume * 0.5f, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to play click sound");
        }
    }

    public float GetVolume() => _volume;

    public void SetVolume(float volume)
    {
        if (volume < 0f || volume > 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0.0 and 1.0");
        }

        _volume = volume;
        _logger.LogInformation("Audio volume set to {Volume}", volume);
    }

    public bool IsAudioEnabled => _audioEnabled;

    public void SetAudioEnabled(bool enabled)
    {
        _audioEnabled = enabled;
        _logger.LogInformation("Audio feedback {Status}", enabled ? "enabled" : "disabled");
    }

    /// <summary>
    /// Generates a PCM WAV byte array for a sine wave of the given frequency,
    /// duration and peak amplitude (0.0-1.0). Pure managed code; no platform API.
    /// </summary>
    public byte[] GenerateWav(float frequencyHz, int durationMs, float volume)
    {
        if (frequencyHz <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(frequencyHz), "Frequency must be greater than zero.");
        }

        if (durationMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMs), "Duration must be non-negative.");
        }

        if (volume is < 0f or > 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0.0 and 1.0");
        }

        var sampleCount = (int)(SampleRate * durationMs / 1000.0);
        var dataLength = sampleCount * (BitsPerSample / 8) * Channels;
        var wav = new byte[44 + dataLength];

        // RIFF header
        wav[0] = (byte)'R'; wav[1] = (byte)'I'; wav[2] = (byte)'F'; wav[3] = (byte)'F';
        WriteInt32(wav, 4, 36 + dataLength); // ChunkSize
        wav[8] = (byte)'W'; wav[9] = (byte)'A'; wav[10] = (byte)'V'; wav[11] = (byte)'E';

        // fmt sub-chunk
        wav[12] = (byte)'f'; wav[13] = (byte)'m'; wav[14] = (byte)'t'; wav[15] = (byte)' ';
        WriteInt32(wav, 16, 16); // Subchunk1Size
        WriteInt16(wav, 20, 1); // AudioFormat = PCM
        WriteInt16(wav, 22, Channels);
        WriteInt32(wav, 24, SampleRate);
        WriteInt32(wav, 28, SampleRate * Channels * (BitsPerSample / 8)); // ByteRate
        WriteInt16(wav, 32, Channels * (BitsPerSample / 8)); // BlockAlign
        WriteInt16(wav, 34, BitsPerSample);

        // data sub-chunk
        wav[36] = (byte)'d'; wav[37] = (byte)'a'; wav[38] = (byte)'t'; wav[39] = (byte)'a';
        WriteInt32(wav, 40, dataLength);

        // Samples with a short linear fade-in/out so the click doesn't pop.
        var peak = (short)(short.MaxValue * volume);
        for (var i = 0; i < sampleCount; i++)
        {
            var t = (double)i / sampleCount;
            var env = t < 0.15 ? t / 0.15 : (t > 0.85 ? (1.0 - t) / 0.15 : 1.0);
            var sample = (short)(peak * env * Math.Sin(2.0 * Math.PI * frequencyHz * i / SampleRate));
            var offset = 44 + i * 2;
            wav[offset] = (byte)(sample & 0xFF);
            wav[offset + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return wav;
    }

    /// <summary>
    /// Routes generated WAV bytes to the platform audio player.
    /// TODO: wire per-platform playback (Android: AudioTrack / Xamarin.Essentials;
    /// iOS: AVAudioPlayer; Windows: SoundPlayer; Mac: NSSound). Until a platform
    /// player is implemented the bytes are produced and logged, not silenced.
    /// </summary>
    private async Task PlayWavAsync(byte[] wav, float volume, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Generated {ByteCount}-byte WAV sample for playback", wav.Length);
        await _audioPlayer.PlayAsync(wav, volume, cancellationToken).ConfigureAwait(false);
    }

    private static void WriteInt32(byte[] buf, int offset, int value)
    {
        buf[offset] = (byte)(value & 0xFF);
        buf[offset + 1] = (byte)((value >> 8) & 0xFF);
        buf[offset + 2] = (byte)((value >> 16) & 0xFF);
        buf[offset + 3] = (byte)((value >> 24) & 0xFF);
    }

    private static void WriteInt16(byte[] buf, int offset, int value)
    {
        buf[offset] = (byte)(value & 0xFF);
        buf[offset + 1] = (byte)((value >> 8) & 0xFF);
    }
}
