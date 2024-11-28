using System.Windows;
using DatabaseBackupApp.Wpf.Models;
using DatabaseBackupApp.Wpf.Services;
using DatabaseBackupApp.Wpf.ViewModels;

namespace DatabaseBackupApp.Wpf.Views
{
    public partial class BackupConfigurationWindow : Window
    {
        public BackupConfigurationWindow(DatabaseConnection databaseConnection)
        {
            InitializeComponent();
            DataContext = new BackupConfigurationViewModel(databaseConnection, this);
        }

        public BackupSchedulerService.BackupScheduleType ScheduleType { get; set; }

        public BackupConfiguration BackupConfiguration { get; private set; }

        public void LoadConfiguration(BackupConfiguration config)
        {
            BackupConfiguration = config;
            // Add any additional logic to load configuration into UI
        }
    }
}
