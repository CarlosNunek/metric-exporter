using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using metric_exporter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<InfluxService>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run("http://0.0.0.0:5000");
