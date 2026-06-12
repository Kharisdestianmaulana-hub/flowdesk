using System;
using System.IO;

namespace FlowDesk.Infrastructure.Services;

public class ExceptionLogger
{
    private readonly string _logsFolder;

    public ExceptionLogger()
    {
        var dataFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlowDeskData");
        _logsFolder = Path.Join(dataFolder, "logs");
    }

    public void LogException(Exception ex)
    {
        try
        {
            if (!Directory.Exists(_logsFolder))
                Directory.CreateDirectory(_logsFolder);

            var logFile = Path.Join(_logsFolder, "error_log.txt");
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logMessage = $"[{timestamp}] {ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}--------------------------------------------------{Environment.NewLine}";

            File.AppendAllText(logFile, logMessage);
        }
        catch
        {
            // Failsafe: if we can't write to the log, swallow so we don't crash the crash handler
        }
    }
}
