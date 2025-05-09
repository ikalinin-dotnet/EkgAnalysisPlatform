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

// Register application services
builder.Services.AddScoped<IBatchProcessingService, BatchProcessingService>();

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

app.Run();