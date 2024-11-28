using System.Windows;
using DatabaseBackupApp.Wpf.Models;
using DatabaseBackupApp.Wpf.ViewModels;

namespace DatabaseBackupApp.Wpf.Views
{
    public partial class DatabaseConfigurationWindow : Window
    {
        public DatabaseConfigurationWindow()
        {
            InitializeComponent();
            DataContext = new DatabaseConfigurationViewModel(this);
        }

        public DatabaseConnection DatabaseConnection { get; set; }
    }
}
