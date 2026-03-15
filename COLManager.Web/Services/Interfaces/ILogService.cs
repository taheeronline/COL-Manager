namespace COLManager.Web.Services;

public interface ILogService
{
    /// <summary>
    /// Gets all log entries from the log file
    /// </summary>
    Task<IEnumerable<LogEntry>> GetAllLogsAsync();

    /// <summary>
    /// Gets log entries for a specific date
    /// </summary>
    Task<IEnumerable<LogEntry>> GetLogsByDateAsync(DateTime date);

    /// <summary>
    /// Gets log entries with a specific log level
    /// </summary>
    Task<IEnumerable<LogEntry>> GetLogsByLevelAsync(string level);

    /// <summary>
    /// Clears the log file
    /// </summary>
    Task<bool> ClearLogsAsync();

    /// <summary>
    /// Gets the total count of log entries
    /// </summary>
    Task<int> GetLogCountAsync();

    /// <summary>
    /// Exports logs to a file
    /// </summary>
    Task<byte[]> ExportLogsAsync();

    /// <summary>
    /// Gets available log levels
    /// </summary>
    IEnumerable<string> GetAvailableLogLevels();
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Logger { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string RawLine { get; set; } = string.Empty;
}
