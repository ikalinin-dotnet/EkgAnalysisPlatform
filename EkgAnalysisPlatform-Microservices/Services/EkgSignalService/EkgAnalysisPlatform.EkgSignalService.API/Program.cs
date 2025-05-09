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

// Register application services
builder.Services.AddScoped<IEkgSignalService, EkgSignalService>();

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

app.Run();