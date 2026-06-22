using System.ComponentModel;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Real_Time_Stock_Price_Monitor.Hubs;
using Real_Time_Stock_Price_Monitor.Services;
using Real_Time_Stock_Price_Monitor.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//SERVICE REGISTRATION

//Jwt signing secret
var jwtSecret = "meridian-capital-jwt-signing-secret-key-must-be-32-chars";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

//JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuerSigningKey = true,
               IssuerSigningKey = key,
               ValidateIssuer = true,
               ValidIssuer = "meridian-capital",
               ValidateAudience = true,
               ValidAudience = "meridian-capital",
               ValidateLifetime = true,
               ClockSkew = TimeSpan.FromSeconds(30)
           };


           options.Events = new JwtBearerEvents
           {
               OnMessageReceived = context =>
               {
                   var accessToken = context.Request.Query["access_token"];
                   var path = context.HttpContext.Request.Path;

                   if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/stockHub"))
                   {
                       context.Token = accessToken;
                   }

                   return Task.CompletedTask;
               }
           };
       });

builder.Services.AddAuthorization();

//Register Services
builder.Services.AddSingleton<TokenService>();
builder.Services.AddHostedService<StockPriceFeedService>();
builder.Services.AddSingleton<SubscriberStore>();


//Register SignalR with JSON string enum serialization
builder.Services.AddSignalR()
       .AddJsonProtocol(options =>
       {
           options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
       });

var app = builder.Build();

app.UseDefaultFiles();                               
app.UseStaticFiles();                                      

app.UseAuthentication();                                  
app.UseAuthorization();

//Login endpoint
app.MapPost("/auth/login",(LoginRequest req, TokenService ts) =>
{
    var result = ts.Authenticate(req);
    if(result == null)
    {
        return Results.Unauthorized();
    }else
    {
        return Results.Ok(result);
    }
});

app.MapHub<StockHub>("/stockHub");

app.Run();
