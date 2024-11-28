using System;
using System.IO;
using System.Text.Json;
using DatabaseBackupApp.Wpf.Models;

namespace DatabaseBackupApp.Wpf.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        private AppSettings _currentSettings;

        public SettingsService()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DatabaseBackupApp");
            
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _settingsPath = Path.Combine(appDataPath, "settings.json");
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    _currentSettings = JsonSerializer.Deserialize<AppSettings>(json);
                }
            }
            catch (Exception ex)
            {
                // If loading fails, create new settings
                System.Windows.MessageBox.Show($"Failed to load settings: {ex.Message}. Creating new settings file.",
                    "Settings Load Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }

            _currentSettings ??= new AppSettings();
            return _currentSettings;
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsPath, json);
                _currentSettings = settings;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save settings: {ex.Message}",
                    "Settings Save Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void SaveDatabase(DatabaseConnection database)
        {
            var settings = LoadSettings();
            
            // Update or add the database
            var existingDb = settings.SavedDatabases.Find(db => db.DatabaseName == database.DatabaseName);
            if (existingDb != null)
            {
                settings.SavedDatabases.Remove(existingDb);
            }
            settings.SavedDatabases.Add(database);
            
            SaveSettings(settings);
        }

        public void SaveBackupConfiguration(string databaseName, BackupConfiguration config)
        {
            var settings = LoadSettings();
            settings.DatabaseBackupConfigs[databaseName] = config;
            SaveSettings(settings);
        }
    }
}
