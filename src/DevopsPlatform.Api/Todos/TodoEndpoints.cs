namespace DevopsPlatform.Api.Todos;

public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var todos = app.MapGroup("/api/todos").WithTags("Todos");

        todos.MapGet("/", (ITodoStore store) => TypedResults.Ok(store.List()));

        todos.MapGet("/{id:guid}", (Guid id, ITodoStore store) =>
            store.Get(id) is { } todo ? Results.Ok(todo) : Results.NotFound());

        todos.MapPost("/", (TodoRequest request, ITodoStore store) =>
        {
            if (request.Validate() is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var todo = store.Add(request.Title!, request.IsDone);
            return Results.Created($"/api/todos/{todo.Id}", todo);
        });

        todos.MapPut("/{id:guid}", (Guid id, TodoRequest request, ITodoStore store) =>
        {
            if (request.Validate() is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            return store.Update(id, request.Title!, request.IsDone) is { } todo
                ? Results.Ok(todo)
                : Results.NotFound();
        });

        todos.MapDelete("/{id:guid}", (Guid id, ITodoStore store) =>
            store.Delete(id) ? Results.NoContent() : Results.NotFound());

        return app;
    }
}
