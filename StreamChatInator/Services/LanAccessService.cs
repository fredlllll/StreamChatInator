using System.Security.Cryptography;

namespace StreamChatInator.Services
{
    /// <summary>
    /// Holds the shared LAN-access PIN and whether gating is enabled. When
    /// enabled (the default), browsers must present a session cookie obtained
    /// via POST /api/auth/pin-login before any controller or hub call is
    /// allowed. When disabled (e.g. the app is behind a VPN or an nginx TLS
    /// reverse proxy that already gates access), every request passes.
    /// </summary>
    public class LanAccessService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromSeconds(30);

        private readonly object _lock = new();
        private int _failedAttempts;
        private DateTime _lockedUntilUtc = DateTime.MinValue;

        public LanAccessService(IConfiguration config)
        {
            Enabled = config.GetValue("Auth:Enabled", true);
            var configuredPin = config["Auth:Pin"];
            Pin = string.IsNullOrWhiteSpace(configuredPin) ? GeneratePin() : configuredPin.Trim();
        }

        public bool Enabled { get; }
        public string Pin { get; }

        public bool ValidatePin(string? pin)
        {
            if (string.IsNullOrEmpty(pin) || pin.Length != Pin.Length) return false;
            var a = System.Text.Encoding.UTF8.GetBytes(pin);
            var b = System.Text.Encoding.UTF8.GetBytes(Pin);
            return CryptographicOperations.FixedTimeEquals(a, b);
        }

        public bool IsLockedOut()
        {
            lock (_lock)
            {
                return DateTime.UtcNow < _lockedUntilUtc;
            }
        }

        public void RegisterFailure()
        {
            lock (_lock)
            {
                _failedAttempts++;
                if (_failedAttempts >= MaxFailedAttempts)
                {
                    _lockedUntilUtc = DateTime.UtcNow + LockoutDuration;
                    _failedAttempts = 0;
                }
            }
        }

        public void ResetFailures()
        {
            lock (_lock)
            {
                _failedAttempts = 0;
                _lockedUntilUtc = DateTime.MinValue;
            }
        }

        private static string GeneratePin()
        {
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);
            var value = BitConverter.ToUInt32(bytes) % 1_000_000u;
            return value.ToString("D6");
        }
    }
}
