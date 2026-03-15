// Quick Reference: Using Logging in COL Manager Services

// 1. INJECT LOGGER INTO YOUR SERVICE
public class MyService : IMyService
{
    private readonly ILogger<MyService> _logger;
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
    
    public async Task<ServiceResult<T>> DoSomethingAsync()
    {
        try
        {
            _logger.LogInformation("Starting operation: DoSomething");
            
            // Your business logic here
            
            _logger.LogInformation("Operation completed successfully");
            return ServiceResult<T>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in DoSomething operation");
            return ServiceResult<T>.Failure("An error occurred");
        }
    }
}

// 2. LOG LEVELS AND WHEN TO USE THEM

_logger.LogTrace("Very detailed diagnostic information");           // Most verbose, usually disabled
_logger.LogDebug("Diagnostic information for developers");          // Development debugging
_logger.LogInformation("General informational messages");            // Important state changes
_logger.LogWarning("Warning: Something unexpected but recoverable"); // Potential issues
_logger.LogError(ex, "Error: Something went wrong");                 // Errors that need attention
_logger.LogCritical(ex, "Critical: System failure");                 // System failures

// 3. ACCESSING LOGS IN APPLICATION

// Navigate to: http://yourapp/logs
// Or click "Logs" in the navigation menu

// 4. LOG FILE LOCATION

// File: {ApplicationRoot}/Logs/application.log
// Accessible via UI at: /logs page
// Daily rolling files: application.log.20250314, etc.

// 5. CURRENT EXAMPLES IN CODE

// From ColumnService.cs
_logger.LogInformation("Column created successfully: {ColumnName}", columnMasterCreateDto.ColumnName);

// From AuditService.cs
_logger.LogInformation("Audit entry created for table: {TableName}", audit.TableName);

// 6. VIEWING LOGS

// Features available:
// - View all logs with timestamps
// - Filter by date
// - Filter by severity level (Info, Warning, Error, etc.)
// - View full exception details
// - Export logs as text file
// - Clear all logs
// - See total log count

// 7. COMMON PATTERNS

// Pattern 1: Logging state changes
_logger.LogInformation("Column {ColumnID} status changed from {OldStatus} to {NewStatus}", 
    columnId, oldStatus, newStatus);

// Pattern 2: Logging with exceptions
try
{
    // Some code
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to save column: {ColumnName}", column.ColumnName);
    throw;
}

// Pattern 3: Logging method entry/exit
_logger.LogDebug("Entering method GetColumnAsync with ID: {ColumnID}", columnId);
// ... method logic ...
_logger.LogDebug("Exiting method GetColumnAsync");

// 8. DEVELOPER WORKFLOW

// 1. Run the application
// 2. Perform actions that generate logs
// 3. Navigate to /logs page
// 4. Review logs in real-time
// 5. Filter as needed
// 6. Export for analysis if needed
// 7. Clear old logs when appropriate

// 9. IMPORTANT NOTES

// - Log file is at: Logs/application.log (relative to application root)
// - Logs are created automatically
// - Daily rolling files kept for 30 days
// - Each file limited to 10 MB
// - Minimum log level set to Information (debug logs not shown unless changed in Program.cs)
// - File is accessible even while application is running (shared file access)
