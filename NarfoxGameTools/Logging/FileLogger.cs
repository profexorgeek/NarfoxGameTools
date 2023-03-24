using NarfoxGameTools.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace NarfoxGameTools.Logging
{

    public class FileLogger : ILogger
    {
        internal class LogMessage
        {
            public LogLevel Level { get; set; }
            public DateTime Time { get; set; }
            public string Message { get; set; }
        }

        List<LogMessage> logs = new List<LogMessage>();
        
        public string LogPath { get; set; }
        public int MaxLogFileByteSize { get; set; } = 5/*mb*/ * 1024/*kb*/ * 1024/*b*/;




        public FileLogger()
        {
            var filename = "game.log";
            if (string.IsNullOrEmpty(FileService.Instance.AppName) == false)
            {
                var logname = FileService.Instance.AppName.ToLower();
                filename = $"{logname}.log";
            }
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

        public void Purge()
        {
            logs.Clear();
        }

        public void Save()
        {
            // force a log file to exist
            if(!File.Exists(LogPath))
            {
                var newFile = File.Create(LogPath);
                newFile.Close();
            }

            // determine whether to append to the existing log or back
            // up the current log and start a new one
            var fileLength = new FileInfo(LogPath).Length;
            bool append = true;
            if (fileLength > MaxLogFileByteSize)
            {
                // copy log file to backup location
                var date = DateTime.Now;
                var dateString = $"{date.Year}_{date.Month}_{date.Day}";
                var backupPath = LogPath.Replace(".log", $"{dateString}.log");
                FileService.Instance.CopyFile(LogPath, backupPath);
                append = false;
            }

            // now write each log message to the file
            using (StreamWriter writer = new StreamWriter(LogPath, append))
            {
                for (var i = 0; i < logs.Count; i++)
                {
                    var l = logs[i];
                    var msg = string.Format("{0}\t{1}\t{2}", l.Time, l.Level, l.Message);
                    writer.WriteLine(msg);
                }
            }

            Purge();
        }


        private  void Write(LogLevel level, string message)
        {
            if (level >= Level)
            {
                var msg = new LogMessage()
                {
                    Level = level,
                    Time = DateTime.Now,
                    Message = message
                };
                logs.Add(msg);
            }
        }
    }
}
