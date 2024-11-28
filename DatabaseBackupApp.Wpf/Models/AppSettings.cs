using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DatabaseBackupApp.Wpf.Models
{
    public class AppSettings
    {
        public string LocalBackupPath { get; set; }
        public string GoogleDriveFolderId { get; set; }
        public List<DatabaseConnection> SavedDatabases { get; set; } = new List<DatabaseConnection>();
        public Dictionary<string, BackupConfiguration> DatabaseBackupConfigs { get; set; } = new Dictionary<string, BackupConfiguration>();
    }
}
