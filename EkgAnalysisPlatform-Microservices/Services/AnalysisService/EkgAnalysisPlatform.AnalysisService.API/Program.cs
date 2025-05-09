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

// Register application services
builder.Services.AddScoped<IAnalysisService, AnalysisService>();

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

app.Run();