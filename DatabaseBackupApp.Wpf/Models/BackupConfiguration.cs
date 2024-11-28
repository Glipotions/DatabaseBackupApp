using DatabaseBackupApp.Wpf.Services;
using System;
using System.Text.Json.Serialization;

namespace DatabaseBackupApp.Wpf.Models
{
    public class BackupConfiguration
    {
        [JsonPropertyName("scheduleType")]
        public BackupSchedulerService.BackupScheduleType ScheduleType { get; set; }

        [JsonPropertyName("backupPath")]
        public string BackupPath { get; set; }

        [JsonPropertyName("compressionEnabled")]
        public bool CompressionEnabled { get; set; } = true;

        [JsonPropertyName("retentionDays")]
        public int RetentionDays { get; set; } = 30;

        [JsonPropertyName("specificTime")]
        public DateTime? SpecificTime { get; set; }

        [JsonPropertyName("daysOfWeek")]
        public DayOfWeek[] DaysOfWeek { get; set; }
    }
}
