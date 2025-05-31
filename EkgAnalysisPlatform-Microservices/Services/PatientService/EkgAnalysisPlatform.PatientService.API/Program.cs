using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.RabbitMQ;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.Events;
using EkgAnalysisPlatform.PatientService.API.EventHandlers;
using EkgAnalysisPlatform.PatientService.Domain.Repositories;
using EkgAnalysisPlatform.PatientService.Infrastructure.Data;
using EkgAnalysisPlatform.PatientService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
builder.Services.AddDbContext<PatientDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PatientDatabase")));

// Register repositories
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

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
    .AddDbContextCheck<PatientDbContext>();

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
    var dbContext = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();