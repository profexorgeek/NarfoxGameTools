namespace NarfoxGameTools.Logging
{
    public class NullLogger : ILogger
    {
        public LogLevel Level { get; set; } = LogLevel.Debug;

        public void Debug(string msg)
        {
            // swallow
        }

        public void Error(string msg)
        {
            // swallow
        }

        public void Info(string msg)
        {
            // swallow
        }

        public void Warn(string msg)
        {
            // swallow
        }

        public void Purge()
        {
            // no-op
        }

        public void Save()
        {
            // no-op
        }
    }
}
