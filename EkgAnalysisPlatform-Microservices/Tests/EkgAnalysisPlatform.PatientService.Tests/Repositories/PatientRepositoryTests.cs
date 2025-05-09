namespace EkgAnalysisPlatform.PatientService.Tests.Repositories
{
    public class PatientRepositoryTests
    {
        // These tests would interact with a test database or in-memory database
        // Here's a simplified example using an in-memory database:
        
        
        private readonly DbContextOptions<PatientDbContext> _options;
        private readonly PatientDbContext _context;
        private readonly PatientRepository _repository;

        public PatientRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<PatientDbContext>()
                .UseInMemoryDatabase(databaseName: "PatientTestDb")
                .Options;

            _context = new PatientDbContext(_options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            
            // Seed test data
            _context.Patients.AddRange(
                new Patient { Id = 1, PatientCode = "P001", Age = 35, Gender = "Male", ContactInfo = "test@example.com", RegisteredDate = DateTime.UtcNow },
                new Patient { Id = 2, PatientCode = "P002", Age = 42, Gender = "Female", ContactInfo = "test2@example.com", RegisteredDate = DateTime.UtcNow }
            );
            _context.SaveChanges();
            
            _repository = new PatientRepository(_context);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllPatients()
        {
            // Act
            var result = await _repository.GetAllAsync();
            
            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsPatient()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);
            
            // Assert
            result.Should().NotBeNull();
            result.PatientCode.Should().Be("P001");
        }

        [Fact]
        public async Task AddAsync_AddsNewPatient_ReturnsId()
        {
            // Arrange
            var newPatient = new Patient
            {
                PatientCode = "P003",
                Age = 28,
                Gender = "Female",
                ContactInfo = "test3@example.com",
                RegisteredDate = DateTime.UtcNow
            };
            
            // Act
            var id = await _repository.AddAsync(newPatient);
            var patients = await _repository.GetAllAsync();
            
            // Assert
            id.Should().Be(3);
            patients.Should().HaveCount(3);
        }
    }
}