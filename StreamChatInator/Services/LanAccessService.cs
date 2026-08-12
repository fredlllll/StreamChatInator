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
        private static readonly TimeSpan IdleRetention = TimeSpan.FromHours(1);
        // Bucket used when the client IP can't be determined (RemoteIpAddress is
        // null); all such clients share one bucket rather than sharing with real IPs.
        private const string UnknownIpKey = "unknown";

        private readonly object _lock = new();
        private readonly Dictionary<string, AttemptState> _attemptsByIp = new();

        private sealed class AttemptState
        {
            public int FailedAttempts;
            public DateTime LockedUntilUtc = DateTime.MinValue;
            public DateTime LastFailureUtc = DateTime.MinValue;
        }

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

        public bool IsLockedOut(string clientIp)
        {
            lock (_lock)
            {
                PruneExpired();
                var key = NormalizeIp(clientIp);
                return _attemptsByIp.TryGetValue(key, out var state)
                    && DateTime.UtcNow < state.LockedUntilUtc;
            }
        }

        public void RegisterFailure(string clientIp)
        {
            lock (_lock)
            {
                PruneExpired();
                var key = NormalizeIp(clientIp);
                if (!_attemptsByIp.TryGetValue(key, out var state))
                {
                    state = new AttemptState();
                    _attemptsByIp[key] = state;
                }
                state.FailedAttempts++;
                state.LastFailureUtc = DateTime.UtcNow;
                if (state.FailedAttempts >= MaxFailedAttempts)
                {
                    state.LockedUntilUtc = DateTime.UtcNow + LockoutDuration;
                    state.FailedAttempts = 0;
                }
            }
        }

        public void ResetFailures(string clientIp)
        {
            lock (_lock)
            {
                var key = NormalizeIp(clientIp);
                if (_attemptsByIp.TryGetValue(key, out var state))
                {
                    state.FailedAttempts = 0;
                    state.LockedUntilUtc = DateTime.MinValue;
                }
            }
        }

        private static string NormalizeIp(string? clientIp)
        {
            return string.IsNullOrEmpty(clientIp) ? UnknownIpKey : clientIp;
        }

        /// <summary>
        /// Drops buckets that no longer matter so the dictionary can't grow
        /// unbounded: resolved lockouts, successful logins (failures reset to
        /// zero), and abandoned partial-count buckets that have been idle long
        /// enough that their half-count is no longer relevant.
        /// </summary>
        private void PruneExpired()
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _attemptsByIp.ToList())
            {
                var state = kvp.Value;
                var lockoutDone = state.LockedUntilUtc <= now;
                var idle = now - state.LastFailureUtc > IdleRetention;
                if (lockoutDone && (state.FailedAttempts == 0 || idle))
                {
                    _attemptsByIp.Remove(kvp.Key);
                }
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
