using Microsoft.Extensions.Logging;

namespace FB_App.Application.Common.Extensions;

public static class ILoggerExtensions
{
    extension(ILogger logger)
    {
        public void LogIfLevel(LogLevel logLevel = LogLevel.Error, string message = "", params object?[] args)
        {
            if (logger.IsEnabled(logLevel))
            {
                logger.Log(logLevel: logLevel, message: message, args: args);
            }
        }
    }
}
