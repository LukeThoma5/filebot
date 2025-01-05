public abstract record PromptResult<T>
{
    public static PromptResult<T> ExitResult => new Exit();
    public static implicit operator PromptResult<T>(T item) => new Selection(item);
    public sealed record Selection(T Item) : PromptResult<T>
    {
    }

    public sealed record Exit : PromptResult<T>;

    public sealed record Manual : PromptResult<T>;

    public string Display(Func<T, string> display) => this switch
    {
        Exit => "exit",
        Manual => "manual entry",
        Selection(var inner) => display(inner)
    };
}