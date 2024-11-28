using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using DatabaseBackupApp.Wpf.Models;
using DatabaseBackupApp.Wpf.Services;
using DatabaseBackupApp.Wpf.Views;
using Microsoft.Data.SqlClient;
using DatabaseBackupApp.Wpf.Common;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DatabaseBackupApp.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly BackupSchedulerService _schedulerService;
        private readonly GoogleDriveUploadService _googleDriveService;
        private readonly SettingsService _settingsService;
        private DatabaseConnection _selectedDatabase;
        private string _localBackupPath;
        private string _googleDriveFolderId;

        public MainViewModel(
            BackupSchedulerService schedulerService,
            GoogleDriveUploadService googleDriveService,
            SettingsService settingsService)
        {
            _schedulerService = schedulerService;
            _googleDriveService = googleDriveService;
            _settingsService = settingsService;

            // Initialize commands
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
            ConfigureGoogleDriveCommand = new RelayCommand(ConfigureGoogleDrive);
            AddDatabaseCommand = new RelayCommand(AddDatabase);
            RemoveDatabaseCommand = new RelayCommand(RemoveDatabase, CanRemoveDatabase);
            ConfigureBackupCommand = new RelayCommand(ConfigureBackup, CanConfigureBackup);
            TestConnectionCommand = new RelayCommand(TestConnection, CanTestConnection);

            // Initialize collections
            Databases = new ObservableCollection<DatabaseConnection>();

            // Load saved settings
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = _settingsService.LoadSettings();
            
            // Load saved values
            LocalBackupPath = settings.LocalBackupPath;
            GoogleDriveFolderId = settings.GoogleDriveFolderId;
            
            // Load saved databases
            Databases.Clear();
            foreach (var database in settings.SavedDatabases)
            {
                Databases.Add(database);
            }
        }

        private void SaveSettings()
        {
            var settings = new AppSettings
            {
                LocalBackupPath = LocalBackupPath,
                GoogleDriveFolderId = GoogleDriveFolderId,
                SavedDatabases = new List<DatabaseConnection>(Databases)
            };
            
            _settingsService.SaveSettings(settings);
        }

        public ObservableCollection<DatabaseConnection> Databases { get; }

        public DatabaseConnection SelectedDatabase
        {
            get => _selectedDatabase;
            set
            {
                _selectedDatabase = value;
                OnPropertyChanged();
                RemoveDatabaseCommand.RaiseCanExecuteChanged();
                ConfigureBackupCommand.RaiseCanExecuteChanged();
                TestConnectionCommand.RaiseCanExecuteChanged();
            }
        }

        public string LocalBackupPath
        {
            get => _localBackupPath;
            set
            {
                _localBackupPath = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }

        public string GoogleDriveFolderId
        {
            get => _googleDriveFolderId;
            set
            {
                _googleDriveFolderId = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }

        public RelayCommand BrowseFolderCommand { get; }
        public RelayCommand ConfigureGoogleDriveCommand { get; }
        public RelayCommand AddDatabaseCommand { get; }
        public RelayCommand RemoveDatabaseCommand { get; }
        public RelayCommand ConfigureBackupCommand { get; }
        public RelayCommand TestConnectionCommand { get; }

        private void BrowseFolder()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select Backup Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                LocalBackupPath = dialog.FileName;
            }
        }


        private void ConfigureGoogleDrive()
        {
            var dialog = new GoogleDriveConfigurationWindow();
            if (dialog.ShowDialog() == true)
            {
                // Reload settings after configuration
                var settings = GoogleDriveSettings.Load();
                GoogleDriveFolderId = settings.DefaultFolderId;
                
                // Recreate Google Drive service with new settings
                _googleDriveService.UpdateCredentials(settings);
            }
        }

        private void AddDatabase()
        {
            var dialog = new DatabaseConfigurationWindow();
            if (dialog.ShowDialog() == true)
            {
                Databases.Add(dialog.DatabaseConnection);
                SaveSettings();
            }
        }

        private void RemoveDatabase()
        {
            if (SelectedDatabase != null)
            {
                Databases.Remove(SelectedDatabase);
                SaveSettings();
            }
        }

        private bool CanRemoveDatabase() => SelectedDatabase != null;

        private void ConfigureBackup()
        {
            if (SelectedDatabase != null)
            {
                var dialog = new BackupConfigurationWindow(SelectedDatabase);
                
                // Load existing configuration if available
                var settings = _settingsService.LoadSettings();
                if (settings.DatabaseBackupConfigs.TryGetValue(SelectedDatabase.DatabaseName, out var config))
                {
                    dialog.LoadConfiguration(config);
                }
                
                if (dialog.ShowDialog() == true)
                {
                    // Save the configuration
                    _settingsService.SaveBackupConfiguration(SelectedDatabase.DatabaseName, dialog.BackupConfiguration);
                    
                    // Schedule backup with configured settings
                    _schedulerService.ScheduleBackupJob(
                        SelectedDatabase,
                        LocalBackupPath,
                        GoogleDriveFolderId,
                        dialog.ScheduleType);
                }
            }
        }

        private bool CanConfigureBackup() => SelectedDatabase != null;

        private void TestConnection()
        {
            if (SelectedDatabase != null)
            {
                try
                {
                    using (var connection = new SqlConnection(SelectedDatabase.GetConnectionString()))
                    {
                        connection.Open();
                        MessageBox.Show("Connection successful!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection failed: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanTestConnection() => SelectedDatabase != null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
