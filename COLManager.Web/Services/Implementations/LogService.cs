using System.Text;
using System.Text.RegularExpressions;

namespace COLManager.Web.Services.Implementations;

public class LogService : ILogService
{
    private readonly ILogger<LogService> _logger;
    private readonly IHostEnvironment _environment;
    private readonly string _logDirectory;
    private readonly string _logFilePattern;

    public LogService(ILogger<LogService> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;

        _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }

        _logFilePattern = "COLManager-*.log";
    }

    public async Task<IEnumerable<LogEntry>> GetAllLogsAsync()
    {
        try
        {
            return await ReadLogsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading logs");
            return Enumerable.Empty<LogEntry>();
        }
    }

    public async Task<IEnumerable<LogEntry>> GetLogsByDateAsync(DateTime date)
    {
        try
        {
            var logs = await ReadLogsAsync();
            return logs.Where(l => l.Timestamp.Date == date.Date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering logs by date");
            return Enumerable.Empty<LogEntry>();
        }
    }

    public async Task<IEnumerable<LogEntry>> GetLogsByLevelAsync(string level)
    {
        try
        {
            var logs = await ReadLogsAsync();
            return logs.Where(l => l.Level.Equals(level, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering logs by level");
            return Enumerable.Empty<LogEntry>();
        }
    }

    public Task<bool> ClearLogsAsync()
    {
        try
        {
            // Close Serilog file handles
            Serilog.Log.CloseAndFlush();

            var files = Directory.GetFiles(_logDirectory, _logFilePattern);

            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

            // Optional: recreate empty folder structure
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            _logger.LogInformation("All log files deleted");

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing logs");
            return Task.FromResult(false);
        }
    }

    public async Task<int> GetLogCountAsync()
    {
        try
        {
            var logs = await ReadLogsAsync();
            return logs.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting log count");
            return 0;
        }
    }

    public async Task<byte[]> ExportLogsAsync()
    {
        try
        {
            var files = Directory.GetFiles(_logDirectory, _logFilePattern)
                                 .OrderByDescending(f => f);

            var builder = new StringBuilder();

            foreach (var file in files)
            {
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);

                var content = await reader.ReadToEndAsync();
                builder.AppendLine(content);
            }

            return Encoding.UTF8.GetBytes(builder.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting logs");
            return Array.Empty<byte>();
        }
    }

    public IEnumerable<string> GetAvailableLogLevels()
    {
        return new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" };
    }

    private async Task<IEnumerable<LogEntry>> ReadLogsAsync()
    {
        var logs = new List<LogEntry>();

        var logFiles = Directory.GetFiles(_logDirectory, _logFilePattern)
                                .OrderByDescending(f => f);

        foreach (var file in logFiles)
        {
            try
            {
                using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream);

                string? line;
                LogEntry? currentEntry = null;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var entry = ParseLogLine(line);

                    if (entry != null)
                    {
                        if (currentEntry != null)
                        {
                            logs.Add(currentEntry);
                        }

                        currentEntry = entry;
                    }
                    else if (currentEntry != null)
                    {
                        currentEntry.Exception += Environment.NewLine + line;
                    }
                }

                if (currentEntry != null)
                {
                    logs.Add(currentEntry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading log file {File}", file);
            }
        }

        return logs.OrderByDescending(l => l.Timestamp);
    }

    private LogEntry? ParseLogLine(string line)
    {
        try
        {
            var pattern = @"(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3})\s\[(\w+)\]\s\[([^\]]+)\]\s(.*)";
            var match = Regex.Match(line, pattern);

            if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var timestamp))
            {
                return new LogEntry
                {
                    Timestamp = timestamp,
                    Level = match.Groups[2].Value,
                    Logger = match.Groups[3].Value,
                    Message = match.Groups[4].Value,
                    RawLine = line
                };
            }

            if (line.StartsWith("["))
            {
                var bracketEnd = line.IndexOf("]");
                if (bracketEnd > 0)
                {
                    var timeStr = line.Substring(1, bracketEnd - 1);

                    if (DateTime.TryParse(timeStr, out var parsedTimestamp))
                    {
                        var remainder = line.Substring(bracketEnd + 1).TrimStart();

                        return new LogEntry
                        {
                            Timestamp = parsedTimestamp,
                            Level = "Information",
                            Logger = "Application",
                            Message = remainder,
                            RawLine = line
                        };
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}