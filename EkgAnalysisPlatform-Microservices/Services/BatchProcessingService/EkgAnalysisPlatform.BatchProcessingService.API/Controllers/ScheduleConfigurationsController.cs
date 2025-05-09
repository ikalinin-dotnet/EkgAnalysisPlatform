using EkgAnalysisPlatform.BatchProcessingService.API.DTOs;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Models;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EkgAnalysisPlatform.BatchProcessingService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleConfigurationsController : ControllerBase
    {
        private readonly IScheduleConfigurationRepository _repository;
        private readonly ILogger<ScheduleConfigurationsController> _logger;
        
        public ScheduleConfigurationsController(
            IScheduleConfigurationRepository repository,
            ILogger<ScheduleConfigurationsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ScheduleConfigurationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ScheduleConfigurationDto>>> GetAll()
        {
            try
            {
                var configs = await _repository.GetAllAsync();
                var configDtos = configs.Select(c => new ScheduleConfigurationDto
                {
                    Id = c.Id,
                    JobType = c.JobType,
                    CronExpression = c.CronExpression,
                    IsEnabled = c.IsEnabled,
                    MaxBatchSize = c.MaxBatchSize,
                    MaxConcurrentJobs = c.MaxConcurrentJobs,
                    MaxRetries = c.MaxRetries,
                    RetryDelayInMinutes = c.RetryDelayInMinutes
                });
                
                return Ok(configDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule configurations");
                return StatusCode(500, "An error occurred while retrieving schedule configurations");
            }
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ScheduleConfigurationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ScheduleConfigurationDto>> GetById(int id)
        {
            try
            {
                var config = await _repository.GetByIdAsync(id);
                
                if (config == null)
                {
                    return NotFound($"Schedule configuration with ID {id} not found");
                }
                
                var configDto = new ScheduleConfigurationDto
                {
                    Id = config.Id,
                    JobType = config.JobType,
                    CronExpression = config.CronExpression,
                    IsEnabled = config.IsEnabled,
                    MaxBatchSize = config.MaxBatchSize,
                    MaxConcurrentJobs = config.MaxConcurrentJobs,
                    MaxRetries = config.MaxRetries,
                    RetryDelayInMinutes = config.RetryDelayInMinutes
                };
                
                return Ok(configDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule configuration with ID {ConfigId}", id);
                return StatusCode(500, "An error occurred while retrieving the schedule configuration");
            }
        }
        
        [HttpGet("job-type/{jobType}")]
        [ProducesResponseType(typeof(ScheduleConfigurationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ScheduleConfigurationDto>> GetByJobType(string jobType)
        {
            try
            {
                var config = await _repository.GetByJobTypeAsync(jobType);
                
                if (config == null)
                {
                    return NotFound($"Schedule configuration for job type '{jobType}' not found");
                }
                
                var configDto = new ScheduleConfigurationDto
                {
                    Id = config.Id,
                    JobType = config.JobType,
                    CronExpression = config.CronExpression,
                    IsEnabled = config.IsEnabled,
                    MaxBatchSize = config.MaxBatchSize,
                    MaxConcurrentJobs = config.MaxConcurrentJobs,
                    MaxRetries = config.MaxRetries,
                    RetryDelayInMinutes = config.RetryDelayInMinutes
                };
                
                return Ok(configDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule configuration for job type {JobType}", jobType);
                return StatusCode(500, "An error occurred while retrieving the schedule configuration");
            }
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ScheduleConfigurationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ScheduleConfigurationDto>> Create([FromBody] CreateScheduleConfigurationDto createConfigDto)
        {
            try
            {
                // Check if a configuration for this job type already exists
                var existingConfig = await _repository.GetByJobTypeAsync(createConfigDto.JobType);
                if (existingConfig != null)
                {
                    return BadRequest($"A schedule configuration for job type '{createConfigDto.JobType}' already exists");
                }
                
                var config = new ScheduleConfiguration
                {
                    JobType = createConfigDto.JobType,
                    CronExpression = createConfigDto.CronExpression,
                    IsEnabled = createConfigDto.IsEnabled,
                    MaxBatchSize = createConfigDto.MaxBatchSize,
                    MaxConcurrentJobs = createConfigDto.MaxConcurrentJobs,
                    MaxRetries = createConfigDto.MaxRetries,
                    RetryDelayInMinutes = createConfigDto.RetryDelayInMinutes
                };
                
                var id = await _repository.AddAsync(config);
                
                var createdConfigDto = new ScheduleConfigurationDto
                {
                    Id = id,
                    JobType = config.JobType,
                    CronExpression = config.CronExpression,
                    IsEnabled = config.IsEnabled,
                    MaxBatchSize = config.MaxBatchSize,
                    MaxConcurrentJobs = config.MaxConcurrentJobs,
                    MaxRetries = config.MaxRetries,
                    RetryDelayInMinutes = config.RetryDelayInMinutes
                };
                
                return CreatedAtAction(nameof(GetById), new { id }, createdConfigDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule configuration");
                return StatusCode(500, "An error occurred while creating the schedule configuration");
            }
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] ScheduleConfigurationDto updateConfigDto)
        {
            try
            {
                if (id != updateConfigDto.Id)
                {
                    return BadRequest("ID in the URL does not match the ID in the request body");
                }
                
                var existingConfig = await _repository.GetByIdAsync(id);
                
                if (existingConfig == null)
                {
                    return NotFound($"Schedule configuration with ID {id} not found");
                }
                
                existingConfig.JobType = updateConfigDto.JobType;
                existingConfig.CronExpression = updateConfigDto.CronExpression;
                existingConfig.IsEnabled = updateConfigDto.IsEnabled;
                existingConfig.MaxBatchSize = updateConfigDto.MaxBatchSize;
                existingConfig.MaxConcurrentJobs = updateConfigDto.MaxConcurrentJobs;
                existingConfig.MaxRetries = updateConfigDto.MaxRetries;
                existingConfig.RetryDelayInMinutes = updateConfigDto.RetryDelayInMinutes;
                
                await _repository.UpdateAsync(existingConfig);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule configuration with ID {ConfigId}", id);
                return StatusCode(500, "An error occurred while updating the schedule configuration");
            }
        }
        
        [HttpPut("{id}/toggle")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleEnabled(int id)
        {
            try
            {
                var existingConfig = await _repository.GetByIdAsync(id);
                
                if (existingConfig == null)
                {
                    return NotFound($"Schedule configuration with ID {id} not found");
                }
                
                existingConfig.IsEnabled = !existingConfig.IsEnabled;
                
                await _repository.UpdateAsync(existingConfig);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling schedule configuration with ID {ConfigId}", id);
                return StatusCode(500, "An error occurred while toggling the schedule configuration");
            }
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingConfig = await _repository.GetByIdAsync(id);
                
                if (existingConfig == null)
                {
                    return NotFound($"Schedule configuration with ID {id} not found");
                }
                
                await _repository.DeleteAsync(id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule configuration with ID {ConfigId}", id);
                return StatusCode(500, "An error occurred while deleting the schedule configuration");
            }
        }
    }
}