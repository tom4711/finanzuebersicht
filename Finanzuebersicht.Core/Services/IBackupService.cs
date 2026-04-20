using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Finanzuebersicht.Services
{
    /// <summary>
    /// Service für Backup-, Restore- und Export-Operationen.
    /// </summary>
    public interface IBackupService
    {
        /// <summary>
        /// Erstellt ein manuelles Backup des aktuellen Datenstands.
        /// </summary>
        /// <param name="customPath">Optionaler benutzerdefinierter Pfad. Falls null, nutzt BackupPath aus Einstellungen.</param>
        /// <returns>Metadaten des erstellten Backups</returns>
        Task<BackupMetadata> CreateBackupAsync(string? customPath = null);

        /// <summary>
        /// Listet alle verfügbaren Backups im angegebenen Pfad.
        /// </summary>
        /// <param name="backupPath">Pfad zum Backup-Verzeichnis</param>
        /// <returns>Liste der Backup-Metadaten, sortiert nach Erstellungsdatum (neueste zuerst)</returns>
        Task<IEnumerable<BackupMetadata>> ListBackupsAsync(string backupPath);

        /// <summary>
        /// Stellt ein Backup wieder her mit atomaren Operationen und Validierung.
        /// </summary>
        /// <param name="backupPath">Pfad zum Backup-Verzeichnis</param>
        /// <param name="backupId">Eindeutige ID des Backups (Dateiname ohne .zip)</param>
        /// <returns>Ergebnis mit Status und optionalen Fehlermeldungen</returns>
        Task<RestoreResult> RestoreBackupAsync(string backupPath, string backupId);

        /// <summary>
        /// Löscht ein altes Backup.
        /// </summary>
        /// <param name="backupPath">Pfad zum Backup-Verzeichnis</param>
        /// <param name="backupId">Eindeutige ID des Backups</param>
        Task DeleteBackupAsync(string backupPath, string backupId);

        /// <summary>
        /// Exportiert aktuelle Daten als CSV (Transaktionen, Kategorien).
        /// </summary>
        /// <returns>Stream mit CSV-Daten (UTF-8)</returns>
        Task<Stream> ExportAsCSVAsync();
    }
}
