using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;

namespace StreamChatInator
{
    /// <summary>
    /// Splits the console into a fixed info panel (top) and a scrolling log area
    /// (bottom) using an ANSI scroll region. When the console is redirected or
    /// the terminal is too limited it stays disabled and the normal console
    /// logger is used instead.
    /// </summary>
    public static class ConsoleUi
    {
        private const int MinWidth = 60;
        private const int MinHeight = 14;
        private const int PanelHeight = 8;           // rows 0..7
        private const int StatusRow = 4;
        private const int LogStartRow = PanelHeight;  // first log row (0-based)
        private const int HistoryLimit = 200;

        private static readonly object Sync = new();
        private static readonly ConcurrentQueue<string> History = new();
        private static CancellationTokenSource? _cts;
        private static string _status = "";
        private static string _baseUrl = "";
        private static string _url = "";
        private static string _pin = "";
        private static int _bufferWidth;
        private static int _bufferHeight;
        private static int _logRow = LogStartRow;
        private static bool _enabled;

        public static bool IsEnabled => _enabled;

        /// <summary>Draws the console UI and arms the log scroll region. Returns false when the fancy UI can't be used (e.g. output is redirected).</summary>
        public static bool Init(string title, string url)
        {
            _baseUrl = url;
            _url = url;
            try
            {
                if (Console.IsOutputRedirected) return false;

                Console.Title = title;
                Console.OutputEncoding = Encoding.UTF8;
                EnableVirtualTerminal();
                Console.CursorVisible = false;

                _cts = new CancellationTokenSource();
                ResizeBufferToWindow();
                lock (Sync)
                {
                    Redraw();
                    _enabled = true;
                    foreach (var line in History)
                    {
                        WriteLineInternal(line);
                    }
                }
                StartResizeWatcher();
                return true;
            }
            catch
            {
                _enabled = false;
                return false;
            }
        }

        /// <summary>Restores the console before the app exits.</summary>
        public static void Shutdown()
        {
            _cts?.Cancel();
            lock (Sync)
            {
                if (!_enabled) return;
                try
                {
                    Console.Write("\x1b[r");
                    Console.CursorVisible = true;
                    Console.ResetColor();
                }
                catch { }
            }
        }

        /// <summary>Appends a line to the log area (thread-safe).</summary>
        public static void WriteLogLine(string line)
        {
            if (!_enabled) return;
            History.Enqueue(line);
            while (History.Count > HistoryLimit) History.TryDequeue(out _);

            RunLocked(() =>
            {
                foreach (var part in line.Replace("\r\n", "\n").Split('\n'))
                {
                    WriteLineInternal(part);
                }
            });
        }

        /// <summary>Updates the status line inside the info panel (thread-safe).</summary>
        public static void SetStatus(string status)
        {
            RunLocked(() =>
            {
                _status = status;
                var inner = Math.Max(1, _bufferWidth - 4);
                var text = "Status: " + status;
                if (text.Length > inner) text = text[..inner];
                Console.SetCursorPosition(2, StatusRow);
                Console.Write(text.PadRight(inner));
                PositionCursorToLogRow();
            });
        }

        /// <summary>
        /// Shows the LAN access PIN on its own line in the info panel (thread-safe).
        /// Unlike the log area, this line never scrolls away, so the streamer can
        /// always read the PIN. Pass null/empty to clear it.
        /// </summary>
        /// <remarks>
        /// The "Open:" link reacts to this dynamically: when a PIN is set it gets
        /// appended as <c>?pin=…</c> so anyone who clicks/copies the link unlocks
        /// the UI without typing the PIN, and it drops back to the bare URL when
        /// the PIN is cleared.
        /// </remarks>
        public static void SetPin(string? pin)
        {
            RunLocked(() =>
            {
                _pin = pin ?? "";
                _url = string.IsNullOrEmpty(_pin) ? _baseUrl : $"{_baseUrl}?pin={_pin}";
                DrawPanel();
                PositionCursorToLogRow();
            });
        }

        /// <summary>
        /// Runs <paramref name="action"/> while holding <see cref="Sync"/>, but
        /// only when the UI is enabled. The double <c>_enabled</c> check lets a
        /// caller bail before ever contending on the lock, while still guarding
        /// against a shutdown that races in between.
        /// </summary>
        private static void RunLocked(Action action)
        {
            if (!_enabled) return;
            lock (Sync)
            {
                if (!_enabled) return;
                action();
            }
        }

        private static void ResizeBufferToWindow()
        {
            var width = Math.Max(Console.WindowWidth, MinWidth);
            var height = Math.Max(Console.WindowHeight, MinHeight);
            // The buffer can only be resized on Windows; on macOS/Linux the buffer
            // always matches the window size.
            if (OperatingSystem.IsWindows())
            {
                if (Console.BufferWidth != width) Console.BufferWidth = width;
                if (Console.BufferHeight != height) Console.BufferHeight = height;
            }
            _bufferWidth = width;
            _bufferHeight = height;
            if (_logRow < LogStartRow) _logRow = LogStartRow;
            if (_logRow >= _bufferHeight) _logRow = _bufferHeight - 1;
        }

        private static void Redraw()
        {
            Console.Write("\x1b[r");   // reset scroll region
            Console.Clear();
            DrawPanel();
            SetScrollRegion();
            PositionCursorToLogRow();
        }

        private static void DrawPanel()
        {
            var w = _bufferWidth;
            Console.SetCursorPosition(0, 0);
            Console.Write("╔" + new string('═', Math.Max(0, w - 2)) + "╗");
            WriteRow(1, "StreamChatInator");
            WriteRow(2, string.IsNullOrEmpty(_pin) ? "" : "PIN: " + _pin);
            WriteRow(3, "Open: " + _url);
            WriteRow(4, "Status: " + _status);
            WriteRow(5, "");
            WriteRow(6, "Close this window to stop the app.");
            Console.SetCursorPosition(0, 7);
            Console.Write("╚" + new string('═', Math.Max(0, w - 2)) + "╝");
        }

        private static void WriteRow(int row, string text)
        {
            var inner = Math.Max(1, _bufferWidth - 4);
            if (text.Length > inner) text = text[..inner];
            Console.SetCursorPosition(0, row);
            Console.Write("║  " + text.PadRight(inner) + "  ║");
        }

        private static void SetScrollRegion()
        {
            var start = LogStartRow + 1; // 1-based
            var end = Math.Max(_bufferHeight, start);
            Console.Write($"\x1b[{start};{end}r");
        }

        private static void WriteLineInternal(string line)
        {
            if (_logRow < LogStartRow) _logRow = LogStartRow;
            if (_logRow >= _bufferHeight)
            {
                // Area full: scroll the log region up one row, then write at the bottom.
                Console.SetCursorPosition(0, _bufferHeight - 1);
                Console.Write("\n");
                _logRow = _bufferHeight - 1;
            }

            var width = Math.Max(1, _bufferWidth);
            if (line.Length > width) line = line[..width] + "\x1b[0m";
            Console.SetCursorPosition(0, _logRow);
            Console.Write(line);
            _logRow++;
            PositionCursorToLogRow();
        }

        private static void PositionCursorToLogRow()
        {
            var row = _logRow >= _bufferHeight ? _bufferHeight - 1 : Math.Max(LogStartRow, _logRow);
            Console.SetCursorPosition(0, row);
        }

        private static void StartResizeWatcher()
        {
            var token = _cts!.Token;
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(300, token).ConfigureAwait(false);
                        int w, h;
                        lock (Sync) { w = _bufferWidth; h = _bufferHeight; }
                        if (Console.WindowWidth != w || Console.WindowHeight != h)
                        {
                            lock (Sync)
                            {
                                ResizeBufferToWindow();
                                Redraw();
                                foreach (var line in History)
                                {
                                    WriteLineInternal(line);
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch { }
                }
            }, token);
        }

        private static void EnableVirtualTerminal()
        {
            // macOS/Linux terminals support ANSI escape sequences natively.
            if (!OperatingSystem.IsWindows()) return;

            var handle = GetStdHandle(StdOutputHandle);
            if (!GetConsoleMode(handle, out var mode)) throw new InvalidOperationException("GetConsoleMode failed");
            if (!SetConsoleMode(handle, mode | EnableVirtualTerminalProcessing)) throw new InvalidOperationException("SetConsoleMode failed");
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private const int StdOutputHandle = -11;
        private const uint EnableVirtualTerminalProcessing = 0x0004;
    }

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
