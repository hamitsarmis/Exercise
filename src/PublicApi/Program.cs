using PublicApi.Middleware;
using PublicApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<QueueService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
var appLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var queueService = app.Services.GetRequiredService<QueueService>();

appLifetime.ApplicationStarted.Register(async () => await queueService.StartAsync(appLifetime.ApplicationStopping));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();