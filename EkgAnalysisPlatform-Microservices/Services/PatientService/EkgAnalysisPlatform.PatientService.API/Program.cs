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

// Register application services
builder.Services.AddScoped<IPatientService, PatientService>();

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

app.Run();