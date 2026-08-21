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

        private static readonly object s_sync = new();
        private static readonly ConcurrentQueue<string> s_history = new();
        private static CancellationTokenSource? s_cts;
        private static string s_status = "";
        private static string s_baseUrl = "";
        private static string s_url = "";
        private static string s_pin = "";
        private static int s_bufferWidth;
        private static int s_bufferHeight;
        private static int s_logRow = LogStartRow;
        private static bool s_enabled;

        public static bool IsEnabled => s_enabled;

        /// <summary>Draws the console UI and arms the log scroll region. Returns false when the fancy UI can't be used (e.g. output is redirected).</summary>
        public static bool Init(string title, string url)
        {
            s_baseUrl = url;
            s_url = url;
            try
            {
                if (Console.IsOutputRedirected) return false;

                Console.Title = title;
                Console.OutputEncoding = Encoding.UTF8;
                EnableVirtualTerminal();
                Console.CursorVisible = false;

                s_cts = new CancellationTokenSource();
                ResizeBufferToWindow();
                lock (s_sync)
                {
                    Redraw();
                    s_enabled = true;
                    foreach (var line in s_history)
                    {
                        WriteLineInternal(line);
                    }
                }
                StartResizeWatcher();
                return true;
            }
            catch
            {
                s_enabled = false;
                return false;
            }
        }

        /// <summary>Restores the console before the app exits.</summary>
        public static void Shutdown()
        {
            s_cts?.Cancel();
            lock (s_sync)
            {
                if (!s_enabled) return;
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
            if (!s_enabled) return;
            s_history.Enqueue(line);
            while (s_history.Count > HistoryLimit) s_history.TryDequeue(out _);

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
                s_status = status;
                var inner = Math.Max(1, s_bufferWidth - 4);
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
                s_pin = pin ?? "";
                s_url = string.IsNullOrEmpty(s_pin) ? s_baseUrl : $"{s_baseUrl}?pin={s_pin}";
                DrawPanel();
                PositionCursorToLogRow();
            });
        }

        /// <summary>
        /// Runs <paramref name="action"/> while holding <see cref="s_sync"/>, but
        /// only when the UI is enabled. The double <c>s_enabled</c> check lets a
        /// caller bail before ever contending on the lock, while still guarding
        /// against a shutdown that races in between.
        /// </summary>
        private static void RunLocked(Action action)
        {
            if (!s_enabled) return;
            lock (s_sync)
            {
                if (!s_enabled) return;
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
            s_bufferWidth = width;
            s_bufferHeight = height;
            if (s_logRow < LogStartRow) s_logRow = LogStartRow;
            if (s_logRow >= s_bufferHeight) s_logRow = s_bufferHeight - 1;
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
            var w = s_bufferWidth;
            Console.SetCursorPosition(0, 0);
            Console.Write("╔" + new string('═', Math.Max(0, w - 2)) + "╗");
            WriteRow(1, "StreamChatInator");
            WriteRow(2, string.IsNullOrEmpty(s_pin) ? "" : "PIN: " + s_pin);
            WriteRow(3, "Open: " + s_url);
            WriteRow(4, "Status: " + s_status);
            WriteRow(5, "");
            WriteRow(6, "Close this window to stop the app.");
            Console.SetCursorPosition(0, 7);
            Console.Write("╚" + new string('═', Math.Max(0, w - 2)) + "╝");
        }

        private static void WriteRow(int row, string text)
        {
            var inner = Math.Max(1, s_bufferWidth - 4);
            if (text.Length > inner) text = text[..inner];
            Console.SetCursorPosition(0, row);
            Console.Write("║  " + text.PadRight(inner) + "  ║");
        }

        private static void SetScrollRegion()
        {
            var start = LogStartRow + 1; // 1-based
            var end = Math.Max(s_bufferHeight, start);
            Console.Write($"\x1b[{start};{end}r");
        }

        private static void WriteLineInternal(string line)
        {
            if (s_logRow < LogStartRow) s_logRow = LogStartRow;
            if (s_logRow >= s_bufferHeight)
            {
                // Area full: scroll the log region up one row, then write at the bottom.
                Console.SetCursorPosition(0, s_bufferHeight - 1);
                Console.Write("\n");
                s_logRow = s_bufferHeight - 1;
            }

            var width = Math.Max(1, s_bufferWidth);
            if (line.Length > width) line = line[..width] + "\x1b[0m";
            Console.SetCursorPosition(0, s_logRow);
            Console.Write(line);
            s_logRow++;
            PositionCursorToLogRow();
        }

        private static void PositionCursorToLogRow()
        {
            var row = s_logRow >= s_bufferHeight ? s_bufferHeight - 1 : Math.Max(LogStartRow, s_logRow);
            Console.SetCursorPosition(0, row);
        }

        private static void StartResizeWatcher()
        {
            var token = s_cts!.Token;
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(300, token).ConfigureAwait(false);
                        int w, h;
                        lock (s_sync) { w = s_bufferWidth; h = s_bufferHeight; }
                        if (Console.WindowWidth != w || Console.WindowHeight != h)
                        {
                            lock (s_sync)
                            {
                                ResizeBufferToWindow();
                                Redraw();
                                foreach (var line in s_history)
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
}
