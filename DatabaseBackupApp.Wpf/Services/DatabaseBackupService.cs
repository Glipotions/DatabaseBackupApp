using System;
using System.IO;
using System.IO.Compression;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using DatabaseBackupApp.Wpf.Models;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DatabaseBackupApp.Wpf.Services
{
    public class DatabaseBackupService
    {
        public async Task BackupDatabaseAsync(DatabaseConnection connection, string backupPath)
        {
            try
            {
                // Ensure the backup directory exists
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                // Generate unique backup filename with timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"{connection.DatabaseName}_{timestamp}.bak";
                string fullBackupPath = Path.Combine(backupPath, backupFileName);

                // Create backup based on database type
                switch (connection.DatabaseType.ToUpper())
                {
                    case "MSSQL":
                        await BackupMsSqlDatabaseAsync(connection, fullBackupPath);
                        break;
                    
                    case "MYSQL":
                        await BackupMySqlDatabaseAsync(connection, fullBackupPath);
                        break;
                    
                    default:
                        throw new NotSupportedException($"Database type {connection.DatabaseType} is not supported.");
                }

                // Compress the backup file
                CompressBackupFile(fullBackupPath);
            }
            catch (Exception ex)
            {
                // Log the error and include more details
                throw new Exception($"Backup failed for {connection.DatabaseName} at {backupPath}: {ex.Message}\nConnection string: {connection.GetConnectionString()}", ex);
            }
        }

        private async Task BackupMsSqlDatabaseAsync(DatabaseConnection connection, string backupPath)
        {
            // Normalize the path to use backslashes and escape them for SQL
            string sqlBackupPath = backupPath.Replace("/", "\\").Replace("\\", "\\\\");
            
            using (var sqlCon = new SqlConnection(connection.GetConnectionString()))
            {
                await sqlCon.OpenAsync();
                
                string backupQuery = $"BACKUP DATABASE [{connection.DatabaseName}] TO DISK = N'{sqlBackupPath}' WITH FORMAT, INIT, NAME = N'{connection.DatabaseName}-Full Database Backup'";
                using (var command = new SqlCommand(backupQuery, sqlCon))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task BackupMySqlDatabaseAsync(DatabaseConnection connection, string backupPath)
        {
            using (var mysqlConnection = new MySqlConnection(connection.GetConnectionString()))
            {
                await mysqlConnection.OpenAsync();

                using (var command = new MySqlCommand())
                {
                    using (var backup = new MySqlBackup(command))
                    {
                        command.Connection = mysqlConnection;
                        backup.ExportToFile(backupPath);
                    }
                }
            }
        }

        private void CompressBackupFile(string backupFilePath)
        {
            string zipFilePath = backupFilePath + ".zip";
            
            try
            {
                using (FileStream originalFileStream = File.Open(backupFilePath, FileMode.Open))
                {
                    using (FileStream compressedFileStream = File.Create(zipFilePath))
                    {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                        {
                            originalFileStream.CopyTo(compressionStream);
                        }
                    }
                }

                // Delete the original .bak file after successful compression
                if (File.Exists(backupFilePath))
                {
                    File.Delete(backupFilePath);
                }
            }
            catch (Exception ex)
            {
                // If compression fails, ensure we clean up any partial zip file
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
                throw new Exception($"Failed to compress backup file: {ex.Message}", ex);
            }
        }
    }
}
