using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using PublicApi;
using PublicApi.Interfaces;
using PublicApi.Middleware;
using PublicApi.Services;
using System.Text;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<QueueService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("admin"));
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });
builder.Logging.AddConsole();

builder.Services.AddScoped<ITokenService, TokenService>();
if (bool.TryParse(config["UseAuthentication"], out bool useAuthentication) && !useAuthentication)
    builder.Services.AddSingleton<IAuthorizationHandler, AllowAnonymous>();

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
else 
    app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();