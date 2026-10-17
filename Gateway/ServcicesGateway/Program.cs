using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Polly;
using ServcicesGateway.Middlewares;
using System.Threading.RateLimiting;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddServiceDiscovery();
    
builder.AddRedisClient("redis");

var redisConnectionString = builder.Configuration.GetConnectionString("redis");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "Gateway_";
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(
                    "[Gateway] JWT Authentication failed: {Error}",
                    context.Exception.Message
                );
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                logger.LogDebug("[Gateway] JWT validated for user {UserId}", userId);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var partitionKey = context.User.Identity?.IsAuthenticated == true
            ? $"user:{context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}"
            : $"ip:{context.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4,
            QueueLimit = 0
        });
    });

    options.AddPolicy("auth", context =>
    {
        var partitionKey = $"ip:{context.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(
            "[Gateway] Rate limit exceeded: IP={IP}, Path={Path}",
            context.HttpContext.Connection.RemoteIpAddress,
            context.HttpContext.Request.Path);

        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Too many requests. Please try again later." },
            cancellationToken);
    };
});


builder.Services.AddHttpClient("IdentityService", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var identityServiceUrl = "http://identityservice";
    client.BaseAddress = new Uri(identityServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
    var gatewaySecret = config["Gateway:Secret"];
    if (!string.IsNullOrEmpty(gatewaySecret))
    {
        client.DefaultRequestHeaders.Add("X-Gateway-Secret", gatewaySecret);
        client.DefaultRequestHeaders.Add("X-Gateway-Require-Auth", "false");
    }
}).AddServiceDiscovery()
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(100 * retryAttempt)));


builder.Services.AddScoped<ServcicesGateway.Interfaces.IAuthorizationService, ServcicesGateway.Services.CachedAuthorizationService>();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DiasporaBackOffice", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200" // pour le dev local
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
    context.Response.Headers["Pragma"] = "no-cache";
    await next();
});

app.UseCors("DiasporaBackOffice");

app.UseAuthentication();
app.UseRateLimiter();
app.UseMiddleware<AuthorizationMiddleware>();

app.MapReverseProxy();

app.Logger.LogInformation("[Gateway] API Gateway started successfully");

app.Run();
