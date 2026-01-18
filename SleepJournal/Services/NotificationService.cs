using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace SleepJournal.Services;

/// <summary>
/// Platform-agnostic notification service for scheduling local notifications
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private const int BedtimeNotificationId = 1000;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> RequestPermissionsAsync()
    {
        try
        {
            var enabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            if (!enabled)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
                enabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            }

            _logger.LogInformation("Notification permissions {Status}", enabled ? "granted" : "denied");
            return enabled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request notification permissions");
            return false;
        }
    }

    public async Task ScheduleBedtimeReminderAsync(int hour, int minute, string message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (hour < 0 || hour > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23");
        }

        if (minute < 0 || minute > 59)
        {
            throw new ArgumentOutOfRangeException(nameof(minute), "Minute must be between 0 and 59");
        }

        try
        {
            // Cancel existing bedtime notifications
            LocalNotificationCenter.Current.Cancel(BedtimeNotificationId);

            var now = DateTime.Now;
            var scheduledTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

            // If the time has already passed today, schedule for tomorrow
            if (scheduledTime < now)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            var notification = new NotificationRequest
            {
                NotificationId = BedtimeNotificationId,
                Title = "💤 Sleep Journal Reminder",
                Description = message,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = scheduledTime,
                    RepeatType = NotificationRepeat.Daily
                },
                Android = new AndroidOptions
                {
                    Priority = AndroidPriority.High,
                    ChannelId = "sleepjournal_reminders"
                }
            };

            await LocalNotificationCenter.Current.Show(notification);

            _logger.LogInformation(
                "Scheduled daily bedtime reminder at {Hour}:{Minute:D2}",
                hour,
                minute);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule bedtime reminder");
            throw;
        }
    }

    public Task CancelAllNotificationsAsync()
    {
        try
        {
            LocalNotificationCenter.Current.CancelAll();
            _logger.LogInformation("Cancelled all notifications");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel notifications");
            throw;
        }
    }

    public async Task<bool> AreNotificationsEnabledAsync()
    {
        try
        {
            var result = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check notification status");
            return false;
        }
    }
}
