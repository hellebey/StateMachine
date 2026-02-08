namespace StateMachine;

public interface ICallback<TTrigger, TState>
    where TTrigger : Enum
    where TState : Enum
{
    bool ChangeState { get; }

    Type? InboxDataType { get; }

    TState? NewState { get; init; }

    TTrigger Trigger { get; init; }

    Task InvokeAsync(object? inboxData);
}

internal abstract class Callback<TTrigger, TState> : ICallback<TTrigger, TState>
    where TTrigger : Enum
    where TState : Enum
{
    public bool ChangeState { get; init; }

    public abstract Type? InboxDataType { get; }

    public TState? NewState { get; init; }

    public TTrigger Trigger { get; init; }

    protected Callback(TTrigger trigger)
    {
        ChangeState = false;
        Trigger = trigger;
    }

    protected Callback(
        TTrigger trigger,
        TState newState)
        : this(trigger)

    {
        ChangeState = true;
        NewState = newState;
    }

    public abstract Task InvokeAsync(object? inboxData);
}

internal class CallbackWithoutDataAsync<TTrigger, TState>
    : Callback<TTrigger, TState>
    where TTrigger : Enum
    where TState : Enum
{
    public virtual Func<Task> Action { get; init; }

    public override Type? InboxDataType => null;

    public CallbackWithoutDataAsync(
        TTrigger trigger,
        Func<Task>? callback)
        : base(trigger)
    {
        Action = callback;
    }

    public CallbackWithoutDataAsync(
        TTrigger trigger,
        TState newState,
        Func<Task> callback)
        : base(trigger, newState)
    {
        Action = callback;
    }

    public override async Task InvokeAsync(object? inboxData)
    {
        await Action();
    }
}

internal class CallbackWithDataAsync<TTrigger, TState, TInboxDataType>
   : Callback<TTrigger, TState>
   where TTrigger : Enum
   where TState : Enum
{
    public virtual Func<TInboxDataType, Task>? Callback { get; init; }

    public override Type? InboxDataType => typeof(TInboxDataType);

    public CallbackWithDataAsync(
        TTrigger trigger,
        Func<TInboxDataType, Task>? callback)
        : base(trigger)
    {
        Callback = callback;
    }

    public CallbackWithDataAsync(
        TTrigger trigger,
        TState? newState,
        Func<TInboxDataType, Task>? callback)
        : base(trigger, newState)
    {
        Callback = callback;
    }

    public override async Task InvokeAsync(object? inboxData)
    {
        await (Callback?.Invoke((TInboxDataType)inboxData!) ?? Task.CompletedTask);
    }
}