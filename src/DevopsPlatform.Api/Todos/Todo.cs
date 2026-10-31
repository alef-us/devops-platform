namespace DevopsPlatform.Api.Todos;

public sealed record Todo(Guid Id, string Title, bool IsDone, DateTimeOffset CreatedAt);

public sealed record TodoRequest(string? Title, bool IsDone = false)
{
    public const int MaxTitleLength = 200;

    public Dictionary<string, string[]>? Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            return new() { [nameof(Title)] = ["Title is required."] };
        }

        if (Title.Trim().Length > MaxTitleLength)
        {
            return new() { [nameof(Title)] = [$"Title must be at most {MaxTitleLength} characters."] };
        }

        return null;
    }
}
