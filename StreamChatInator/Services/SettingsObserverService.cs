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
            if (string.IsNullOrWhiteSpace(Value.Value))
            {
                var val = _db.GetSettingsValueOrNull(_settingName);
                Value.Post(val);
                return val;
            }
            else
            {
                return Value.Value;
            }
        }

        protected void SetValue(string token)
        {
            Value.Post(token);
            _db.SetSettingsValue(_settingName, token);
        }

        protected void UnsetValue()
        {
            Value.Post(null);
            _db.UnsetSettingsValue(_settingName);
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
    }
}
