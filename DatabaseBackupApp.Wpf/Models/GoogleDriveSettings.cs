using System;
using System.Text.Json;
using System.IO;

namespace DatabaseBackupApp.Wpf.Models
{
    public class GoogleDriveSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public DateTime? AccessTokenExpiry { get; set; }
        public string DefaultFolderId { get; set; }

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DatabaseBackupApp",
            "googledrive_settings.json");

        public static GoogleDriveSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string jsonContent = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<GoogleDriveSettings>(jsonContent);
                }
            }
            catch (Exception)
            {
                // If there's any error loading the settings, return a new instance
            }
            return new GoogleDriveSettings();
        }

        public void Save()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string jsonContent = JsonSerializer.Serialize(this, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(SettingsPath, jsonContent);
            }
            catch (Exception)
            {
                throw new Exception("Failed to save Google Drive settings");
            }
        }
    }
}
