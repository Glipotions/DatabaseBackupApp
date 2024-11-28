using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using DatabaseBackupApp.Wpf.Models;
using System.IO;

namespace DatabaseBackupApp.Wpf.Services
{
    public class BackupSchedulerService
    {
        private readonly IScheduler _scheduler;
        private readonly DatabaseBackupService _backupService;
        private readonly GoogleDriveUploadService _googleDriveService;
        private static DatabaseConnection _currentDatabase;
        private static DatabaseBackupService _currentBackupService;
        private static GoogleDriveUploadService _currentGoogleDriveService;

        public BackupSchedulerService(
            DatabaseBackupService backupService, 
            GoogleDriveUploadService googleDriveService)
        {
            _backupService = backupService;
            _googleDriveService = googleDriveService;
            
            // Create a scheduler factory and start it immediately
            var schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler().Result;
            _scheduler.Start().Wait(); // Start the scheduler immediately
        }

        public async Task ScheduleBackupJob(
            DatabaseConnection database, 
            string localBackupPath, 
            string googleDriveFolderId = null,
            BackupScheduleType scheduleType = BackupScheduleType.Daily)
        {
            // Store the current database and services for the job
            _currentDatabase = database;
            _currentBackupService = _backupService;
            _currentGoogleDriveService = _googleDriveService;

            // Create a job for the database backup
            var job = JobBuilder.Create<DatabaseBackupJob>()
                .WithIdentity($"backup-{database.DatabaseName}-{DateTime.Now.Ticks}")
                .UsingJobData("DatabaseName", database.DatabaseName)
                .UsingJobData("ServerName", database.ServerName)
                .UsingJobData("DatabaseType", database.DatabaseType)
                .UsingJobData("Username", database.Username)
                .UsingJobData("Password", database.Password)
                .UsingJobData("WindowsAuthentication", database.WindowsAuthentication)
                .UsingJobData("LocalBackupPath", localBackupPath)
                .UsingJobData("GoogleDriveFolderId", googleDriveFolderId ?? "")
                .Build();

            // Create trigger based on schedule type
            ITrigger trigger = CreateTriggerForScheduleType(scheduleType);

            // Schedule the job
            await _scheduler.ScheduleJob(job, trigger);

            // For one-time backups, we'll show a message
            if (scheduleType == BackupScheduleType.OneTime)
            {
                System.Windows.MessageBox.Show("One-time backup has been initiated.", "Backup Started", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private ITrigger CreateTriggerForScheduleType(BackupScheduleType scheduleType)
        {
            switch (scheduleType)
            {
                case BackupScheduleType.Hourly:
                    return TriggerBuilder.Create()
                        .WithIdentity($"hourlyTrigger-{DateTime.Now.Ticks}")
                        .WithSimpleSchedule(x => x
                            .WithIntervalInHours(1)
                            .RepeatForever())
                        .Build();

                case BackupScheduleType.Daily:
                    return TriggerBuilder.Create()
                        .WithIdentity($"dailyTrigger-{DateTime.Now.Ticks}")
                        .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0))
                        .Build();

                case BackupScheduleType.WeekdaysMultipleTimes:
                    return TriggerBuilder.Create()
                        .WithIdentity($"weekdaysTrigger-{DateTime.Now.Ticks}")
                        .WithSchedule(CronScheduleBuilder.CronSchedule("0 0 9,13,17 ? * MON-FRI"))
                        .Build();

                case BackupScheduleType.OneTime:
                    return TriggerBuilder.Create()
                        .WithIdentity($"oneTimeTrigger-{DateTime.Now.Ticks}")
                        .StartNow()
                        .Build();

                default:
                    throw new ArgumentException("Invalid schedule type");
            }
        }

        public class DatabaseBackupJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                try
                {
                    var dataMap = context.JobDetail.JobDataMap;
                    
                    // Reconstruct the database connection from job data
                    var database = new DatabaseConnection
                    {
                        DatabaseName = dataMap.GetString("DatabaseName"),
                        ServerName = dataMap.GetString("ServerName"),
                        DatabaseType = dataMap.GetString("DatabaseType"),
                        Username = dataMap.GetString("Username"),
                        Password = dataMap.GetString("Password"),
                        WindowsAuthentication = dataMap.GetBoolean("WindowsAuthentication")
                    };

                    var localBackupPath = dataMap.GetString("LocalBackupPath");
                    var googleDriveFolderId = dataMap.GetString("GoogleDriveFolderId");

                    // Perform local backup
                    await _currentBackupService.BackupDatabaseAsync(database, localBackupPath);

                    // Optional: Upload to Google Drive if folder ID is provided
                    if (!string.IsNullOrEmpty(googleDriveFolderId))
                    {
                        var backupFiles = Directory.GetFiles(localBackupPath, "*.bak");
                        foreach (var backupFile in backupFiles)
                        {
                            await _currentGoogleDriveService.UploadFileAsync(backupFile, googleDriveFolderId);
                        }
                    }

                    System.Windows.MessageBox.Show($"Backup completed for database: {database.DatabaseName}", 
                        "Backup Complete", System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Backup failed: {ex.Message}", 
                        "Backup Error", System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public enum BackupScheduleType
        {
            Hourly,
            Daily,
            WeekdaysMultipleTimes,
            OneTime
        }
    }
}
