using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.RabbitMQ;
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

// Configure RabbitMQ Event Bus
var eventBusHostName = builder.Configuration["EventBus:HostName"] ?? "localhost";
builder.Services.AddSingleton<IEventBus>(sp => new RabbitMQEventBus(eventBusHostName));

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

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();