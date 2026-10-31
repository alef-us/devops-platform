using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using DevopsPlatform.Api.Todos;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DevopsPlatform.Api.Tests;

public sealed class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Root_ReturnsServiceInfo()
    {
        using var doc = JsonDocument.Parse(await _client.GetStringAsync("/"));

        Assert.Equal("devops-platform-api", doc.RootElement.GetProperty("service").GetString());
        Assert.False(string.IsNullOrEmpty(doc.RootElement.GetProperty("version").GetString()));
    }

    [Theory]
    [InlineData("/healthz")]
    [InlineData("/readyz")]
    public async Task HealthEndpoints_ReportHealthy(string path)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Todo_FullLifecycle()
    {
        var created = await _client.PostAsJsonAsync("/api/todos", new TodoRequest("  Write the Dockerfile  "));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var todo = await created.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(todo);
        Assert.Equal("Write the Dockerfile", todo.Title);
        Assert.Equal($"/api/todos/{todo.Id}", created.Headers.Location?.ToString());

        var fetched = await _client.GetFromJsonAsync<Todo>($"/api/todos/{todo.Id}");
        Assert.Equal(todo, fetched);

        var list = await _client.GetFromJsonAsync<List<Todo>>("/api/todos");
        Assert.Contains(list!, t => t.Id == todo.Id);

        var updated = await _client.PutAsJsonAsync($"/api/todos/{todo.Id}", new TodoRequest("Write the Dockerfile", IsDone: true));
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.True((await updated.Content.ReadFromJsonAsync<Todo>())!.IsDone);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/todos/{todo.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/todos/{todo.Id}")).StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_WithoutTitle_IsRejected(string? title)
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new TodoRequest(title));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("Title", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Create_WithTooLongTitle_IsRejected()
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new TodoRequest(new string('x', TodoRequest.MaxTitleLength + 1)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnknownTodo_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/todos/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync($"/api/todos/{id}", new TodoRequest("x"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"/api/todos/{id}")).StatusCode);
    }

    [Fact]
    public async Task MalformedId_ReturnsNotFound()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/todos/not-a-guid")).StatusCode);
    }
}
