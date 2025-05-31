using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.RabbitMQ;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.Events;
using EkgAnalysisPlatform.EkgSignalService.API.EventHandlers;
using EkgAnalysisPlatform.EkgSignalService.Domain.Repositories;
using EkgAnalysisPlatform.EkgSignalService.Infrastructure.Data;
using EkgAnalysisPlatform.EkgSignalService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
builder.Services.AddDbContext<EkgSignalDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EkgSignalDatabase")));

// Register repositories
builder.Services.AddScoped<IEkgSignalRepository, EkgSignalRepository>();

// Register event handlers
builder.Services.AddScoped<AnalysisCompletedEventHandler>();

// Configure RabbitMQ Event Bus
var eventBusHostName = builder.Configuration["EventBus:HostName"] ?? "localhost";
builder.Services.AddSingleton<IEventBus>(sp => 
{
    var logger = sp.GetService<ILogger<RabbitMQEventBus>>();
    return new RabbitMQEventBus(eventBusHostName, sp, logger);
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EkgSignalDbContext>();

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

// Configure event subscriptions
using (var scope = app.Services.CreateScope())
{
    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
    eventBus.Subscribe<AnalysisCompletedEvent, AnalysisCompletedEventHandler>();
}

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EkgSignalDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();