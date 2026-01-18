namespace SleepJournal.Services;

/// <summary>
/// Service for managing local notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Request notification permissions from the user
    /// </summary>
    /// <returns>True if permissions were granted</returns>
    Task<bool> RequestPermissionsAsync();

    /// <summary>
    /// Schedule a daily bedtime reminder notification
    /// </summary>
    /// <param name="hour">Hour (0-23)</param>
    /// <param name="minute">Minute (0-59)</param>
    /// <param name="message">Notification message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ScheduleBedtimeReminderAsync(int hour, int minute, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel all scheduled notifications
    /// </summary>
    Task CancelAllNotificationsAsync();

    /// <summary>
    /// Check if notification permissions are granted
    /// </summary>
    Task<bool> AreNotificationsEnabledAsync();
}
