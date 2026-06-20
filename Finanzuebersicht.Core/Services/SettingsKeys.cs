namespace Finanzuebersicht.Core.Services
{
    /// <summary>
    /// Konstanten für App-Einstellungen.
    /// </summary>
    public static class SettingsKeys
    {
        // Daten und Pfade
        /// <summary>
        /// Benutzerdefinierter Pfad für Datenspeicher.
        /// </summary>
        public const string DataPath = "DataPath";

        // Backup-Einstellungen
        /// <summary>
        /// Pfad zum Backup-Verzeichnis (Standard: {DataPath}/backups).
        /// </summary>
        public const string BackupPath = "BackupPath";

        /// <summary>
        /// Zeitstempel des letzten Backups (ISO 8601 Format).
        /// </summary>
        public const string LastBackupTime = "LastBackupTime";

        /// <summary>
        /// Ob automatische Backups aktiviert sind (true/false).
        /// </summary>
        public const string AutoBackupEnabled = "AutoBackupEnabled";

        /// <summary>
        /// Häufigkeit automatischer Backups: "daily", "weekly", "monthly".
        /// </summary>
        public const string BackupFrequency = "BackupFrequency";

        /// <summary>
        /// Maximale Anzahl aufzubewahrende Backups (Standard: 10).
        /// </summary>
        public const string MaxBackupsToKeep = "MaxBackupsToKeep";

        /// <summary>
        /// Whether the first-run onboarding wizard was completed (true/false).
        /// </summary>
        public const string OnboardingCompleted = "OnboardingCompleted";

        /// <summary>
        /// Whether the dashboard budget section is expanded (true/false).
        /// </summary>
        public const string DashboardBudgetExpanded = "DashboardBudgetExpanded";

        /// <summary>
        /// Whether the dashboard year details section is expanded (true/false).
        /// </summary>
        public const string DashboardYearDetailsExpanded = "DashboardYearDetailsExpanded";
    }
}
