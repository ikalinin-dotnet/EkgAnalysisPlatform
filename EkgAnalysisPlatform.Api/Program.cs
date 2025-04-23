using System.Text;
using EkgAnalysisPlatform.Core.Interfaces;
using EkgAnalysisPlatform.Core.Models;
using EkgAnalysisPlatform.Core.Services;
using EkgAnalysisPlatform.Infrastructure.Data;
using EkgAnalysisPlatform.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EkgAnalysisPlatform.Api.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text.Json;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ekgapi-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IEkgSignalRepository, EkgSignalRepository>();

// Register services
builder.Services.AddScoped<IEkgAnalysisService, EkgAnalysisService>();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "ThisIsASecretKeyForDemoOnlyDontUseInProduction"))
        };
    });

builder.Services.AddAuthorization();

// Add controllers and response caching
builder.Services.AddControllers();
builder.Services.AddResponseCaching();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // Replace with your validator class if needed

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for Blazor WASM
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7001")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedData(context);
}

// Configure the HTTP request pipeline.
// Add exception handling middleware
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            Log.Error($"Something went wrong: {contextFeature.Error}");
            await context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error."
            }.ToString());
        }
    });
});

// Always use Swagger for this portfolio project
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseResponseCaching();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Helper method to seed initial data
async Task SeedData(ApplicationDbContext context)
{
    // Add only if DB is empty
    if (!context.Patients.Any())
    {
        var patient = new Patient 
        { 
            PatientCode = "P12345", 
            Age = 45, 
            Gender = "Male" 
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        
        var signal = new EkgSignal
        {
            PatientId = patient.Id,
            RecordedAt = DateTime.Now.AddDays(-1),
            SamplingRate = 250,
            DataPoints = GenerateSampleEkgData(2500)
        };
        context.EkgSignals.Add(signal);
        
        await context.SaveChangesAsync();
    }
}

double[] GenerateSampleEkgData(int size)
{
    var data = new double[size];
    var random = new Random(42);
    
    for (int i = 0; i < size; i++)
    {
        // Create a simple sine wave with some noise
        data[i] = Math.Sin(i * 0.1) + random.NextDouble() * 0.1;
        
        // Add some peaks at regular intervals
        if (i % 250 == 0)
        {
            data[i] = 1.0; // R peak
        }
    }
    
    return data;
}

// Error Details class
public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}