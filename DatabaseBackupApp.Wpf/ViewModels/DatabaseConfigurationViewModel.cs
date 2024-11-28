using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using DatabaseBackupApp.Wpf.Common;
using DatabaseBackupApp.Wpf.Models;
using DatabaseBackupApp.Wpf.Views;
using Microsoft.Data.SqlClient;

namespace DatabaseBackupApp.Wpf.ViewModels
{
    public class DatabaseConfigurationViewModel : INotifyPropertyChanged
    {
        private string _databaseType;
        private string _serverName;
        private string _databaseName;
        private bool _windowsAuthentication = true;
        private string _username;
        private string _password;
        private Window _window;

        public DatabaseConfigurationViewModel(Window window)
        {
            _window = window;
            DatabaseTypes = new List<string> { "MSSQL", "MySQL" };
            SaveCommand = new RelayCommand(Save, CanSave);
            TestConnectionCommand = new RelayCommand(TestConnection, CanTestConnection);
        }

        public List<string> DatabaseTypes { get; }

        public string DatabaseType
        {
            get => _databaseType;
            set
            {
                if (_databaseType != value)
                {
                    _databaseType = value;
                    OnPropertyChanged();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (TestConnectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string ServerName
        {
            get => _serverName;
            set
            {
                if (_serverName != value)
                {
                    _serverName = value;
                    OnPropertyChanged();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (TestConnectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string DatabaseName
        {
            get => _databaseName;
            set
            {
                if (_databaseName != value)
                {
                    _databaseName = value;
                    OnPropertyChanged();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (TestConnectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool WindowsAuthentication
        {
            get => _windowsAuthentication;
            set
            {
                if (_windowsAuthentication != value)
                {
                    _windowsAuthentication = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Username));
                    OnPropertyChanged(nameof(Password));
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (TestConnectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (TestConnectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (TestConnectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand TestConnectionCommand { get; }

        private bool CanSave()
        {
            if (string.IsNullOrWhiteSpace(DatabaseType) ||
                string.IsNullOrWhiteSpace(ServerName) ||
                string.IsNullOrWhiteSpace(DatabaseName))
                return false;

            // For SQL Authentication, require username and password
            if (!WindowsAuthentication)
            {
                if (string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(Password))
                    return false;
            }

            return true;
        }

        private void Save()
        {
            var databaseConnection = new DatabaseConnection
            {
                DatabaseType = DatabaseType,
                ServerName = ServerName,
                DatabaseName = DatabaseName,
                WindowsAuthentication = WindowsAuthentication,
                Username = Username,
                Password = Password
            };

            // Close dialog with success
            if (_window is DatabaseConfigurationWindow configWindow)
            {
                configWindow.DatabaseConnection = databaseConnection;
                configWindow.DialogResult = true;
                configWindow.Close();
            }
        }

        private bool CanTestConnection()
        {
            return CanSave();
        }

        private void TestConnection()
        {
            try
            {
                var databaseConnection = new DatabaseConnection
                {
                    DatabaseType = DatabaseType,
                    ServerName = ServerName,
                    DatabaseName = DatabaseName,
                    WindowsAuthentication = WindowsAuthentication,
                    Username = Username,
                    Password = Password
                };

                using (var connection = new SqlConnection(databaseConnection.GetConnectionString()))
                {
                    connection.Open();
                    System.Windows.MessageBox.Show("Connection successful!", "Success", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Connection failed: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
