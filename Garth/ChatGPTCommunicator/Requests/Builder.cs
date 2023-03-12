namespace ChatGPTCommunicator.Requests;

public class Builder<T, U> where T : class where U : new()
{
    protected U Instance = new();

    protected T Do(Action action)
    {
        action?.Invoke();
        return (this as T)!;
    }

    public U Build() =>
        Instance;

    public static implicit operator U(Builder<T, U> builder) => builder.Build();
}