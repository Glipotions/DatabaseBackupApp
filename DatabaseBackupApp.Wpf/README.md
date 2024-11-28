# Database Backup Application

## Overview
A comprehensive Windows application for automated database backups with multiple storage and scheduling options.

## Features
- Support for multiple database types (MSSQL, MySQL)
- Flexible backup scheduling
- Local folder and Google Drive backup destinations
- Compressed ZIP backup format
- OAuth2 Google Drive integration

## Prerequisites
- .NET 6.0 or later
- Visual Studio 2022
- Google Drive API Credentials (for Google Drive backup)

## Installation
1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Configure Google Drive credentials (if using Google Drive backup)

## Configuration
### Database Connection
- Add database connection details in the application settings
- Supports Windows and standard authentication

### Backup Scheduling
Supported backup schedules:
- Hourly
- Daily
- Weekdays at multiple times

### Google Drive Integration
1. Create a Google Cloud project
2. Enable Google Drive API
3. Create OAuth 2.0 credentials
4. Download credentials JSON file
5. Configure in the application settings

## Security Considerations
- Credentials are securely stored
- OAuth2 authentication for Google Drive
- Encrypted backup file compression

## Troubleshooting
- Check application logs for detailed error information
- Ensure network connectivity
- Verify database connection strings

## Contributing
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License
MIT License

## Contact
For support, please open an issue in the GitHub repository.
