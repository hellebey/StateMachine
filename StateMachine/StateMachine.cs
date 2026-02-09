namespace StateMachine;

public interface IStateMachine<TState, TTrigger>
    where TState : Enum
    where TTrigger : Enum
{
    TState State { get; }

    IStateConfigurator<TState, TTrigger> Configure(TState state);

    void Trigger(TTrigger trigger);

    void Trigger<TInboxData>(TTrigger trigger, TInboxData inboxData);

    Task TriggerAsync(TTrigger trigger);

    Task TriggerAsync<TInboxData>(TTrigger trigger, TInboxData inboxData);
}

public class StateMachine<TState, TTrigger>
    : IStateMachine<TState, TTrigger>
    where TState : Enum
    where TTrigger : Enum
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly Action<TriggerResult<TState, TTrigger>>? _changeStateNotifyFunc;

    private readonly Action<TState>? _stateUpdateFunc;

    private Dictionary<TState, IStateConfiguration<TState, TTrigger>> _configurations = new();

    private Dictionary<TState, IStateConfigurator<TState, TTrigger>> _configurators = new();

    public TState State => _state;

    private TState _state { get; set; }

    public StateMachine(
        TState state,
        Action<TState>? stateUpdateFunc = null,
        Action<TriggerResult<TState, TTrigger>>? changeStateNotifyFunc = null)
    {
        _state = state;
        _stateUpdateFunc = stateUpdateFunc;
        _changeStateNotifyFunc = changeStateNotifyFunc;
    }

    public IStateConfigurator<TState, TTrigger> Configure(TState state)
    {
        if (_configurations.ContainsKey(state))
        {
            return _configurators[state];
        }
        else
        {
            var config = new StateConfiguration<TState, TTrigger>();
            _configurations.Add(state, config);
            _configurators.Add(state, config);
            return config;
        }
    }

    public void Trigger(TTrigger trigger)
    {
        Trigger<object>(trigger, null!);
    }

    public void Trigger<TInboxData>(TTrigger trigger, TInboxData inboxData)
    {
        TriggerAsync(trigger, inboxData)
             .GetAwaiter()
             .GetResult();
    }

    public Task TriggerAsync(TTrigger trigger)
    {
        return TriggerAsync<object>(trigger, null!);
    }

    public async Task TriggerAsync<TInboxData>(TTrigger trigger, TInboxData inboxData)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_configurations.TryGetValue(_state, out var configuration))
            {
                var callback = configuration.GetCallback(trigger, inboxData)
                        ?? throw new InvalidOperationException($"Триггер {trigger} не сконфигурирован в статусе {_state}.");
                await (callback?.InvokeAsync(inboxData) ?? Task.CompletedTask);
                if (callback?.ChangeState ?? false)
                {
                    await configuration.OnExitAsyncFunc();
                    var oldState = _state;
                    _state = callback.NewState!;
                    _stateUpdateFunc?.Invoke(_state);
                    if (_configurations.TryGetValue(_state, out configuration))
                    {
                        await configuration.OnEntryAsyncFunc();
                    }
                    var result = new TriggerResult<TState, TTrigger>(trigger, oldState, _state);
                    _changeStateNotifyFunc?.Invoke(result);
                }

                return;
            }

            throw new InvalidOperationException($"Статус {_state} не сконфигурирован.");
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

public record class TriggerResult<TState, TTrigger>(TTrigger Trigger, TState OldState, TState NewState);