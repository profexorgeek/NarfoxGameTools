namespace Narfox.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// This is the minimum level the logger should write at.
        /// Log messages below this level should generally be swallowed.
        /// </summary>
        LogLevel Level { get; set; }

        /// <summary>
        /// Write a debug log message.
        /// </summary>
        /// <param name="msg">The message to log</param>
        void Debug(string msg);

        /// <summary>
        /// Write an info log message
        /// </summary>
        /// <param name="msg">The message to log</param>
        void Info(string msg);

        /// <summary>
        /// Write a warning log message
        /// </summary>
        /// <param name="msg">The message to log</param>
        void Warn(string msg);

        /// <summary>
        /// Write an error log message
        /// </summary>
        /// <param name="msg">The message to log</param>
        void Error(string msg);

        /// <summary>
        /// Purge any running collection of logs
        /// </summary>
        void Purge();

        /// <summary>
        /// Save the log to a file or some other type of
        /// persistance layer. May be a no-op for many types
        /// of loggers
        /// </summary>
        void Save();
    }
}
