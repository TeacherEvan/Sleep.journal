using System.Threading;
using System.Threading.Tasks;

namespace SleepJournal.Services;

/// <summary>
/// Platform audio player contract. The <see cref="AudioService"/> synthesises
/// WAV bytes and hands them here; this interface is the test seam that lets the
/// service be unit-tested without touching any platform audio API.
/// </summary>
public interface IAudioPlayer
{
    /// <summary>
    /// Plays the given PCM WAV bytes through the platform audio subsystem.
    /// </summary>
    /// <param name="wav">RIFF/WAVE byte array (header + PCM data).</param>
    /// <param name="volume">Playback volume, 0.0-1.0.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PlayAsync(byte[] wav, float volume, CancellationToken cancellationToken = default);
}
