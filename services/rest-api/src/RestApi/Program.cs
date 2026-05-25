var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Phase 1+: SignalR, RabbitMQ consumer, Redis, gRPC client — see docs/architecture.md

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "rest-api",
    status = "healthy",
    phase = 0
}));

app.MapGet("/api/sensors", () => Results.Ok(Array.Empty<object>()));

app.Run();
