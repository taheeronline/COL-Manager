# Application Logging Viewer - Implementation Guide

## Overview
A comprehensive logging viewer has been implemented in the COL Manager application, allowing developers and users to view, filter, and manage application logs directly from the UI without needing to know the log file path.

## Features Implemented

### 1. **Centralized Logging Service**
- **File Location**: `Logs/application.log` (created automatically in the application root directory)
- **Automatic Creation**: Log directory and file are created automatically on first run
- **File Logging**: Using Serilog for enterprise-grade logging with daily rolling files
- **Log Retention**: Automatically maintains 30 days of log history
- **File Size Management**: Each log file is limited to 10 MB before rolling

### 2. **Log Viewer Page** (`/logs`)
The dedicated logging page provides:

#### Display & Filtering
- **View All Logs**: Shows all application logs with timestamps, severity levels, logger source, and messages
- **Filter by Date**: Select a specific date to view logs from that day
- **Filter by Level**: Filter logs by severity (Trace, Debug, Information, Warning, Error, Critical)
- **Color-Coded Severity**: 
  - Error = Red highlighting
  - Warning = Yellow highlighting
  - Critical = Dark highlighting
  - Information/Debug/Trace = Standard text

#### Log Management
- **View Exception Details**: Click "Details" on any log entry with exceptions to view full stack traces
- **Export Logs**: Download logs as a text file for external analysis
- **Clear Logs**: Clear all log entries with a confirmation dialog
- **Log Count**: See total number of log entries at a glance
- **Refresh**: Manually refresh the log list

### 3. **Navigation Integration**
- New "Logs" menu item added to the main navigation menu
- Icon: Document/File icon (bi bi-file-text)
- Easy access from any page in the application

### 4. **Log Parser**
The LogService intelligently parses log entries with support for:
- Standard Serilog format: `[Timestamp] [Level] [Logger] Message`
- Exception details and stack traces (multi-line support)
- Lenient parsing for various log formats
- Timestamp extraction and sorting (newest first)

## Technical Implementation

### Configuration

**Serilog Setup (Program.cs)**:
```csharp
// Logs are written to: {CurrentDirectory}/Logs/application.log
// Format: {Timestamp} [{Level}] [{SourceContext}] {Message}{Exception}
// Daily rolling files with 30-day retention
// Individual file size limit: 10 MB
```

**Dependencies Added**:
- Serilog (4.2.0) - Core logging framework
- Serilog.AspNetCore (9.0.0) - ASP.NET Core integration
- Serilog.Sinks.File (6.0.0) - File logging sink

### Services

**ILogService Interface** (`Services/Interfaces/ILogService.cs`):
- `GetAllLogsAsync()` - Retrieve all logs
- `GetLogsByDateAsync(DateTime)` - Filter by date
- `GetLogsByLevelAsync(string)` - Filter by severity level
- `ClearLogsAsync()` - Clear all logs
- `GetLogCountAsync()` - Get total log count
- `ExportLogsAsync()` - Export logs as byte array
- `GetAvailableLogLevels()` - Get available severity levels

**LogService Implementation** (`Services/Implementations/LogService.cs`):
- Reads logs with concurrent file access support
- Parses complex log formats
- Handles multi-line exceptions
- Returns results ordered by timestamp (newest first)

### UI Components

**Logs.razor Page** (`Pages/Logs.razor`):
- Responsive table layout
- Bootstrap styling for consistency
- Modal dialogs for exception details and clear confirmation
- Real-time filtering and searching
- Toast notifications for user feedback

### JavaScript Helpers

Added to `wwwroot/js/site.js`:
- `downloadFile(data, filename)` - Download exported logs as a text file

## How to Use

### Access Logs
1. Click "Logs" in the navigation menu
2. The page loads all available logs automatically

### Filter Logs
1. **By Date**: Select a date from the date picker and click "Filter"
2. **By Level**: Select severity level and click "Filter"
3. **Both**: Apply date filter, then level filter for more granular results
4. **Reset**: Select empty level and click "Filter" to see all logs

### View Exception Details
1. Find a log entry with a severity level of Error or Critical
2. Click the "Details" link in the Message column
3. A modal opens showing full exception details and stack trace

### Export Logs
1. Click "Export Logs" button
2. A `application-logs.txt` file is automatically downloaded
3. Open in any text editor for further analysis

### Clear Logs
1. Click "Clear All Logs" button
2. Confirmation modal appears
3. Confirm the action to clear all log entries
4. Page refreshes automatically after clearing

## Log File Location

The log file is stored at:
```
{ApplicationRoot}/Logs/application.log
```

Example on Windows: `C:\Shams\NetCoreLearning\COL Manager\Logs\application.log`

Daily rolling files will be created as: `application.log.20250314`, etc.

## Logging Best Practices

### In Application Code
```csharp
// Inject ILogger<T> into your services
private readonly ILogger<MyService> _logger;

// Log different levels
_logger.LogInformation("Information message");
_logger.LogWarning("Warning message");
_logger.LogError(ex, "Error message with exception");
```

### Current Configuration
- **Minimum Level**: Information (Trace and Debug are filtered out)
- **Console**: Logs are also output to console
- **File**: Logs are persisted to file for later review

## Benefits

✅ **No Path Memorization**: Users don't need to remember log file locations
✅ **Real-time Viewing**: View logs through the UI without external tools
✅ **Advanced Filtering**: Filter by date, severity, or both
✅ **Exception Details**: Full stack traces visible in detail modal
✅ **Export Capability**: Export logs for archival or external analysis
✅ **Clean Management**: Clear old logs when needed
✅ **User-Friendly**: Intuitive UI with color coding and badges
✅ **Audit Trail**: Complete application activity log for debugging and compliance

## Future Enhancements

Potential improvements that could be added:
- Search/keyword filtering
- Log persistence in database
- Scheduled log archival
- Email notifications for errors
- Log download as CSV or JSON
- Real-time log streaming
- Performance metrics based on logs
