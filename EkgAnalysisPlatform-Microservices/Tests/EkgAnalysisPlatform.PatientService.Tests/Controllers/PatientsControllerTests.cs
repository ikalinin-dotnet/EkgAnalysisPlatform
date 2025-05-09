using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EkgAnalysisPlatform.PatientService.API.Controllers;
using EkgAnalysisPlatform.PatientService.API.DTOs;
using EkgAnalysisPlatform.PatientService.Domain.Models;
using EkgAnalysisPlatform.PatientService.Domain.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EkgAnalysisPlatform.PatientService.Tests.Controllers
{
    public class PatientsControllerTests
    {
        private readonly Mock<IPatientRepository> _repositoryMock;
        private readonly Mock<ILogger<PatientsController>> _loggerMock;
        private readonly PatientsController _controller;

        public PatientsControllerTests()
        {
            _repositoryMock = new Mock<IPatientRepository>();
            _loggerMock = new Mock<ILogger<PatientsController>>();
            _controller = new PatientsController(_repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfPatients()
        {
            // Arrange
            var patients = new List<Patient>
            {
                new Patient { Id = 1, PatientCode = "P001", Age = 35, Gender = "Male", ContactInfo = "test@example.com", RegisteredDate = DateTime.UtcNow },
                new Patient { Id = 2, PatientCode = "P002", Age = 42, Gender = "Female", ContactInfo = "test2@example.com", RegisteredDate = DateTime.UtcNow }
            };
            
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(patients);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPatients = okResult.Value.Should().BeAssignableTo<IEnumerable<PatientDto>>().Subject;
            returnedPatients.Should().HaveCount(2);
            returnedPatients.First().Id.Should().Be(1);
            returnedPatients.First().PatientCode.Should().Be("P001");
            returnedPatients.Last().Id.Should().Be(2);
            returnedPatients.Last().PatientCode.Should().Be("P002");
        }

        [Fact]
        public async Task GetAll_HandleExceptions_ReturnInternalServerError()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkResultWithPatient()
        {
            // Arrange
            var patient = new Patient { Id = 1, PatientCode = "P001", Age = 35, Gender = "Male", ContactInfo = "test@example.com", RegisteredDate = DateTime.UtcNow };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(patient);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPatient = okResult.Value.Should().BeAssignableTo<PatientDto>().Subject;
            returnedPatient.Id.Should().Be(1);
            returnedPatient.PatientCode.Should().Be("P001");
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Patient)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createPatientDto = new CreatePatientDto
            {
                PatientCode = "P003",
                Age = 28,
                Gender = "Female",
                ContactInfo = "test3@example.com"
            };
            
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Patient>())).ReturnsAsync(3);

            // Act
            var result = await _controller.Create(createPatientDto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.GetById));
            createdAtActionResult.RouteValues["id"].Should().Be(3);
            
            var returnedPatient = createdAtActionResult.Value.Should().BeAssignableTo<PatientDto>().Subject;
            returnedPatient.Id.Should().Be(3);
            returnedPatient.PatientCode.Should().Be("P003");
        }

        [Fact]
        public async Task Update_WithValidIdAndData_ReturnsNoContent()
        {
            // Arrange
            var existingPatient = new Patient
            {
                Id = 1,
                PatientCode = "P001",
                Age = 35,
                Gender = "Male",
                ContactInfo = "test@example.com",
                RegisteredDate = DateTime.UtcNow
            };
            
            var updatePatientDto = new UpdatePatientDto
            {
                Age = 36,
                Gender = "Male",
                ContactInfo = "updated@example.com"
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingPatient);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Patient>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(1, updatePatientDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Patient>(p => 
                p.Age == 36 && 
                p.ContactInfo == "updated@example.com")), Times.Once);
        }

        [Fact]
        public async Task Update_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var updatePatientDto = new UpdatePatientDto
            {
                Age = 36,
                Gender = "Male",
                ContactInfo = "updated@example.com"
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Patient)null);

            // Act
            var result = await _controller.Update(999, updatePatientDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var existingPatient = new Patient
            {
                Id = 1,
                PatientCode = "P001",
                Age = 35,
                Gender = "Male",
                ContactInfo = "test@example.com",
                RegisteredDate = DateTime.UtcNow
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingPatient);
            _repositoryMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Patient)null);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}