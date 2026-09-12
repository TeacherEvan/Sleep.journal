using System.Threading;
using System.Threading.Tasks;

namespace SleepJournal.Services;

/// <summary>
/// Default <see cref="IAudioPlayer"/> that routes WAV bytes to the platform
/// audio subsystem. Each platform block is compile-switched so the class still
/// compiles (as a logging stub) under the plain net9.0 classlib target where no
/// platform audio API is available.
/// </summary>
public class PlatformAudioPlayer : IAudioPlayer
{
    /// <inheritdoc />
    public async Task PlayAsync(byte[] wav, float volume, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (wav is null || wav.Length <= 44)
        {
            return;
        }

#if ANDROID
        await PlayAndroidAsync(wav, cancellationToken).ConfigureAwait(false);
#elif IOS || MACCATALYST
        await PlayAppleAsync(wav, cancellationToken).ConfigureAwait(false);
#elif WINDOWS
        await PlayWindowsAsync(wav, cancellationToken).ConfigureAwait(false);
#else
        // No platform audio API available under this target (e.g. plain net9.0
        // classlib). Bytes are produced by AudioService but cannot be routed;
        // surface this explicitly rather than silently dropping the sound.
        System.Diagnostics.Debug.WriteLine(
            $"PlatformAudioPlayer: no audio backend for this target; {wav.Length} bytes not routed.");
        await Task.CompletedTask;
#endif
    }

#if ANDROID
    /// <summary>
    /// Streams the PCM payload (bytes after the 44-byte RIFF header) through
    /// Android's <see cref="Android.Media.AudioTrack"/> in streaming mode.
    /// </summary>
    private static async Task PlayAndroidAsync(byte[] wav, CancellationToken cancellationToken)
    {
        var sampleRate = 44100;
        var channels = 1;
        var bitsPerSample = 16;
        var pcmLength = wav.Length - 44;
        var bufferSize = pcmLength;

        var config = Android.Media.AudioChannelConfiguration.FirstChannel;
        var audioTrack = new Android.Media.AudioTrack(
            Android.Media.StreamType.Music,
            sampleRate,
            channels,
            Android.Media.AudioFormat.EncodingPcm16,
            bufferSize,
            Android.Media.AudioTrackMode.Stream);

        audioTrack.Play();
        try
        {
            // AudioTrack.Write accepts a byte[] for PCM_16LE.
            audioTrack.Write(wav, 44, pcmLength);
            await Task.Delay(pcmLength / sampleRate * 1000, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            audioTrack.Stop();
            audioTrack.Release();
        }
    }
#endif

#if IOS || MACCATALYST
    /// <summary>
    /// Writes the WAV to a temp file and plays it with AVAudioPlayer.
    /// </summary>
    private static async Task PlayAppleAsync(byte[] wav, CancellationToken cancellationToken)
    {
        var path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"sleepjournal_{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.wav");
        try
        {
            await System.IO.File.WriteAllBytesAsync(path, wav, cancellationToken).ConfigureAwait(false);
            var player = new AVFoundation.AVAudioPlayer(new Foundation.NSUrl(path), out var error);
            if (error != null)
            {
                System.Diagnostics.Debug.WriteLine($"PlatformAudioPlayer: AVAudioPlayer error: {error}");
                return;
            }
            player.Play();
            await Task.Delay((int)(player.Duration * 1000), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            try { System.IO.File.Delete(path); } catch { }
        }
    }
#endif

#if WINDOWS
    /// <summary>
    /// Writes the WAV to a temp file and plays it through the WinRT
    /// <see cref="Windows.Media.Playback.MediaPlayer"/>.
    /// </summary>
    private static async Task PlayWindowsAsync(byte[] wav, CancellationToken cancellationToken)
    {
        var path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"sleepjournal_{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.wav");
        try
        {
            await System.IO.File.WriteAllBytesAsync(path, wav, cancellationToken).ConfigureAwait(false);
            var player = new Windows.Media.Playback.MediaPlayer();
            player.SetDataSource(path);
            player.Play();
            await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            try { System.IO.File.Delete(path); } catch { }
        }
    }
#endif
}
