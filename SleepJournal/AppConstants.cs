namespace SleepJournal;

/// <summary>
/// Application-wide constants to eliminate magic numbers and improve maintainability.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Database configuration constants.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Name of the SQLite database file.
        /// </summary>
        public const string FileName = "sleepjournal.db";

        /// <summary>
        /// Maximum number of KDF iterations for WAL mode initialization retries.
        /// </summary>
        public const int MaxWalRetries = 3;
    }

    /// <summary>
    /// Pagination constants for data loading.
    /// </summary>
    public static class Pagination
    {
        /// <summary>
        /// Number of entries to load per page in history view.
        /// </summary>
        public const int PageSize = 20;

        /// <summary>
        /// Threshold for triggering next page load (items remaining).
        /// </summary>
        public const int RemainingItemsThreshold = 5;
    }

    /// <summary>
    /// Validation constants for journal entries.
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Maximum allowed characters for journal entry text.
        /// </summary>
        public const int MaxTextLength = 200;

        /// <summary>
        /// Minimum value for rating scales (mood, anxiety, regret).
        /// </summary>
        public const int MinRating = 1;

        /// <summary>
        /// Maximum value for rating scales (mood, anxiety, regret).
        /// </summary>
        public const int MaxRating = 10;

        /// <summary>
        /// Default rating value.
        /// </summary>
        public const int DefaultRating = 5;
    }

    /// <summary>
    /// Default settings values.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// Default reminder time (9:00 PM).
        /// </summary>
        public static readonly TimeSpan ReminderTime = new(21, 0, 0);

        /// <summary>
        /// Default dark mode setting.
        /// </summary>
        public const bool UseDarkMode = false;

        /// <summary>
        /// Default reminder enabled setting.
        /// </summary>
        public const bool EnableReminders = false;
    }

    /// <summary>
    /// Security and encryption constants.
    /// </summary>
    public static class Security
    {
        /// <summary>
        /// Secure storage key for biometric authentication preference.
        /// </summary>
        public const string BiometricEnabledKey = "BiometricAuthEnabled";

        /// <summary>
        /// Secure storage key for last successful authentication timestamp.
        /// </summary>
        public const string LastAuthTimestampKey = "LastAuthTimestamp";

        /// <summary>
        /// Minutes before requiring re-authentication.
        /// </summary>
        public const int AuthSessionMinutes = 30;
    }
}
