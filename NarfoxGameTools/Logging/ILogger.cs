using System;
using System.Collections.Generic;
using System.Text;

namespace NarfoxGameTools.Logging
{
    public interface ILogger
    {
        LogLevel Level { get; set; }

        void Debug(string msg);
        void Info(string msg);
        void Warn(string msg);
        void Error(string msg);
    }
}
