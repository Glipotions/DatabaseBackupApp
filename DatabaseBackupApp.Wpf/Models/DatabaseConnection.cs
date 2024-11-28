using System;

namespace DatabaseBackupApp.Wpf.Models
{
    public class DatabaseConnection
    {
        public string Id { get; set; }
        public string DatabaseType { get; set; } // MSSQL, MySQL, etc.
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool WindowsAuthentication { get; set; }

        public string GetConnectionString()
        {
            // Implement connection string generation based on database type
            switch (DatabaseType.ToUpper())
            {
                case "MSSQL":
                    return WindowsAuthentication 
                        ? $"Server={ServerName};Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;" 
                        : $"Server={ServerName};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=True;";
                
                case "MYSQL":
                    return $"Server={ServerName};Database={DatabaseName};Uid={Username};Pwd={Password};";
                
                default:
                    throw new NotSupportedException($"Database type {DatabaseType} is not supported.");
            }
        }
    }
}
