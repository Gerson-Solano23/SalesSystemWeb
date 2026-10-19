using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SalesSystem.API.Utility;
using SalesSystem.IOC;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = JwtSettings.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.InjectDependencies(builder.Configuration);

builder.Services.AddAuthentication("Bearer").AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = jwtSettings.GetSigningKey(),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
    };
});

// Rate limit configuration
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        await context.HttpContext.Response.WriteAsJsonAsync(new Response<object?>
        {
            status = false,
            message = "You have exceeded the request limit. Please try again later."
        }, cancellationToken);
    };

    options.AddFixedWindowLimiter(policyName: RateLimitPolicies.Fixed, limiter =>
    {
        limiter.PermitLimit = 4;
        limiter.Window = TimeSpan.FromSeconds(12);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 2;
    });

    // Login attempts are limited per client IP to slow down brute-force attacks.
    options.AddPolicy(RateLimitPolicies.Login, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// Authorization configuration (JWT role claim)
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("Admin", policy => policy.RequireRole(Roles.Admin));
    opt.AddPolicy("Employee", policy => policy.RequireRole(Roles.Employee));
    opt.AddPolicy("Supervisor", policy => policy.RequireRole(Roles.Supervisor));

    opt.AddPolicy("Admin_Supervisor", policy => policy.RequireRole(Roles.Admin, Roles.Supervisor));
    opt.AddPolicy("Admin_Supervisor_Employee", policy => policy.RequireRole(Roles.Admin, Roles.Employee, Roles.Supervisor));
});

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("NewPolicy", app =>
    {
        app.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Cache configuration
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("users", builder => builder.Expire(TimeSpan.FromMinutes(30)).Tag("users"));
    options.AddPolicy("products", builder => builder.Expire(TimeSpan.FromMinutes(30)).Tag("products"));
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NewPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

app.MapControllers();

app.Run();
