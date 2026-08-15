namespace StreamChatInator
{
    /// <summary>Logging provider that feeds ASP.NET log output into <see cref="ConsoleUi"/>'s log area.</summary>
    public sealed class ConsoleUiLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new Logger(categoryName);
        public void Dispose() { }

        private sealed class Logger : ILogger
        {
            private readonly string _category;

            public Logger(string categoryName) => _category = categoryName;

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter(state, exception);
                var color = logLevel switch
                {
                    LogLevel.Critical => "\x1b[95m",
                    LogLevel.Error => "\x1b[91m",
                    LogLevel.Warning => "\x1b[93m",
                    LogLevel.Debug or LogLevel.Trace => "\x1b[90m",
                    _ => "",
                };
                ConsoleUi.WriteLogLine($"{DateTime.Now:HH:mm:ss} {color}{ShortName(logLevel)} {ShortCategory()}: {message}\x1b[0m");
                if (exception is not null)
                {
                    ConsoleUi.WriteLogLine($"{DateTime.Now:HH:mm:ss} \x1b[91m{exception}\x1b[0m");
                }
            }

            private string ShortCategory()
            {
                var idx = _category.LastIndexOf('.');
                return idx >= 0 ? _category[(idx + 1)..] : _category;
            }

            private static string ShortName(LogLevel level) => level switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
                _ => "none",
            };
        }
    }
}