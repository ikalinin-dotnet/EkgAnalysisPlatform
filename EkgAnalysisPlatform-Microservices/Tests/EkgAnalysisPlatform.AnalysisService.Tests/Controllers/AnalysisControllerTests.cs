using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EkgAnalysisPlatform.AnalysisService.API.Controllers;
using EkgAnalysisPlatform.AnalysisService.API.DTOs;
using EkgAnalysisPlatform.AnalysisService.Domain.Models;
using EkgAnalysisPlatform.AnalysisService.Domain.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EkgAnalysisPlatform.AnalysisService.Tests.Controllers
{
    public class AnalysisControllerTests
    {
        private readonly Mock<IAnalysisRequestRepository> _requestRepositoryMock;
        private readonly Mock<IAnalysisResultRepository> _resultRepositoryMock;
        private readonly Mock<ILogger<AnalysisController>> _loggerMock;
        private readonly AnalysisController _controller;

        public AnalysisControllerTests()
        {
            _requestRepositoryMock = new Mock<IAnalysisRequestRepository>();
            _resultRepositoryMock = new Mock<IAnalysisResultRepository>();
            _loggerMock = new Mock<ILogger<AnalysisController>>();
            _controller = new AnalysisController(
                _requestRepositoryMock.Object,
                _resultRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllRequests_ReturnsOkResult_WithListOfRequests()
        {
            // Arrange
            var requests = new List<AnalysisRequest>
            {
                new AnalysisRequest { 
                    Id = 1, 
                    SignalReference = "SIG001", 
                    PatientCode = "P001", 
                    RequestedAt = DateTime.UtcNow.AddHours(-2),
                    Status = AnalysisStatus.Completed,
                    RequestedBy = "Dr. Smith",
                    AnalysisType = "Standard",
                    Priority = 1
                },
                new AnalysisRequest { 
                    Id = 2, 
                    SignalReference = "SIG002", 
                    PatientCode = "P002", 
                    RequestedAt = DateTime.UtcNow.AddHours(-1),
                    Status = AnalysisStatus.Pending,
                    RequestedBy = "Dr. Jones",
                    AnalysisType = "Extended",
                    Priority = 2
                }
            };
            
            _requestRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(requests);

            // Act
            var result = await _controller.GetAllRequests();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedRequests = okResult.Value.Should().BeAssignableTo<IEnumerable<AnalysisRequestDto>>().Subject;
            returnedRequests.Should().HaveCount(2);
            returnedRequests.First().Id.Should().Be(1);
            returnedRequests.First().SignalReference.Should().Be("SIG001");
            returnedRequests.First().Status.Should().Be("Completed");
            returnedRequests.Last().Id.Should().Be(2);
            returnedRequests.Last().SignalReference.Should().Be("SIG002");
            returnedRequests.Last().Status.Should().Be("Pending");
        }

        [Fact]
        public async Task GetRequestById_WithValidId_ReturnsOkResultWithRequest()
        {
            // Arrange
            var request = new AnalysisRequest { 
                Id = 1, 
                SignalReference = "SIG001", 
                PatientCode = "P001", 
                RequestedAt = DateTime.UtcNow.AddHours(-2),
                Status = AnalysisStatus.Completed,
                RequestedBy = "Dr. Smith",
                AnalysisType = "Standard",
                Priority = 1
            };
            
            _requestRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(request);

            // Act
            var result = await _controller.GetRequestById(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedRequest = okResult.Value.Should().BeAssignableTo<AnalysisRequestDto>().Subject;
            returnedRequest.Id.Should().Be(1);
            returnedRequest.SignalReference.Should().Be("SIG001");
            returnedRequest.Status.Should().Be("Completed");
        }

        [Fact]
        public async Task GetRequestById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _requestRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AnalysisRequest)null);

            // Act
            var result = await _controller.GetRequestById(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateRequest_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createRequestDto = new CreateAnalysisRequestDto
            {
                SignalReference = "SIG003",
                PatientCode = "P003",
                RequestedBy = "Dr. Brown",
                AnalysisType = "Standard",
                Priority = 1
            };
            
            _requestRepositoryMock.Setup(r => r.AddAsync(It.IsAny<AnalysisRequest>())).ReturnsAsync(3);

            // Act
            var result = await _controller.CreateRequest(createRequestDto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(AnalysisController.GetRequestById));
            createdAtActionResult.RouteValues["id"].Should().Be(3);
            
            var returnedRequest = createdAtActionResult.Value.Should().BeAssignableTo<AnalysisRequestDto>().Subject;
            returnedRequest.Id.Should().Be(3);
            returnedRequest.SignalReference.Should().Be("SIG003");
            returnedRequest.Status.Should().Be("Pending");
        }

        [Fact]
        public async Task GetAllResults_ReturnsOkResult_WithListOfResults()
        {
            // Arrange
            var results = new List<AnalysisResult>
            {
                new AnalysisResult { 
                    Id = 1, 
                    SignalReference = "SIG001", 
                    PatientCode = "P001", 
                    HeartRate = 72.5,
                    HasArrhythmia = false,
                    QRSDuration = 0.08,
                    PRInterval = 0.16,
                    QTInterval = 0.38,
                    AnalyzedAt = DateTime.UtcNow.AddHours(-1),
                    AnalyzerVersion = "1.0.0",
                    Status = AnalysisStatus.Completed,
                    DiagnosticNotes = "Normal sinus rhythm"
                },
                new AnalysisResult { 
                    Id = 2, 
                    SignalReference = "SIG002", 
                    PatientCode = "P002", 
                    HeartRate = 92.3,
                    HasArrhythmia = true,
                    QRSDuration = 0.11,
                    PRInterval = 0.14,
                    QTInterval = 0.42,
                    AnalyzedAt = DateTime.UtcNow,
                    AnalyzerVersion = "1.0.0",
                    Status = AnalysisStatus.Completed,
                    DiagnosticNotes = "Possible arrhythmia detected"
                }
            };
            
            _resultRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(results);

            // Act
            var result = await _controller.GetAllResults();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResults = okResult.Value.Should().BeAssignableTo<IEnumerable<AnalysisResultDto>>().Subject;
            returnedResults.Should().HaveCount(2);
            returnedResults.First().Id.Should().Be(1);
            returnedResults.First().SignalReference.Should().Be("SIG001");
            returnedResults.First().HeartRate.Should().Be(72.5);
            returnedResults.First().HasArrhythmia.Should().BeFalse();
            returnedResults.Last().Id.Should().Be(2);
            returnedResults.Last().SignalReference.Should().Be("SIG002");
            returnedResults.Last().HeartRate.Should().Be(92.3);
            returnedResults.Last().HasArrhythmia.Should().BeTrue();
        }

        [Fact]
        public async Task GetResultById_WithValidId_ReturnsOkResultWithResult()
        {
            // Arrange
            var result = new AnalysisResult { 
                Id = 1, 
                SignalReference = "SIG001", 
                PatientCode = "P001", 
                HeartRate = 72.5,
                HasArrhythmia = false,
                QRSDuration = 0.08,
                PRInterval = 0.16,
                QTInterval = 0.38,
                AnalyzedAt = DateTime.UtcNow.AddHours(-1),
                AnalyzerVersion = "1.0.0",
                Status = AnalysisStatus.Completed,
                DiagnosticNotes = "Normal sinus rhythm"
            };
            
            _resultRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetResultById(1);

            // Assert
            var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<AnalysisResultDto>().Subject;
            returnedResult.Id.Should().Be(1);
            returnedResult.SignalReference.Should().Be("SIG001");
            returnedResult.HeartRate.Should().Be(72.5);
            returnedResult.HasArrhythmia.Should().BeFalse();
        }

        [Fact]
        public async Task GetResultById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _resultRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AnalysisResult)null);

            // Act
            var result = await _controller.GetResultById(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetResultsByPatientCode_ReturnsOkResult_WithListOfResults()
        {
            // Arrange
            var results = new List<AnalysisResult>
            {
                new AnalysisResult { 
                    Id = 1, 
                    SignalReference = "SIG001", 
                    PatientCode = "P001", 
                    HeartRate = 72.5,
                    HasArrhythmia = false,
                    QRSDuration = 0.08,
                    PRInterval = 0.16,
                    QTInterval = 0.38,
                    AnalyzedAt = DateTime.UtcNow.AddDays(-1),
                    AnalyzerVersion = "1.0.0",
                    Status = AnalysisStatus.Completed,
                    DiagnosticNotes = "Normal sinus rhythm"
                },
                new AnalysisResult { 
                    Id = 3, 
                    SignalReference = "SIG003", 
                    PatientCode = "P001", 
                    HeartRate = 68.2,
                    HasArrhythmia = false,
                    QRSDuration = 0.09,
                    PRInterval = 0.17,
                    QTInterval = 0.39,
                    AnalyzedAt = DateTime.UtcNow,
                    AnalyzerVersion = "1.0.0",
                    Status = AnalysisStatus.Completed,
                    DiagnosticNotes = "Normal sinus rhythm"
                }
            };
            
            _resultRepositoryMock.Setup(r => r.GetByPatientCodeAsync("P001")).ReturnsAsync(results);

            // Act
            var result = await _controller.GetResultsByPatientCode("P001");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResults = okResult.Value.Should().BeAssignableTo<IEnumerable<AnalysisResultDto>>().Subject;
            returnedResults.Should().HaveCount(2);
            returnedResults.All(r => r.PatientCode == "P001").Should().BeTrue();
        }
    }
}