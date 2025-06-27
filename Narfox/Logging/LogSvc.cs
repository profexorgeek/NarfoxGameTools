
using Narfox.Logging;
using Narfox.Services;

namespace Masteroid.Core.Services;

public class LogSvc : ILogger
{
    internal class LogMessage
    {
        public LogLevel Level { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }
    
    private static LogSvc _instance;
    private static readonly Object _padlock = new Object();
    private List<LogMessage> _logs;
    private FileSvc _fileSvc;
    public string LogPath { get; set; }
    public int LogsBeforeAutoFlush { get; set; } = 50;

    public static LogSvc I
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("LogSvc has not been initialized!");
            }

            return _instance;
        }
    }

    public LogLevel Level { get; set; } = LogLevel.Debug;
    
    
    private LogSvc(FileSvc fileSvc)
    {
        _logs = new List<LogMessage>();
        _fileSvc = fileSvc;

        if (string.IsNullOrEmpty(LogPath))
        {
            LogPath = Path.Combine(_fileSvc.DefaultSaveDirectory, $"{_fileSvc.AppName}.log");
        }
    }

    public static void Initialize(FileSvc fileSvc)
    {
        lock (_padlock)
        {
            if (_instance == null)
            {
                _instance = new LogSvc(fileSvc);
            }
        }
    }
    
    public void Debug(string msg)
    {
        Write(LogLevel.Debug, msg);
    }

    public void Info(string msg)
    {
        Write(LogLevel.Info, msg);
    }

    public void Warn(string msg)
    {
        Write(LogLevel.Warn, msg);
    }

    public void Error(string msg)
    {
        Write(LogLevel.Error, msg);
    }

    public void Purge()
    {
        _logs.Clear();
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(LogPath))
        {
            System.Diagnostics.Debug.Assert(false, $"{LogPath} was null or empty!");
            return;
        }

        if (File.Exists(LogPath) == false)
        {
            File.CreateText(LogPath).Close();
        }

        using (StreamWriter sw = new StreamWriter(LogPath, true))
        {
            for (var i = 0; i < _logs.Count; i++)
            {
                var log = _logs[i];
                var msg = $"{log.Level.ToString().ToUpper()}|{log.Time.ToShortDateString()}|{log.Time.ToShortTimeString()}|{log.Message}";
                sw.WriteLine(msg);
            }
        }
        
        Purge();
    }

    void Write(LogLevel level, string msg)
    {
        if (Level <= level)
        {
            var consoleMsg = $"{level.ToString().ToUpper()}: {msg}";
            Console.WriteLine(consoleMsg);
            
            _logs.Add(new LogMessage()
            {
                Level = level,
                Time = DateTime.Now,
                Message = msg.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "--"),
            });

            if (_logs.Count > LogsBeforeAutoFlush)
            {
                Save();
            }
        }
    }
}