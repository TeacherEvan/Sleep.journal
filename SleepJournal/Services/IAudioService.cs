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
    /// Gets the current volume level (0.0 to 1.0)
    /// </summary>
    /// <returns>The current volume level.</returns>
    float GetVolume();

    /// <summary>
    /// Sets the volume for audio playback (0.0 to 1.0)
    /// </summary>
    /// <param name="volume">Volume level between 0.0 and 1.0</param>
    void SetVolume(float volume);

    /// <summary>
    /// Gets a value indicating whether audio feedback is enabled.
    /// </summary>
    bool IsAudioEnabled { get; }

    /// <summary>
    /// Sets whether audio feedback is enabled or muted.
    /// </summary>
    /// <param name="enabled">true to enable audio; false to mute.</param>
    void SetAudioEnabled(bool enabled);
}
