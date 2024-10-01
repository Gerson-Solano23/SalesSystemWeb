
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SalesSystem.BLL.Services;
using SalesSystem.IOC;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
string? key = Environment.GetEnvironmentVariable("CERTIFICATE_KEY");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<WeeklyTaskService>();
builder.Services.InjectDependencies(builder.Configuration);
builder.Services.AddAuthentication("Bearer").AddJwtBearer(opt =>
{

    var singning = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
    var signingCredentials = new SigningCredentials(singning, SecurityAlgorithms.HmacSha256Signature);

    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = singning,
    };

});
//Rate Limit Configuration


builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;

    }));




// Authorization Configuration  JWT 
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("Admin", policy => policy.RequireRole("1"));
    opt.AddPolicy("Employee", policy => policy.RequireRole("2"));
    opt.AddPolicy("Supervisor", policy => policy.RequireRole("3"));
    
    opt.AddPolicy("Admin_Supervisor", policy => policy.RequireRole("1", "3"));
    opt.AddPolicy("Admin_Supervisor_Employee", policy => policy.RequireRole("1", "2", "3"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("NewPolicy", app =>
    {
        app.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
//cache configuration 
builder.Services.AddOutputCache(options => {
    options.AddPolicy("users", builder => builder.Expire(TimeSpan.FromMinutes(30)).Tag("users"));
    options.AddPolicy("products", builder => builder.Expire(TimeSpan.FromMinutes(30)).Tag("products"));

});


builder.Services.AddResponseCaching();
var app = builder.Build();
app.UseRateLimiter();

app.Use(async (context, next) =>
{
    await next();  // Deja que el siguiente middleware procese la solicitud

    // Verifica si la respuesta tiene el estado 429 (Too Many Requests)

    if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
    {
        // Configura el contenido y el código de estado
        context.Response.ContentType = "application/json";

        // Asegúrate de limpiar cualquier respuesta que ya se haya escrito
        context.Response.Clear();

        // Construye el mensaje de respuesta personalizado
        var responseMessage = new
        {
            message = "You have exceeded the request limit. Please try again later."
        };

        // Escribe el mensaje personalizado en la respuesta
        await context.Response.WriteAsJsonAsync(responseMessage);
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NewPolicy");
app.UseOutputCache();
app.UseAuthorization();

app.MapControllers();

app.Run();
