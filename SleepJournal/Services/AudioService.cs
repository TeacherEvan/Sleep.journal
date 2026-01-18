using Microsoft.Extensions.Logging;

namespace SleepJournal.Services;

/// <summary>
/// Platform-agnostic audio service for playing feedback sounds
/// Uses simple file check instead of media playback due to MAUI media limitations
/// </summary>
public class AudioService : IAudioService
{
    private readonly ILogger<AudioService> _logger;
    private float _volume = 0.7f; // Default volume at 70%

    public AudioService(ILogger<AudioService> logger)
    {
        _logger = logger;
    }

    public async Task PlayDropSoundAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: MAUI AudioManager requires platform-specific setup
            // For now, just log the action
            _logger.LogDebug("Drop sound requested at volume {Volume}", _volume);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to play drop sound");
        }
    }

    public async Task PlayClickSoundAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Click sound requested at volume {Volume}", _volume * 0.5f);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to play click sound");
        }
    }

    public void SetVolume(float volume)
    {
        if (volume < 0f || volume > 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0.0 and 1.0");
        }

        _volume = volume;
        _logger.LogInformation("Audio volume set to {Volume}", volume);
    }
}
