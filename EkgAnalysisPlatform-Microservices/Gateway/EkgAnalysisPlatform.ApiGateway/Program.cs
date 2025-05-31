using EkgAnalysisPlatform.ApiGateway.HealthChecks;
using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.RabbitMQ;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new() { Title = "EKG Analysis Platform API", Version = "v1" });
});

// Configure HTTP client for health checks
builder.Services.AddHttpClient("HealthCheck").ConfigureHttpClient(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Configure gateway services
builder.Services.AddHttpClient();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configure RabbitMQ Event Bus
var eventBusHostName = builder.Configuration["EventBus:HostName"] ?? "localhost";
builder.Services.AddSingleton<IEventBus>(sp => 
{
    var logger = sp.GetService<ILogger<RabbitMQEventBus>>();
    return new RabbitMQEventBus(eventBusHostName, sp, logger);
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddMicroserviceHealthChecks(builder.Configuration);

// Add authentication (optional - uncomment if needed)
/*
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["IdentitySettings:Authority"];
        options.Audience = builder.Configuration["IdentitySettings:Audience"];
    });
*/

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// app.UseAuthorization(); // Uncomment if authentication is enabled
app.MapControllers();
app.MapReverseProxy();

// Configure health check endpoint with detailed response
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            Services = report.Entries.Select(e => new
            {
                Service = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration
            })
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
});

app.Run();