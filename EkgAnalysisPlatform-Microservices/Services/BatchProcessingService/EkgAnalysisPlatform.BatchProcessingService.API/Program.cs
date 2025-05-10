using EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories;
using EkgAnalysisPlatform.BatchProcessingService.Infrastructure.Data;
using EkgAnalysisPlatform.BatchProcessingService.Infrastructure.Repositories;
using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.RabbitMQ;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
builder.Services.AddDbContext<BatchDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BatchDatabase")));

// Register repositories
builder.Services.AddScoped<IBatchJobRepository, BatchJobRepository>();
builder.Services.AddScoped<IBatchJobItemRepository, BatchJobItemRepository>();
builder.Services.AddScoped<IScheduleConfigurationRepository, ScheduleConfigurationRepository>();

// Configure RabbitMQ Event Bus
var eventBusHostName = builder.Configuration["EventBus:HostName"] ?? "localhost";
builder.Services.AddSingleton<IEventBus>(sp => new RabbitMQEventBus(eventBusHostName));

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BatchDbContext>();

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
    var dbContext = scope.ServiceProvider.GetRequiredService<BatchDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();