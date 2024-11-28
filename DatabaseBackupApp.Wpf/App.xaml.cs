using DatabaseBackupApp.Wpf.Services;
using DatabaseBackupApp.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace DatabaseBackupApp.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Register services
        var services = new ServiceCollection();
        
        services.AddSingleton<BackupSchedulerService>();
        services.AddSingleton<GoogleDriveUploadService>();
        services.AddSingleton<DatabaseBackupService>();
        services.AddSingleton<SettingsService>();
        
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        var serviceProvider = services.BuildServiceProvider();

        // Create and show main window
        var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
