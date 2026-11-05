using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Real_Time_Stock_Price_Monitor.Data;
using Real_Time_Stock_Price_Monitor.Hubs;
using Real_Time_Stock_Price_Monitor.Models;
using Real_Time_Stock_Price_Monitor.Services;

var builder = WebApplication.CreateBuilder(args);

// JWT key from configuration
var key = JwtSettings.GetSigningKey(builder.Configuration);

// SQLite user store. IsNullOrWhiteSpace so an empty "" also falls back to the default.
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
    connectionString = "Data Source=stockmonitor.db";

var dbFolder = Path.GetDirectoryName(
    Path.GetFullPath(new SqliteConnectionStringBuilder(connectionString).DataSource));
if (!string.IsNullOrEmpty(dbFolder))
    Directory.CreateDirectory(dbFolder);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuerSigningKey = true,
               IssuerSigningKey = key,
               ValidateIssuer = true,
               ValidIssuer = JwtSettings.Issuer,
               ValidateAudience = true,
               ValidAudience = JwtSettings.Audience,
               ValidateLifetime = true,
               ClockSkew = TimeSpan.FromMinutes(15)
           };

           options.Events = new JwtBearerEvents
           {
               OnMessageReceived = context =>
               {
                   var accessToken = context.Request.Query["access_token"];
                   var path = context.HttpContext.Request.Path;

                   if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/stockHub"))
                   {
                       context.Token = accessToken;
                   }

                   return Task.CompletedTask;
               }
           };
       });

builder.Services.AddAuthorization();

builder.Services.AddScoped<TokenService>();   
builder.Services.AddHostedService<StockPriceFeedService>();
builder.Services.AddSingleton<SubscriberStore>();

var signalR = builder.Services.AddSignalR()
       .AddJsonProtocol(options =>
       {
           options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
       });

// Redis backplane only when configured, so the app still runs without Redis.
var redisConnection = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    signalR.AddStackExchangeRedis(redisConnection);
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.SeedDemoUsers(db, app.Configuration);
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/login", async (LoginRequest req, TokenService ts) =>
{
    var result = await ts.AuthenticateAsync(req);
    return result is null ? Results.Unauthorized() : Results.Ok(result);
});

app.MapHub<StockHub>("/stockHub");

app.Run();