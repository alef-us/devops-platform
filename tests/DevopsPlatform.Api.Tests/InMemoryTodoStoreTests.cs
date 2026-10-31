using DevopsPlatform.Api.Todos;

namespace DevopsPlatform.Api.Tests;

public sealed class InMemoryTodoStoreTests
{
    private sealed class StepClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now = _now.AddSeconds(1);
    }

    private readonly InMemoryTodoStore _store = new(new StepClock());

    [Fact]
    public void Add_TrimsTitle_AndStampsCreationTime()
    {
        var todo = _store.Add("  task  ", isDone: false);

        Assert.Equal("task", todo.Title);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 1, TimeSpan.Zero), todo.CreatedAt);
        Assert.Equal(todo, _store.Get(todo.Id));
    }

    [Fact]
    public void List_IsOrderedByCreationTime()
    {
        var first = _store.Add("first", false);
        var second = _store.Add("second", false);
        var third = _store.Add("third", false);

        Assert.Equal([first.Id, second.Id, third.Id], _store.List().Select(t => t.Id));
    }

    [Fact]
    public void Update_KeepsIdAndCreationTime()
    {
        var todo = _store.Add("old", false);

        var updated = _store.Update(todo.Id, "new", true);

        Assert.Equal(todo with { Title = "new", IsDone = true }, updated);
    }

    [Fact]
    public void Update_And_Delete_OfMissingTodo_ReportMiss()
    {
        Assert.Null(_store.Update(Guid.NewGuid(), "x", false));
        Assert.False(_store.Delete(Guid.NewGuid()));
    }

    [Fact]
    public async Task ConcurrentAdds_AreAllKept()
    {
        await Task.WhenAll(Enumerable.Range(0, 500).Select(i => Task.Run(() => _store.Add($"t{i}", false))));

        Assert.Equal(500, _store.List().Count);
    }
}
