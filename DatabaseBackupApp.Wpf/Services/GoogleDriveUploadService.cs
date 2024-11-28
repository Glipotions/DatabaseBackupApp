using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using DatabaseBackupApp.Wpf.Models;

namespace DatabaseBackupApp.Wpf.Services
{
    public class GoogleDriveUploadService
    {
        private DriveService _driveService;
        private GoogleDriveSettings _settings;

        public GoogleDriveUploadService()
        {
            _settings = GoogleDriveSettings.Load();
            InitializeService();
        }

        public void UpdateCredentials(GoogleDriveSettings settings)
        {
            _settings = settings;
            InitializeService();
        }

        private void InitializeService()
        {
            if (string.IsNullOrEmpty(_settings.AccessToken) || 
                !_settings.AccessTokenExpiry.HasValue || 
                _settings.AccessTokenExpiry.Value <= DateTime.UtcNow)
            {
                return;
            }

            var credential = GoogleCredential.FromAccessToken(_settings.AccessToken);
            
            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Database Backup App"
            });
        }

        public async Task UploadFileAsync(string filePath, string folderId)
        {
            if (_driveService == null)
            {
                throw new InvalidOperationException("Google Drive service is not initialized. Please configure Google Drive settings first.");
            }

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = new[] { folderId }
            };

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var request = _driveService.Files.Create(fileMetadata, stream, "application/octet-stream");
                request.Fields = "id, name, webViewLink";
                var response = await request.UploadAsync();

                if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
                {
                    throw new Exception($"Upload failed: {response.Status}");
                }
            }
        }
    }
}
