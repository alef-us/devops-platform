using System.Collections.Concurrent;

namespace DevopsPlatform.Api.Todos;

public sealed class InMemoryTodoStore(TimeProvider clock) : ITodoStore
{
    private readonly ConcurrentDictionary<Guid, Todo> _todos = new();

    public IReadOnlyList<Todo> List() =>
        [.. _todos.Values.OrderBy(t => t.CreatedAt).ThenBy(t => t.Id)];

    public Todo? Get(Guid id) => _todos.GetValueOrDefault(id);

    public Todo Add(string title, bool isDone)
    {
        var todo = new Todo(Guid.NewGuid(), title.Trim(), isDone, clock.GetUtcNow());
        _todos[todo.Id] = todo;
        return todo;
    }

    public Todo? Update(Guid id, string title, bool isDone)
    {
        while (_todos.TryGetValue(id, out var current))
        {
            var updated = current with { Title = title.Trim(), IsDone = isDone };
            if (_todos.TryUpdate(id, updated, current))
            {
                return updated;
            }
        }

        return null;
    }

    public bool Delete(Guid id) => _todos.TryRemove(id, out _);
}
