namespace SleepJournal.Services;

/// <summary>
/// Service for playing audio feedback sounds
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Plays a water drop sound effect
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PlayDropSoundAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Plays a soft click sound
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PlayClickSoundAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the volume for audio playback (0.0 to 1.0)
    /// </summary>
    /// <param name="volume">Volume level between 0.0 and 1.0</param>
    void SetVolume(float volume);
}
