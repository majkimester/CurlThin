using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CurlThin.HyperPipe
{
    public static class Logging
    {
        public static ILoggerFactory Factory { get; } = new LoggerFactory();

        internal static ILogger GetCurrentClassLogger()
        {
            return Factory.CreateLogger(
                new StackFrame(1).GetMethod().DeclaringType
            );
        }
    }
}