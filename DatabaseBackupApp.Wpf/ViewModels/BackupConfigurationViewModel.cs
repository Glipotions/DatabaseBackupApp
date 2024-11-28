using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using DatabaseBackupApp.Wpf.Common;
using DatabaseBackupApp.Wpf.Models;
using DatabaseBackupApp.Wpf.Services;
using DatabaseBackupApp.Wpf.Views;

namespace DatabaseBackupApp.Wpf.ViewModels
{
    public class BackupConfigurationViewModel : INotifyPropertyChanged
    {
        private bool _isHourlySchedule;
        private bool _isDailySchedule = true;
        private bool _isWeekdaySchedule;
        private bool _isOneTimeBackup;
        private bool _compressBackups = true;
        private bool _uploadToGoogleDrive;
        private bool _keepLocalCopy = true;
        private readonly DatabaseConnection _databaseConnection;
        private readonly Window _window;

        public BackupConfigurationViewModel(DatabaseConnection databaseConnection, Window window)
        {
            _databaseConnection = databaseConnection;
            _window = window;
            SaveCommand = new RelayCommand(Save);
        }

        public bool IsHourlySchedule
        {
            get => _isHourlySchedule;
            set
            {
                if (_isHourlySchedule != value)
                {
                    _isHourlySchedule = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDailySchedule
        {
            get => _isDailySchedule;
            set
            {
                if (_isDailySchedule != value)
                {
                    _isDailySchedule = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsWeekdaySchedule
        {
            get => _isWeekdaySchedule;
            set
            {
                if (_isWeekdaySchedule != value)
                {
                    _isWeekdaySchedule = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsOneTimeBackup
        {
            get => _isOneTimeBackup;
            set
            {
                if (_isOneTimeBackup != value)
                {
                    _isOneTimeBackup = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CompressBackups
        {
            get => _compressBackups;
            set
            {
                if (_compressBackups != value)
                {
                    _compressBackups = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UploadToGoogleDrive
        {
            get => _uploadToGoogleDrive;
            set
            {
                if (_uploadToGoogleDrive != value)
                {
                    _uploadToGoogleDrive = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(KeepLocalCopy));
                }
            }
        }

        public bool KeepLocalCopy
        {
            get => _keepLocalCopy;
            set
            {
                if (_keepLocalCopy != value)
                {
                    _keepLocalCopy = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }

        private void Save()
        {
            BackupSchedulerService.BackupScheduleType scheduleType;

            if (IsHourlySchedule)
                scheduleType = BackupSchedulerService.BackupScheduleType.Hourly;
            else if (IsDailySchedule)
                scheduleType = BackupSchedulerService.BackupScheduleType.Daily;
            else if (IsOneTimeBackup)
                scheduleType = BackupSchedulerService.BackupScheduleType.OneTime;
            else
                scheduleType = BackupSchedulerService.BackupScheduleType.WeekdaysMultipleTimes;

            // Save configuration and close dialog
            if (_window is BackupConfigurationWindow configWindow)
            {
                configWindow.ScheduleType = scheduleType;
                configWindow.DialogResult = true;
                configWindow.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
