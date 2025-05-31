using EkgAnalysisPlatform.AnalysisService.Domain.Repositories;
using EkgAnalysisPlatform.AnalysisService.Domain.Services;
using EkgAnalysisPlatform.AnalysisService.Infrastructure.Data;
using EkgAnalysisPlatform.AnalysisService.Infrastructure.Repositories;
using EkgAnalysisPlatform.AnalysisService.Infrastructure.Services;
using EkgAnalysisPlatform.AnalysisService.API.Services;
using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.RabbitMQ;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
builder.Services.AddDbContext<AnalysisDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AnalysisDatabase")));

// Register repositories
builder.Services.AddScoped<IAnalysisResultRepository, AnalysisResultRepository>();
builder.Services.AddScoped<IAnalysisRequestRepository, AnalysisRequestRepository>();
builder.Services.AddScoped<IAnalysisAlgorithmConfigRepository, AnalysisAlgorithmConfigRepository>();

// Register domain services
builder.Services.AddScoped<IEkgAnalysisEngine, EkgAnalysisEngine>();

// Register background services
builder.Services.AddHostedService<AnalysisBackgroundService>();

// Configure RabbitMQ Event Bus
var eventBusHostName = builder.Configuration["EventBus:HostName"] ?? "localhost";
builder.Services.AddSingleton<IEventBus>(sp => new RabbitMQEventBus(
    eventBusHostName, 
    sp, 
    sp.GetService<ILogger<RabbitMQEventBus>>()));

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AnalysisDbContext>();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();