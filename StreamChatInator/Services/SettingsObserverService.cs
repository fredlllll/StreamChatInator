using Open.Observable;
using StreamChatInator.Database;
using System.Reactive;

namespace StreamChatInator.Services
{
    public abstract class SettingsObserverService
    {
        protected ObservableValue<string?> Value { get; } = new ObservableValue<string?>(null);
        private readonly IServiceScope _scope;
        private readonly DatabaseContext _db;
        private readonly string _settingName;
        // DbContext is not thread-safe, and several singletons call these
        // getters/setters concurrently (e.g. at startup); all database access
        // must happen under this lock.
        private readonly object _dbLock = new();
        public SettingsObserverService(string settingName, IServiceScopeFactory scopeFactory)
        {
            //we are cheating a bit here. we dont strictly need a scope, but we need a database context, and i dont see the sense in creating a new instance for every request. and to make asp.net happy, we just create a scope here once and use the db context from that.
            //if this ever breaks, just create a new scope and db context every time you need it
            _scope = scopeFactory.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            _settingName = settingName;
        }

        protected string? GetValue()
        {
            if (!string.IsNullOrWhiteSpace(Value.Value))
            {
                return Value.Value;
            }

            lock (_dbLock)
            {
                // Another thread may have populated the cache while we waited.
                if (!string.IsNullOrWhiteSpace(Value.Value))
                {
                    return Value.Value;
                }

                var val = _db.GetSettingsValueOrNull(_settingName);
                Value.Post(val);
                return val;
            }
        }

        protected void SetValue(string value)
        {
            Value.Post(value);
            lock (_dbLock)
            {
                _db.SetSettingsValue(_settingName, value);
                _db.SaveChanges();
            }
        }

        protected void UnsetValue()
        {
            Value.Post(null);
            lock (_dbLock)
            {
                _db.UnsetSettingsValue(_settingName);
                _db.SaveChanges();
            }
        }

        public async Task WaitOnValueAsync(CancellationToken stoppingToken)
        {
            //TODO: i assume the await will throw if this is canceled. no idea if that is actually true
            var completionSource = new TaskCompletionSource();
            using var tokenRegistration = stoppingToken.Register(() =>
            {
                completionSource.SetCanceled(stoppingToken);
            });
            using var observerRegistration = Value.Subscribe(Observer.ToObserver<string?>(_ =>
            {
                completionSource.SetResult();
            }));
            await completionSource.Task;
        }

        /// <summary>
        /// Waits until the stored value differs from <paramref name="knownValue"/>.
        /// Completes immediately when it already differs.
        /// </summary>
        public async Task WaitOnChangeFromAsync(string? knownValue, CancellationToken stoppingToken)
        {
            // Subscribing replays the currently cached value, so the filter
            // must ignore it - otherwise every wait would complete instantly
            // and this method would degrade into a busy-loop.
            var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var tokenRegistration = stoppingToken.Register(() => completionSource.SetCanceled(stoppingToken));
            using var observerRegistration = Value.Subscribe(Observer.ToObserver<string?>(notification =>
            {
                var newValue = notification.HasValue ? notification.Value : null;
                if (!string.Equals(newValue, knownValue, StringComparison.Ordinal))
                {
                    completionSource.TrySetResult();
                }
            }));

            if (!string.Equals(GetValue(), knownValue, StringComparison.Ordinal))
            {
                completionSource.TrySetResult();
            }

            await completionSource.Task;
        }
    }
}
