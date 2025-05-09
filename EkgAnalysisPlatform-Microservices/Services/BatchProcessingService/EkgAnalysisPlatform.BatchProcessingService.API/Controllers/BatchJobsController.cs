// In BatchProcessingService.API/Controllers/BatchJobsController.cs
using EkgAnalysisPlatform.BatchProcessingService.API.DTOs;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Models;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EkgAnalysisPlatform.BatchProcessingService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BatchJobsController : ControllerBase
    {
        private readonly IBatchJobRepository _jobRepository;
        private readonly IBatchJobItemRepository _jobItemRepository;
        private readonly ILogger<BatchJobsController> _logger;
        
        public BatchJobsController(
            IBatchJobRepository jobRepository,
            IBatchJobItemRepository jobItemRepository,
            ILogger<BatchJobsController> logger)
        {
            _jobRepository = jobRepository;
            _jobItemRepository = jobItemRepository;
            _logger = logger;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BatchJobDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BatchJobDto>>> GetAllJobs()
        {
            try
            {
                var jobs = await _jobRepository.GetAllAsync();
                var jobDtos = jobs.Select(j => new BatchJobDto
                {
                    Id = j.Id,
                    JobId = j.JobId,
                    CreatedAt = j.CreatedAt,
                    StartedAt = j.StartedAt,
                    CompletedAt = j.CompletedAt,
                    Status = j.Status.ToString(),
                    JobType = j.JobType,
                    TotalItems = j.TotalItems,
                    ProcessedItems = j.ProcessedItems,
                    SuccessfulItems = j.SuccessfulItems,
                    FailedItems = j.FailedItems,
                    ErrorMessage = j.ErrorMessage,
                    Parameters = j.Parameters
                });
                
                return Ok(jobDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving batch jobs");
                return StatusCode(500, "An error occurred while retrieving batch jobs");
            }
        }
        
        [HttpGet("{jobId}")]
        [ProducesResponseType(typeof(BatchJobDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BatchJobDto>> GetJobById(string jobId)
        {
            try
            {
                var job = await _jobRepository.GetByJobIdAsync(jobId);
                
                if (job == null)
                {
                    return NotFound($"Batch job with ID {jobId} not found");
                }
                
                var jobDto = new BatchJobDto
                {
                    Id = job.Id,
                    JobId = job.JobId,
                    CreatedAt = job.CreatedAt,
                    StartedAt = job.StartedAt,
                    CompletedAt = job.CompletedAt,
                    Status = job.Status.ToString(),
                    JobType = job.JobType,
                    TotalItems = job.TotalItems,
                    ProcessedItems = job.ProcessedItems,
                    SuccessfulItems = job.SuccessfulItems,
                    FailedItems = job.FailedItems,
                    ErrorMessage = job.ErrorMessage,
                    Parameters = job.Parameters
                };
                
                return Ok(jobDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving batch job with ID {JobId}", jobId);
                return StatusCode(500, "An error occurred while retrieving the batch job");
            }
        }
        
        [HttpGet("{jobId}/items")]
        [ProducesResponseType(typeof(IEnumerable<BatchJobItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BatchJobItemDto>>> GetJobItems(string jobId)
        {
            try
            {
                var job = await _jobRepository.GetByJobIdAsync(jobId);
                
                if (job == null)
                {
                    return NotFound($"Batch job with ID {jobId} not found");
                }
                
                var items = await _jobItemRepository.GetAllByBatchJobIdAsync(jobId);
                var itemDtos = items.Select(i => new BatchJobItemDto
                {
                    Id = i.Id,
                    BatchJobId = i.BatchJobId,
                    ItemId = i.ItemId,
                    CreatedAt = i.CreatedAt,
                    ProcessedAt = i.ProcessedAt,
                    Status = i.Status.ToString(),
                    ErrorMessage = i.ErrorMessage,
                    RetryCount = i.RetryCount,
                    NextRetryAt = i.NextRetryAt
                });
                
                return Ok(itemDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving items for batch job with ID {JobId}", jobId);
                return StatusCode(500, "An error occurred while retrieving the batch job items");
            }
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(BatchJobDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BatchJobDto>> CreateJob([FromBody] CreateBatchJobDto createJobDto)
        {
            try
            {
                // Create the batch job
                var job = new BatchJob
                {
                    JobId = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    Status = BatchJobStatus.Pending,
                    JobType = createJobDto.JobType,
                    TotalItems = createJobDto.ItemIds.Count,
                    ProcessedItems = 0,
                    SuccessfulItems = 0,
                    FailedItems = 0,
                    Parameters = createJobDto.Parameters
                };
                
                var jobId = await _jobRepository.AddAsync(job);
                
                // Create the batch job items
                var items = createJobDto.ItemIds.Select(itemId => new BatchJobItem
                {
                    BatchJobId = job.JobId,
                    ItemId = itemId,
                    CreatedAt = DateTime.UtcNow,
                    Status = BatchJobItemStatus.Pending
                }).ToList();
                
                await _jobItemRepository.AddRangeAsync(items);
                
                var createdJobDto = new BatchJobDto
                {
                    Id = jobId,
                    JobId = job.JobId,
                    CreatedAt = job.CreatedAt,
                    Status = job.Status.ToString(),
                    JobType = job.JobType,
                    TotalItems = job.TotalItems,
                    ProcessedItems = job.ProcessedItems,
                    SuccessfulItems = job.SuccessfulItems,
                    FailedItems = job.FailedItems,
                    Parameters = job.Parameters
                };
                
                return CreatedAtAction(nameof(GetJobById), new { jobId = job.JobId }, createdJobDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating batch job");
                return StatusCode(500, "An error occurred while creating the batch job");
            }
        }
        
        [HttpPut("{jobId}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelJob(string jobId)
        {
            try
            {
                var job = await _jobRepository.GetByJobIdAsync(jobId);
                
                if (job == null)
                {
                    return NotFound($"Batch job with ID {jobId} not found");
                }
                
                // Only allow cancellation if the job is pending or running
                if (job.Status != BatchJobStatus.Pending && job.Status != BatchJobStatus.Running)
                {
                    return BadRequest($"Cannot cancel job with status {job.Status}");
                }
                
                await _jobRepository.UpdateStatusAsync(jobId, BatchJobStatus.Cancelled);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling batch job with ID {JobId}", jobId);
                return StatusCode(500, "An error occurred while canceling the batch job");
            }
        }
    }
}