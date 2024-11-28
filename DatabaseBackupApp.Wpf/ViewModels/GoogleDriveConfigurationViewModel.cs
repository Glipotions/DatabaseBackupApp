using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DatabaseBackupApp.Wpf.Models;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.Windows;
using DatabaseBackupApp.Wpf.Common;

namespace DatabaseBackupApp.Wpf.ViewModels
{
    public class GoogleDriveConfigurationViewModel : INotifyPropertyChanged
    {
        private string _clientId;
        private string _clientSecret;
        private string _defaultFolderId;
        private string _authorizationStatus;
        private readonly GoogleDriveSettings _settings;

        public GoogleDriveConfigurationViewModel()
        {
            _settings = GoogleDriveSettings.Load();
            _clientId = _settings.ClientId;
            _clientSecret = _settings.ClientSecret;
            _defaultFolderId = _settings.DefaultFolderId;

            SaveCommand = new RelayCommand(Save, CanSave);
            AuthorizeCommand = new RelayCommand(async () => await AuthorizeAsync(), CanAuthorize);

            UpdateAuthorizationStatus();
        }

        public string ClientId
        {
            get => _clientId;
            set
            {
                if (_clientId != value)
                {
                    _clientId = value;
                    OnPropertyChanged();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AuthorizeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string ClientSecret
        {
            get => _clientSecret;
            set
            {
                if (_clientSecret != value)
                {
                    _clientSecret = value;
                    OnPropertyChanged();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AuthorizeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string DefaultFolderId
        {
            get => _defaultFolderId;
            set
            {
                if (_defaultFolderId != value)
                {
                    _defaultFolderId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AuthorizationStatus
        {
            get => _authorizationStatus;
            set
            {
                if (_authorizationStatus != value)
                {
                    _authorizationStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand AuthorizeCommand { get; }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(ClientId) &&
                   !string.IsNullOrWhiteSpace(ClientSecret);
        }

        private void Save()
        {
            try
            {
                _settings.ClientId = ClientId;
                _settings.ClientSecret = ClientSecret;
                _settings.DefaultFolderId = DefaultFolderId;
                _settings.Save();

                var window = Application.Current.Windows[0] as Window;
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAuthorize()
        {
            return !string.IsNullOrWhiteSpace(ClientId) &&
                   !string.IsNullOrWhiteSpace(ClientSecret);
        }

        private async Task AuthorizeAsync()
        {
            try
            {
                var clientSecrets = new ClientSecrets
                {
                    ClientId = ClientId,
                    ClientSecret = ClientSecret
                };

                // Use the Google OAuth 2.0 authorization flow
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    new[] { DriveService.Scope.Drive },
                    "user",
                    System.Threading.CancellationToken.None);

                _settings.RefreshToken = credential.Token.RefreshToken;
                _settings.AccessToken = credential.Token.AccessToken;
                _settings.AccessTokenExpiry = DateTime.Now.AddSeconds(Convert.ToDouble(credential.Token.ExpiresInSeconds));

                UpdateAuthorizationStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authorization failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                AuthorizationStatus = "Not authorized";
            }
        }

        private void UpdateAuthorizationStatus()
        {
            if (!string.IsNullOrEmpty(_settings.AccessToken) && 
                _settings.AccessTokenExpiry.HasValue && 
                _settings.AccessTokenExpiry.Value > DateTime.UtcNow)
            {
                AuthorizationStatus = "Authorized";
            }
            else
            {
                AuthorizationStatus = "Not authorized";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
