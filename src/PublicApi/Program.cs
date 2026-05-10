using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using PublicApi;
using PublicApi.Interfaces;
using PublicApi.Middleware;
using PublicApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var tokenKey = builder.Configuration["TokenKey"]
    ?? throw new InvalidOperationException("TokenKey is not configured.");

builder.Services.AddSingleton<QueueService>();
builder.Services.AddSingleton<IQueueService>(sp => sp.GetRequiredService<QueueService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<QueueService>());
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });
builder.Logging.AddConsole();

builder.Services.AddScoped<ITokenService, TokenService>();
if (!builder.Configuration.GetValue("UseAuthentication", false))
    builder.Services.AddSingleton<IAuthorizationHandler, AllowAnonymous>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

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