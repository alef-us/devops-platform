using System.Reflection;

using DevopsPlatform.Api.Todos;

using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    // One JSON object per line, ready for a log collector in the cluster.
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole();
}

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ITodoStore, InMemoryTodoStore>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck("todo-store", () => HealthCheckResult.Healthy(), tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

var version = typeof(Program).Assembly
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";

app.MapGet("/", (IHostEnvironment env) => Results.Ok(new
{
    service = "devops-platform-api",
    version,
    environment = env.EnvironmentName,
    host = Environment.MachineName,
}));

// Liveness: the process answers. Readiness: dependencies are usable.
app.MapHealthChecks("/healthz", new() { Predicate = check => check.Tags.Contains("live") });
app.MapHealthChecks("/readyz", new() { Predicate = check => check.Tags.Contains("ready") });

app.MapTodoEndpoints();

app.Run();

public partial class Program;
