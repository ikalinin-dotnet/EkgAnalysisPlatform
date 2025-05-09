using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EkgAnalysisPlatform.EkgSignalService.Domain.Models;
using EkgAnalysisPlatform.EkgSignalService.Infrastructure.Data;
using EkgAnalysisPlatform.EkgSignalService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EkgAnalysisPlatform.EkgSignalService.Tests.Repositories
{
    public class EkgSignalRepositoryTests : IDisposable
    {
        private readonly DbContextOptions<EkgSignalDbContext> _options;
        private readonly EkgSignalDbContext _context;
        private readonly EkgSignalRepository _repository;

        public EkgSignalRepositoryTests()
        {
            // Create a unique database name for each test run to isolate test data
            _options = new DbContextOptionsBuilder<EkgSignalDbContext>()
                .UseInMemoryDatabase(databaseName: $"EkgSignalTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new EkgSignalDbContext(_options);
            _context.Database.EnsureCreated();
            
            // Seed test data
            SeedTestData();
            
            _repository = new EkgSignalRepository(_context);
        }

        private void SeedTestData()
        {
            // Add test EKG signals
            _context.EkgSignals.AddRange(
                new EkgSignal
                {
                    Id = 1,
                    PatientCode = "P001",
                    RecordedAt = DateTime.UtcNow.AddDays(-1),
                    DataPoints = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 },
                    SamplingRate = 250,
                    DeviceId = "DEV001",
                    RecordedBy = "Dr. Smith",
                    IsProcessed = true
                },
                new EkgSignal
                {
                    Id = 2,
                    PatientCode = "P001",
                    RecordedAt = DateTime.UtcNow,
                    DataPoints = new double[] { 0.5, 0.4, 0.3, 0.2, 0.1 },
                    SamplingRate = 250,
                    DeviceId = "DEV001",
                    RecordedBy = "Dr. Smith",
                    IsProcessed = false
                },
                new EkgSignal
                {
                    Id = 3,
                    PatientCode = "P002",
                    RecordedAt = DateTime.UtcNow.AddDays(-2),
                    DataPoints = new double[] { 0.1, 0.3, 0.5, 0.7, 0.9 },
                    SamplingRate = 500,
                    DeviceId = "DEV002",
                    RecordedBy = "Dr. Jones",
                    IsProcessed = true
                }
            );
            
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllSignals()
        {
            // Act
            var result = await _repository.GetAllAsync();
            
            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsSignal()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);
            
            // Assert
            result.Should().NotBeNull();
            result!.PatientCode.Should().Be("P001");
            result.DataPoints.Should().HaveCount(5);
            result.DataPoints.Should().BeEquivalentTo(new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 });
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);
            
            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByPatientCodeAsync_ReturnsPatientSignals()
        {
            // Act
            var result = await _repository.GetByPatientCodeAsync("P001");
            
            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(s => s.PatientCode == "P001").Should().BeTrue();
        }

        [Fact]
        public async Task GetUnprocessedSignalsAsync_ReturnsUnprocessedSignals()
        {
            // Act
            var result = await _repository.GetUnprocessedSignalsAsync();
            
            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.All(s => s.IsProcessed == false).Should().BeTrue();
        }

        [Fact]
        public async Task AddAsync_AddsNewSignal_ReturnsId()
        {
            // Arrange
            var newSignal = new EkgSignal
            {
                PatientCode = "P003",
                RecordedAt = DateTime.UtcNow,
                DataPoints = new double[] { 0.2, 0.4, 0.6, 0.8, 1.0 },
                SamplingRate = 250,
                DeviceId = "DEV003",
                RecordedBy = "Dr. Brown",
                IsProcessed = false
            };
            
            // Act
            var id = await _repository.AddAsync(newSignal);
            var allSignals = await _repository.GetAllAsync();
            
            // Assert
            id.Should().BeGreaterThan(0);
            allSignals.Should().HaveCount(4);
            var addedSignal = allSignals.FirstOrDefault(s => s.Id == id);
            addedSignal.Should().NotBeNull();
            addedSignal!.PatientCode.Should().Be("P003");
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingSignal()
        {
            // Arrange
            var signal = await _repository.GetByIdAsync(2);
            signal!.IsProcessed = true;
            signal.Notes = "Updated signal";
            
            // Act
            await _repository.UpdateAsync(signal);
            var updatedSignal = await _repository.GetByIdAsync(2);
            
            // Assert
            updatedSignal.Should().NotBeNull();
            updatedSignal!.IsProcessed.Should().BeTrue();
            updatedSignal.Notes.Should().Be("Updated signal");
        }

        [Fact]
        public async Task DeleteAsync_RemovesSignal()
        {
            // Act
            await _repository.DeleteAsync(3);
            var allSignals = await _repository.GetAllAsync();
            var deletedSignal = await _repository.GetByIdAsync(3);
            
            // Assert
            allSignals.Should().HaveCount(2);
            deletedSignal.Should().BeNull();
        }

        public void Dispose()
        {
            // Clean up the in-memory database after each test
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}