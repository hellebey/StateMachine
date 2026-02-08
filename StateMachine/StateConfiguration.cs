namespace StateMachine;

public interface IStateConfiguration<TState, TTrigger>
    where TState : Enum
    where TTrigger : Enum
{
    Func<Task> OnEntryAsyncFunc { get; }

    Func<Task> OnExitAsyncFunc { get; }

    ICallback<TTrigger, TState>? GetCallback(TTrigger trigger, object? inboxData);
}

public interface IStateConfigurator<TState, TTrigger>
    : IStateConfiguratorSync<TState, TTrigger>,
      IStateConfiguratorAsync<TState, TTrigger>
    where TState : Enum
    where TTrigger : Enum
{
}

public interface IStateConfiguratorAsync<TState, TTrigger>
    where TState : Enum
    where TTrigger : Enum
{
    IStateConfigurator<TState, TTrigger> ChangeStateAsync<TInboxData>(TTrigger trigger, TState newState, Func<TInboxData, Task> onExitFunc);

    IStateConfigurator<TState, TTrigger> ChangeStateAsync(TTrigger trigger, TState newState, Func<Task> onExitFunc);

    IStateConfigurator<TState, TTrigger> OnActionAsync<TInboxData>(TTrigger trigger, Func<TInboxData, Task> func);

    IStateConfigurator<TState, TTrigger> OnActionAsync(TTrigger trigger, Func<Task> func);

    IStateConfigurator<TState, TTrigger> OnEntryAsync(Func<Task> onExitFunc);

    IStateConfigurator<TState, TTrigger> OnExitAsync(Func<Task> onExitFunc);
}

public interface IStateConfiguratorSync<TState, TTrigger>
    where TState : Enum
    where TTrigger : Enum
{
    IStateConfigurator<TState, TTrigger> ChangeState<TInboxData>(TTrigger trigger, TState newState, Action<TInboxData> onExitFunc);

    IStateConfigurator<TState, TTrigger> ChangeState(TTrigger trigger, TState newState, Action onExitFunc);

    IStateConfigurator<TState, TTrigger> ChangeState(TTrigger trigger, TState newState);

    IStateConfigurator<TState, TTrigger> NoAction(TTrigger trigger);

    IStateConfigurator<TState, TTrigger> NoAction<TInboxData>(TTrigger trigger);

    IStateConfigurator<TState, TTrigger> OnAction<TInboxData>(TTrigger trigger, Action<TInboxData> func);

    IStateConfigurator<TState, TTrigger> OnAction(TTrigger trigger, Action func);

    IStateConfigurator<TState, TTrigger> OnEntry(Action onEntryFunc);

    IStateConfigurator<TState, TTrigger> OnExit(Action onEntryFunc);
}

internal class StateConfiguration<TState, TTrigger>
    : IStateConfigurator<TState, TTrigger>,
      IStateConfiguration<TState, TTrigger>
    where TState : Enum
    where TTrigger : Enum
{
    private List<Callback<TTrigger, TState>> _callbacks = new();

    public Func<Task>? OnEntryAsyncFunc { get; private set; } = () => Task.CompletedTask;

    public Func<Task>? OnExitAsyncFunc { get; private set; } = () => Task.CompletedTask;

    public IStateConfigurator<TState, TTrigger> ChangeState<TInboxData>(TTrigger trigger, TState newState, Action<TInboxData> callback)
    {
        AddCallback(new CallbackWithDataAsync<TTrigger, TState, TInboxData>(
            trigger: trigger,
            newState: newState,
            callback: (TInboxData inboxData) => Task.Run(() => callback(inboxData))));
        return this;
    }

    public IStateConfigurator<TState, TTrigger> ChangeState(TTrigger trigger, TState newState, Action callback)
    {
        AddCallback(new CallbackWithoutDataAsync<TTrigger, TState>(
           trigger: trigger,
           callback: () => Task.Run(callback),
           newState: newState));
        return this;
    }

    public IStateConfigurator<TState, TTrigger> ChangeState(TTrigger trigger, TState newState)
    {
        AddCallback(new CallbackWithoutDataAsync<TTrigger, TState>(
           trigger: trigger,
           callback: () => Task.CompletedTask,
           newState: newState));
        return this;
    }

    public IStateConfigurator<TState, TTrigger> ChangeStateAsync<TInboxData>(TTrigger trigger, TState newState, Func<TInboxData, Task> callback)
    {
        AddCallback(new CallbackWithDataAsync<TTrigger, TState, TInboxData>(
            trigger: trigger,
            newState: newState,
            callback: callback));
        return this;
    }

    public IStateConfigurator<TState, TTrigger> ChangeStateAsync(TTrigger trigger, TState newState, Func<Task> onExitFunc)
    {
        AddCallback(new CallbackWithoutDataAsync<TTrigger, TState>(
           trigger: trigger,
           callback: onExitFunc,
           newState: newState));
        return this;
    }

    public ICallback<TTrigger, TState>? GetCallback(TTrigger trigger, object? inboxData)
    {
        return _callbacks.SingleOrDefault(c => c.Trigger.Equals(trigger) &&
                                               c.InboxDataType == (inboxData is not null ? inboxData.GetType()
                                                                                         : null));
    }

    public IStateConfigurator<TState, TTrigger> NoAction(TTrigger trigger)
    {
        AddCallback(new CallbackWithoutDataAsync<TTrigger, TState>(
            trigger: trigger,
            callback: () => Task.CompletedTask));
        return this;
    }

    public IStateConfigurator<TState, TTrigger> NoAction<TInboxData>(TTrigger trigger)
    {
        AddCallback(new CallbackWithDataAsync<TTrigger, TState, TInboxData>(
           trigger: trigger,
           callback: _ => Task.CompletedTask));
        return this;
    }

    public IStateConfigurator<TState, TTrigger> OnAction<TInboxData>(TTrigger trigger, Action<TInboxData> func)
    {
        AddCallback(new CallbackWithDataAsync<TTrigger, TState, TInboxData>(
           trigger: trigger,
           callback: (inboxData) => Task.Run(() => func(inboxData))));
        return this;
    }

    public IStateConfigurator<TState, TTrigger> OnAction(TTrigger trigger, Action func)
    {
        AddCallback(new CallbackWithoutDataAsync<TTrigger, TState>(
           trigger: trigger,
           callback: () => Task.Run(func)));
        return this;
    }

    public IStateConfigurator<TState, TTrigger> OnActionAsync<TInboxData>(TTrigger trigger, Func<TInboxData, Task> func)
    {
        AddCallback(new CallbackWithDataAsync<TTrigger, TState, TInboxData>(trigger, func));
        return this;
    }

    public IStateConfigurator<TState, TTrigger> OnActionAsync(TTrigger trigger, Func<Task> func)
    {
        AddCallback(new CallbackWithoutDataAsync<TTrigger, TState>(trigger, func));
        return this;
    }

    public IStateConfigurator<TState, TTrigger> OnEntry(Action onEntryFunc)
    {
        OnEntryAsyncFunc = () => Task.Run(onEntryFunc);
        return this;
    }

    public IStateConfigurator<TState, TTrigger> OnEntryAsync(Func<Task> onExitFunc)
    {
        OnEntryAsyncFunc = onExitFunc;
        return this;
    }

    public IStateConfigurator<TState, TTrigger> OnExit(Action onExitFunc)
    {
        OnExitAsyncFunc = () => Task.Run(onExitFunc);
        return this;
    }

    public IStateConfigurator<TState, TTrigger> OnExitAsync(Func<Task> onExitFunc)
    {
        OnExitAsyncFunc = onExitFunc;
        return this;
    }

    private void AddCallback(Callback<TTrigger, TState> callback)
    {
        if (_callbacks.Any(c => c.Trigger.Equals(callback.Trigger) && c.InboxDataType == callback.InboxDataType))
        {
            throw new InvalidOperationException($"Триггер {callback.Trigger} с параметрами {callback.InboxDataType?.Name ?? "[не указано]"} уже обрабатывается");
        }
        else
        {
            _callbacks.Add(callback);
        }
    }
}