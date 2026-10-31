namespace DevopsPlatform.Api.Todos;

public interface ITodoStore
{
    IReadOnlyList<Todo> List();

    Todo? Get(Guid id);

    Todo Add(string title, bool isDone);

    Todo? Update(Guid id, string title, bool isDone);

    bool Delete(Guid id);
}
