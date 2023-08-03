using System;
using System.Collections.Generic;
using System.Text;

namespace NarfoxGameTools.Logging
{
    public class ScreenLogger : ILogger
    {
        public LogLevel Level { get; set; } = LogLevel.Debug;

        public ScreenLogger()
        {
            FlatRedBall.Debugging.Debugger.TextCorner = FlatRedBall.Debugging.Debugger.Corner.BottomRight;
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
            // no-op, this logger doesn't save messages
        }

        public void Save()
        {
            // no-op, this logger doesn't save messsages
        }



        void Write(LogLevel level, string msg)
        {
            if (Level <= level)
            {
                msg = $"{level.ToString().ToUpper()}: {msg}";
                FlatRedBall.Debugging.Debugger.CommandLineWrite(msg);
            }
        }

    }
}
