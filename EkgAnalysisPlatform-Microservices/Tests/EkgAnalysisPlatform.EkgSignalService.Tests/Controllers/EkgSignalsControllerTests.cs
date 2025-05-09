using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EkgAnalysisPlatform.EkgSignalService.API.Controllers;
using EkgAnalysisPlatform.EkgSignalService.API.DTOs;
using EkgAnalysisPlatform.EkgSignalService.Domain.Models;
using EkgAnalysisPlatform.EkgSignalService.Domain.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EkgAnalysisPlatform.EkgSignalService.Tests.Controllers
{
    public class EkgSignalsControllerTests
    {
        private readonly Mock<IEkgSignalRepository> _repositoryMock;
        private readonly Mock<ILogger<EkgSignalsController>> _loggerMock;
        private readonly EkgSignalsController _controller;

        public EkgSignalsControllerTests()
        {
            _repositoryMock = new Mock<IEkgSignalRepository>();
            _loggerMock = new Mock<ILogger<EkgSignalsController>>();
            _controller = new EkgSignalsController(_repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfSignals()
        {
            // Arrange
            var signals = new List<EkgSignal>
            {
                new EkgSignal { 
                    Id = 1, 
                    PatientCode = "P001", 
                    RecordedAt = DateTime.UtcNow.AddDays(-1), 
                    DataPoints = new double[] { 0.1, 0.2, 0.3 },
                    SamplingRate = 250,
                    DeviceId = "DEV001",
                    RecordedBy = "Dr. Smith"
                },
                new EkgSignal { 
                    Id = 2, 
                    PatientCode = "P002", 
                    RecordedAt = DateTime.UtcNow, 
                    DataPoints = new double[] { 0.4, 0.5, 0.6 },
                    SamplingRate = 500,
                    DeviceId = "DEV002",
                    RecordedBy = "Dr. Jones"
                }
            };
            
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(signals);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedSignals = okResult.Value.Should().BeAssignableTo<IEnumerable<EkgSignalDto>>().Subject;
            returnedSignals.Should().HaveCount(2);
            returnedSignals.First().Id.Should().Be(1);
            returnedSignals.First().PatientCode.Should().Be("P001");
            returnedSignals.First().DataPointsCount.Should().Be(3);
            returnedSignals.Last().Id.Should().Be(2);
            returnedSignals.Last().PatientCode.Should().Be("P002");
            returnedSignals.Last().DataPointsCount.Should().Be(3);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkResultWithSignal()
        {
            // Arrange
            var signal = new EkgSignal { 
                Id = 1, 
                PatientCode = "P001", 
                RecordedAt = DateTime.UtcNow.AddDays(-1), 
                DataPoints = new double[] { 0.1, 0.2, 0.3 },
                SamplingRate = 250,
                DeviceId = "DEV001",
                RecordedBy = "Dr. Smith"
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(signal);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedSignal = okResult.Value.Should().BeAssignableTo<EkgSignalDto>().Subject;
            returnedSignal.Id.Should().Be(1);
            returnedSignal.PatientCode.Should().Be("P001");
            returnedSignal.DataPointsCount.Should().Be(3);
        }
        
        [Fact]
        public async Task GetSignalData_WithValidId_ReturnsOkResultWithDataPoints()
        {
            // Arrange
            var signal = new EkgSignal { 
                Id = 1, 
                PatientCode = "P001", 
                RecordedAt = DateTime.UtcNow.AddDays(-1), 
                DataPoints = new double[] { 0.1, 0.2, 0.3 },
                SamplingRate = 250,
                DeviceId = "DEV001",
                RecordedBy = "Dr. Smith"
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(signal);

            // Act
            var result = await _controller.GetSignalData(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dataPoints = okResult.Value.Should().BeAssignableTo<double[]>().Subject;
            dataPoints.Should().BeEquivalentTo(new double[] { 0.1, 0.2, 0.3 });
        }

        [Fact]
        public async Task GetByPatientCode_ReturnsOkResultWithSignals()
        {
            // Arrange
            var signals = new List<EkgSignal>
            {
                new EkgSignal { 
                    Id = 1, 
                    PatientCode = "P001", 
                    RecordedAt = DateTime.UtcNow.AddDays(-1), 
                    DataPoints = new double[] { 0.1, 0.2, 0.3 },
                    SamplingRate = 250,
                    DeviceId = "DEV001",
                    RecordedBy = "Dr. Smith"
                },
                new EkgSignal { 
                    Id = 3, 
                    PatientCode = "P001", 
                    RecordedAt = DateTime.UtcNow, 
                    DataPoints = new double[] { 0.7, 0.8, 0.9 },
                    SamplingRate = 250,
                    DeviceId = "DEV001",
                    RecordedBy = "Dr. Smith"
                }
            };
            
            _repositoryMock.Setup(r => r.GetByPatientCodeAsync("P001")).ReturnsAsync(signals);

            // Act
            var result = await _controller.GetByPatientCode("P001");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedSignals = okResult.Value.Should().BeAssignableTo<IEnumerable<EkgSignalDto>>().Subject;
            returnedSignals.Should().HaveCount(2);
            returnedSignals.All(s => s.PatientCode == "P001").Should().BeTrue();
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createSignalDto = new CreateEkgSignalDto
            {
                PatientCode = "P003",
                RecordedAt = DateTime.UtcNow,
                DataPoints = new double[] { 0.1, 0.2, 0.3, 0.4 },
                SamplingRate = 250,
                DeviceId = "DEV003",
                RecordedBy = "Dr. Brown"
            };
            
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<EkgSignal>())).ReturnsAsync(3);

            // Act
            var result = await _controller.Create(createSignalDto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(EkgSignalsController.GetById));
            createdAtActionResult.RouteValues["id"].Should().Be(3);
            
            var returnedSignal = createdAtActionResult.Value.Should().BeAssignableTo<EkgSignalDto>().Subject;
            returnedSignal.Id.Should().Be(3);
            returnedSignal.PatientCode.Should().Be("P003");
            returnedSignal.DataPointsCount.Should().Be(4);
        }

        [Fact]
        public async Task Update_WithValidIdAndData_ReturnsNoContent()
        {
            // Arrange
            var existingSignal = new EkgSignal { 
                Id = 1, 
                PatientCode = "P001", 
                RecordedAt = DateTime.UtcNow.AddDays(-1), 
                DataPoints = new double[] { 0.1, 0.2, 0.3 },
                SamplingRate = 250,
                DeviceId = "DEV001",
                RecordedBy = "Dr. Smith",
                IsProcessed = false
            };
            
            var updateSignalDto = new UpdateEkgSignalDto
            {
                Notes = "Updated notes",
                IsProcessed = true
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingSignal);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<EkgSignal>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(1, updateSignalDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<EkgSignal>(s => 
                s.Notes == "Updated notes" && 
                s.IsProcessed == true)), Times.Once);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var existingSignal = new EkgSignal { 
                Id = 1, 
                PatientCode = "P001", 
                RecordedAt = DateTime.UtcNow.AddDays(-1), 
                DataPoints = new double[] { 0.1, 0.2, 0.3 },
                SamplingRate = 250,
                DeviceId = "DEV001",
                RecordedBy = "Dr. Smith"
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingSignal);
            _repositoryMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}