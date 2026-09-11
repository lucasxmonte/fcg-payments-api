using FCG.PaymentsAPI.Consumers;
using MassTransit;
using Prometheus;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ── MassTransit + RabbitMQ ──────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"] ?? "localhost",
            builder.Configuration["RabbitMQ:VirtualHost"] ?? "/",
            h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });

        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();

// ── Prometheus metrics ─────────────────────────────────────────────
app.UseHttpMetrics();
app.MapMetrics();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "FCG.PaymentsAPI",
    timestamp = DateTime.UtcNow
}));

Log.Information("🚀 FCG.PaymentsAPI iniciando...");
app.Run();
