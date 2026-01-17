using Microsoft.Extensions.Logging;

namespace SleepJournal.Services;

/// <summary>
/// Interface for biometric authentication service.
/// </summary>
public interface IBiometricService
{
    /// <summary>
    /// Checks if biometric authentication is available on the device.
    /// </summary>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Authenticates the user using biometric authentication.
    /// </summary>
    /// <param name="reason">The reason shown to the user for authentication.</param>
    /// <returns>True if authentication succeeded, false otherwise.</returns>
    Task<bool> AuthenticateAsync(string reason = "Authenticate to access your journal");

    /// <summary>
    /// Gets or sets whether biometric authentication is enabled by the user.
    /// </summary>
    Task<bool> IsBiometricEnabledAsync();

    /// <summary>
    /// Enables or disables biometric authentication.
    /// </summary>
    Task SetBiometricEnabledAsync(bool enabled);

    /// <summary>
    /// Checks if re-authentication is required based on session timeout.
    /// </summary>
    Task<bool> RequiresAuthenticationAsync();

    /// <summary>
    /// Records a successful authentication timestamp.
    /// </summary>
    Task RecordSuccessfulAuthenticationAsync();
}

/// <summary>
/// Service for handling biometric authentication using device capabilities.
/// </summary>
public class BiometricService : IBiometricService
{
    private readonly ILogger<BiometricService> _logger;

    public BiometricService(ILogger<BiometricService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            // Check if device has biometric hardware and it's enrolled
            var result = await SecureStorage.Default.GetAsync("test_biometric");
            // If we can access secure storage, biometrics may be available
            // Note: MAUI doesn't have direct biometric API, so we use OS-level features

#if ANDROID || IOS || MACCATALYST
            return true; // Platform supports biometrics
#else
            return false;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check biometric availability");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> AuthenticateAsync(string reason = "Authenticate to access your journal")
    {
        try
        {
            // For now, we simulate biometric auth using secure storage
            // In production, you would use platform-specific APIs:
            // Android: BiometricPrompt
            // iOS: LAContext (LocalAuthentication)

            var isEnabled = await IsBiometricEnabledAsync();
            if (!isEnabled)
            {
                _logger.LogInformation("Biometric authentication is disabled");
                return true; // Allow access if biometrics are disabled
            }

#if ANDROID || IOS || MACCATALYST
            // Simulate successful biometric authentication
            // TODO: Implement platform-specific biometric authentication
            await Task.Delay(500); // Simulate biometric scan
            _logger.LogInformation("Biometric authentication simulated successfully");
            return true;
#else
            _logger.LogWarning("Biometric authentication not supported on this platform");
            return true;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Biometric authentication failed");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsBiometricEnabledAsync()
    {
        try
        {
            var value = await SecureStorage.Default.GetAsync(AppConstants.Security.BiometricEnabledKey);
            return bool.TryParse(value, out var enabled) && enabled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read biometric enabled setting");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task SetBiometricEnabledAsync(bool enabled)
    {
        try
        {
            await SecureStorage.Default.SetAsync(AppConstants.Security.BiometricEnabledKey, enabled.ToString());
            _logger.LogInformation("Biometric authentication {Status}", enabled ? "enabled" : "disabled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set biometric enabled setting");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RequiresAuthenticationAsync()
    {
        try
        {
            var isEnabled = await IsBiometricEnabledAsync();
            if (!isEnabled)
                return false;

            var lastAuthStr = await SecureStorage.Default.GetAsync(AppConstants.Security.LastAuthTimestampKey);

            if (string.IsNullOrEmpty(lastAuthStr))
                return true; // First time, requires auth

            if (DateTime.TryParse(lastAuthStr, out var lastAuth))
            {
                var elapsed = DateTime.UtcNow - lastAuth;
                var requiresAuth = elapsed.TotalMinutes >= AppConstants.Security.AuthSessionMinutes;

                _logger.LogInformation("Last auth: {LastAuth}, Elapsed: {Elapsed}min, Requires auth: {RequiresAuth}",
                    lastAuth, elapsed.TotalMinutes, requiresAuth);

                return requiresAuth;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check authentication requirement");
            return true; // Fail secure - require auth on error
        }
    }

    /// <inheritdoc/>
    public async Task RecordSuccessfulAuthenticationAsync()
    {
        try
        {
            await SecureStorage.Default.SetAsync(AppConstants.Security.LastAuthTimestampKey,
                DateTime.UtcNow.ToString("O"));
            _logger.LogInformation("Recorded successful authentication");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record authentication timestamp");
        }
    }
}
