using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();
