using NarfoxGameTools.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NarfoxGameTools.Logging
{

    public class FileLogger : ILogger
    {
        const string filename = "error.log";
        public string LogPath { get; set; }
        public FileLogger()
        {
            LogPath = Path.Combine(FileService.Instance.DefaultSaveDirectory, filename);
        }

        public  LogLevel Level { get; set; } = LogLevel.Debug;

        public  void Debug(string message)
        {
            Write(LogLevel.Debug, message);
        }

        public  void Info(string message)
        {
            Write(LogLevel.Info, message);
        }

        public  void Warn(string message)
        {
            Write(LogLevel.Warn, message);
        }

        public  void Error(string message)
        {
            Write(LogLevel.Error, message);
        }

        private  void Write(LogLevel level, string message)
        {
            if (level >= Level)
            {
                var dateString = DateTime.Now.ToString("g");
                var msg = $"{dateString}\t{level}\t{message}";
                using (var writer = File.AppendText(LogPath))
                {
                    writer.WriteLine(msg);
                }
            }
        }
    }
}
