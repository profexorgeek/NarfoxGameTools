using NarfoxGameTools.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NarfoxGameTools.Services
{
    public static class LogService
    {
        private static ILogger log;

        public static ILogger Log
        {
            get
            {
                if(log == null)
                {
                    log = new NullLogger();
                }
                return log;
            }
            set
            {
                log = value;
            }
        }
    }
}
