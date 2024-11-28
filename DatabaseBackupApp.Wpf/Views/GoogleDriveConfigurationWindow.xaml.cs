using System.Windows;
using DatabaseBackupApp.Wpf.ViewModels;

namespace DatabaseBackupApp.Wpf.Views
{
    public partial class GoogleDriveConfigurationWindow : Window
    {
        public GoogleDriveConfigurationWindow()
        {
            InitializeComponent();
            DataContext = new GoogleDriveConfigurationViewModel();
        }
    }
}
