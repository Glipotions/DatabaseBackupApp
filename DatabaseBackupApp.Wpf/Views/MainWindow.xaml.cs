using System.Windows;
using DatabaseBackupApp.Wpf.ViewModels;

namespace DatabaseBackupApp.Wpf.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(
                new Services.BackupSchedulerService(
                    new Services.DatabaseBackupService(),
                    new Services.GoogleDriveUploadService()
                ),
                new Services.GoogleDriveUploadService(),
                new Services.SettingsService()
            );
        }
    }
}
