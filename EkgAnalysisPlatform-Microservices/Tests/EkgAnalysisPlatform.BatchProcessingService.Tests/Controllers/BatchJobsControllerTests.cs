using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EkgAnalysisPlatform.BatchProcessingService.API.Controllers;
using EkgAnalysisPlatform.BatchProcessingService.API.DTOs;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Models;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EkgAnalysisPlatform.BatchProcessingService.Tests.Controllers
{
    public class BatchJobsControllerTests
    {
        private readonly Mock<IBatchJobRepository> _jobRepositoryMock;
        private readonly Mock<IBatchJobItemRepository> _jobItemRepositoryMock;
        private readonly Mock<ILogger<BatchJobsController>> _loggerMock;
        private readonly BatchJobsController _controller;

        public BatchJobsControllerTests()
        {
            _jobRepositoryMock = new Mock<IBatchJobRepository>();
            _jobItemRepositoryMock = new Mock<IBatchJobItemRepository>();
            _loggerMock = new Mock<ILogger<BatchJobsController>>();
            _controller = new BatchJobsController(
                _jobRepositoryMock.Object,
                _jobItemRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllJobs_ReturnsOkResult_WithListOfJobs()
        {
            // Arrange
            var jobs = new List<BatchJob>
            {
                new BatchJob { 
                    Id = 1, 
                    JobId = "JOB001", 
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    StartedAt = DateTime.UtcNow.AddHours(-2),
                    CompletedAt = DateTime.UtcNow.AddHours(-1),
                    Status = BatchJobStatus.Completed,
                    JobType = "SignalAnalysis",
                    TotalItems = 10,
                    ProcessedItems = 10,
                    SuccessfulItems = 8,
                    FailedItems = 2,
                    Parameters = new Dictionary<string, string> { { "Algorithm", "StandardEKG" } }
                },
                new BatchJob { 
                    Id = 2, 
                    JobId = "JOB002", 
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    StartedAt = DateTime.UtcNow.AddMinutes(-30),
                    Status = BatchJobStatus.Running,
                    JobType = "SignalAnalysis",
                    TotalItems = 20,
                    ProcessedItems = 5,
                    SuccessfulItems = 5,
                    FailedItems = 0,
                    Parameters = new Dictionary<string, string> { { "Algorithm", "ExtendedEKG" } }
                }
            };
            
            _jobRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(jobs);

            // Act
            var result = await _controller.GetAllJobs();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedJobs = okResult.Value.Should().BeAssignableTo<IEnumerable<BatchJobDto>>().Subject;
            returnedJobs.Should().HaveCount(2);
            returnedJobs.First().Id.Should().Be(1);
            returnedJobs.First().JobId.Should().Be("JOB001");
            returnedJobs.First().Status.Should().Be("Completed");
            returnedJobs.Last().Id.Should().Be(2);
            returnedJobs.Last().JobId.Should().Be("JOB002");
            returnedJobs.Last().Status.Should().Be("Running");
        }

        [Fact]
        public async Task GetJobById_WithValidId_ReturnsOkResultWithJob()
        {
            // Arrange
            var job = new BatchJob { 
                Id = 1, 
                JobId = "JOB001", 
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                StartedAt = DateTime.UtcNow.AddHours(-2),
                CompletedAt = DateTime.UtcNow.AddHours(-1),
                Status = BatchJobStatus.Completed,
                JobType = "SignalAnalysis",
                TotalItems = 10,
                ProcessedItems = 10,
                SuccessfulItems = 8,
                FailedItems = 2,
                Parameters = new Dictionary<string, string> { { "Algorithm", "StandardEKG" } }
            };
            
            _jobRepositoryMock.Setup(r => r.GetByJobIdAsync("JOB001")).ReturnsAsync(job);

            // Act
            var result = await _controller.GetJobById("JOB001");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedJob = okResult.Value.Should().BeAssignableTo<BatchJobDto>().Subject;
            returnedJob.Id.Should().Be(1);
            returnedJob.JobId.Should().Be("JOB001");
            returnedJob.Status.Should().Be("Completed");
            returnedJob.TotalItems.Should().Be(10);
            returnedJob.SuccessfulItems.Should().Be(8);
            returnedJob.FailedItems.Should().Be(2);
        }

        [Fact]
        public async Task GetJobById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _jobRepositoryMock.Setup(r => r.GetByJobIdAsync("INVALID")).ReturnsAsync((BatchJob)null);

            // Act
            var result = await _controller.GetJobById("INVALID");

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetJobItems_WithValidJobId_ReturnsOkResultWithItems()
        {
            // Arrange
            var job = new BatchJob { 
                Id = 1, 
                JobId = "JOB001", 
                Status = BatchJobStatus.Completed
            };
            
            var jobItems = new List<BatchJobItem>
            {
                new BatchJobItem { 
                    Id = 1, 
                    BatchJobId = "JOB001", 
                    ItemId = "SIG001",
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    ProcessedAt = DateTime.UtcNow.AddHours(-2),
                    Status = BatchJobItemStatus.Completed
                },
                new BatchJobItem { 
                    Id = 2, 
                    BatchJobId = "JOB001", 
                    ItemId = "SIG002",
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    ProcessedAt = DateTime.UtcNow.AddHours(-2),
                    Status = BatchJobItemStatus.Failed,
                    ErrorMessage = "Signal data corrupted",
                    RetryCount = 2
                }
            };
            
            _jobRepositoryMock.Setup(r => r.GetByJobIdAsync("JOB001")).ReturnsAsync(job);
            _jobItemRepositoryMock.Setup(r => r.GetAllByBatchJobIdAsync("JOB001")).ReturnsAsync(jobItems);

            // Act
            var result = await _controller.GetJobItems("JOB001");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedItems = okResult.Value.Should().BeAssignableTo<IEnumerable<BatchJobItemDto>>().Subject;
            returnedItems.Should().HaveCount(2);
            returnedItems.First().Id.Should().Be(1);
            returnedItems.First().BatchJobId.Should().Be("JOB001");
            returnedItems.First().ItemId.Should().Be("SIG001");
            returnedItems.First().Status.Should().Be("Completed");
            returnedItems.Last().Id.Should().Be(2);
            returnedItems.Last().BatchJobId.Should().Be("JOB001");
            returnedItems.Last().ItemId.Should().Be("SIG002");
            returnedItems.Last().Status.Should().Be("Failed");
            returnedItems.Last().ErrorMessage.Should().Be("Signal data corrupted");
        }

        [Fact]
        public async Task CreateJob_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createJobDto = new CreateBatchJobDto
            {
                JobType = "SignalAnalysis",
                Parameters = new Dictionary<string, string> { { "Algorithm", "StandardEKG" } },
                ItemIds = new List<string> { "SIG001", "SIG002", "SIG003" }
            };
            
            _jobRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BatchJob>())).ReturnsAsync(1);
            _jobItemRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<BatchJobItem>>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateJob(createJobDto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(BatchJobsController.GetJobById));
            
            var returnedJob = createdAtActionResult.Value.Should().BeAssignableTo<BatchJobDto>().Subject;
            returnedJob.Id.Should().Be(1);
            returnedJob.JobType.Should().Be("SignalAnalysis");
            returnedJob.TotalItems.Should().Be(3);
            returnedJob.Status.Should().Be("Pending");
        }

        [Fact]
        public async Task CancelJob_WithValidJobId_ReturnsNoContent()
        {
            // Arrange
            var job = new BatchJob { 
                Id = 1, 
                JobId = "JOB001", 
                Status = BatchJobStatus.Running
            };
            
            _jobRepositoryMock.Setup(r => r.GetByJobIdAsync("JOB001")).ReturnsAsync(job);
            _jobRepositoryMock.Setup(r => r.UpdateStatusAsync("JOB001", BatchJobStatus.Cancelled)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CancelJob("JOB001");

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _jobRepositoryMock.Verify(r => r.UpdateStatusAsync("JOB001", BatchJobStatus.Cancelled), Times.Once);
        }

        [Fact]
        public async Task CancelJob_WithCompletedJob_ReturnsBadRequest()
        {
            // Arrange
            var job = new BatchJob { 
                Id = 1, 
                JobId = "JOB001", 
                Status = BatchJobStatus.Completed
            };
            
            _jobRepositoryMock.Setup(r => r.GetByJobIdAsync("JOB001")).ReturnsAsync(job);

            // Act
            var result = await _controller.CancelJob("JOB001");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}

namespace EkgAnalysisPlatform.BatchProcessingService.Tests.Controllers
{
    public class ScheduleConfigurationsControllerTests
    {
        private readonly Mock<IScheduleConfigurationRepository> _repositoryMock;
        private readonly Mock<ILogger<ScheduleConfigurationsController>> _loggerMock;
        private readonly ScheduleConfigurationsController _controller;

        public ScheduleConfigurationsControllerTests()
        {
            _repositoryMock = new Mock<IScheduleConfigurationRepository>();
            _loggerMock = new Mock<ILogger<ScheduleConfigurationsController>>();
            _controller = new ScheduleConfigurationsController(
                _repositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfConfigurations()
        {
            // Arrange
            var configs = new List<ScheduleConfiguration>
            {
                new ScheduleConfiguration { 
                    Id = 1, 
                    JobType = "SignalAnalysis", 
                    CronExpression = "0 0 * * *", // Daily at midnight
                    IsEnabled = true,
                    MaxBatchSize = 100,
                    MaxConcurrentJobs = 2,
                    MaxRetries = 3,
                    RetryDelayInMinutes = 5
                },
                new ScheduleConfiguration { 
                    Id = 2, 
                    JobType = "DataCleanup", 
                    CronExpression = "0 0 * * 0", // Weekly on Sunday
                    IsEnabled = false,
                    MaxBatchSize = 1000,
                    MaxConcurrentJobs = 1,
                    MaxRetries = 1,
                    RetryDelayInMinutes = 10
                }
            };
            
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(configs);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedConfigs = okResult.Value.Should().BeAssignableTo<IEnumerable<ScheduleConfigurationDto>>().Subject;
            returnedConfigs.Should().HaveCount(2);
            returnedConfigs.First().Id.Should().Be(1);
            returnedConfigs.First().JobType.Should().Be("SignalAnalysis");
            returnedConfigs.First().IsEnabled.Should().BeTrue();
            returnedConfigs.Last().Id.Should().Be(2);
            returnedConfigs.Last().JobType.Should().Be("DataCleanup");
            returnedConfigs.Last().IsEnabled.Should().BeFalse();
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkResultWithConfiguration()
        {
            // Arrange
            var config = new ScheduleConfiguration { 
                Id = 1, 
                JobType = "SignalAnalysis", 
                CronExpression = "0 0 * * *", // Daily at midnight
                IsEnabled = true,
                MaxBatchSize = 100,
                MaxConcurrentJobs = 2,
                MaxRetries = 3,
                RetryDelayInMinutes = 5
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(config);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedConfig = okResult.Value.Should().BeAssignableTo<ScheduleConfigurationDto>().Subject;
            returnedConfig.Id.Should().Be(1);
            returnedConfig.JobType.Should().Be("SignalAnalysis");
            returnedConfig.CronExpression.Should().Be("0 0 * * *");
            returnedConfig.IsEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task GetByJobType_WithValidType_ReturnsOkResultWithConfiguration()
        {
            // Arrange
            var config = new ScheduleConfiguration { 
                Id = 1, 
                JobType = "SignalAnalysis", 
                CronExpression = "0 0 * * *", // Daily at midnight
                IsEnabled = true,
                MaxBatchSize = 100,
                MaxConcurrentJobs = 2,
                MaxRetries = 3,
                RetryDelayInMinutes = 5
            };
            
            _repositoryMock.Setup(r => r.GetByJobTypeAsync("SignalAnalysis")).ReturnsAsync(config);

            // Act
            var result = await _controller.GetByJobType("SignalAnalysis");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedConfig = okResult.Value.Should().BeAssignableTo<ScheduleConfigurationDto>().Subject;
            returnedConfig.Id.Should().Be(1);
            returnedConfig.JobType.Should().Be("SignalAnalysis");
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createConfigDto = new CreateScheduleConfigurationDto
            {
                JobType = "NewJobType",
                CronExpression = "0 0 * * 1-5", // Weekdays at midnight
                IsEnabled = true,
                MaxBatchSize = 50,
                MaxConcurrentJobs = 1,
                MaxRetries = 2,
                RetryDelayInMinutes = 15
            };
            
            _repositoryMock.Setup(r => r.GetByJobTypeAsync("NewJobType")).ReturnsAsync((ScheduleConfiguration)null);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<ScheduleConfiguration>())).ReturnsAsync(3);

            // Act
            var result = await _controller.Create(createConfigDto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(ScheduleConfigurationsController.GetById));
            createdAtActionResult.RouteValues["id"].Should().Be(3);
            
            var returnedConfig = createdAtActionResult.Value.Should().BeAssignableTo<ScheduleConfigurationDto>().Subject;
            returnedConfig.Id.Should().Be(3);
            returnedConfig.JobType.Should().Be("NewJobType");
            returnedConfig.CronExpression.Should().Be("0 0 * * 1-5");
        }

        [Fact]
        public async Task Create_WithDuplicateJobType_ReturnsBadRequest()
        {
            // Arrange
            var createConfigDto = new CreateScheduleConfigurationDto
            {
                JobType = "ExistingJobType",
                CronExpression = "0 0 * * 1-5",
                IsEnabled = true
            };
            
            var existingConfig = new ScheduleConfiguration { 
                Id = 1, 
                JobType = "ExistingJobType", 
                CronExpression = "0 0 * * *"
            };
            
            _repositoryMock.Setup(r => r.GetByJobTypeAsync("ExistingJobType")).ReturnsAsync(existingConfig);

            // Act
            var result = await _controller.Create(createConfigDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_WithValidIdAndData_ReturnsNoContent()
        {
            // Arrange
            var updateConfigDto = new ScheduleConfigurationDto
            {
                Id = 1,
                JobType = "SignalAnalysis",
                CronExpression = "0 0 * * 1-5", // Updated cron expression
                IsEnabled = false, // Disable the job
                MaxBatchSize = 200,
                MaxConcurrentJobs = 3,
                MaxRetries = 5,
                RetryDelayInMinutes = 10
            };
            
            var existingConfig = new ScheduleConfiguration { 
                Id = 1, 
                JobType = "SignalAnalysis", 
                CronExpression = "0 0 * * *",
                IsEnabled = true,
                MaxBatchSize = 100,
                MaxConcurrentJobs = 2,
                MaxRetries = 3,
                RetryDelayInMinutes = 5
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingConfig);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ScheduleConfiguration>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(1, updateConfigDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<ScheduleConfiguration>(c => 
                c.CronExpression == "0 0 * * 1-5" && 
                c.IsEnabled == false &&
                c.MaxBatchSize == 200)), Times.Once);
        }

        [Fact]
        public async Task ToggleEnabled_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var existingConfig = new ScheduleConfiguration { 
                Id = 1, 
                JobType = "SignalAnalysis", 
                IsEnabled = true
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingConfig);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ScheduleConfiguration>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ToggleEnabled(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<ScheduleConfiguration>(c => 
                c.IsEnabled == false)), Times.Once);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var existingConfig = new ScheduleConfiguration { 
                Id = 1, 
                JobType = "SignalAnalysis"
            };
            
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingConfig);
            _repositoryMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}