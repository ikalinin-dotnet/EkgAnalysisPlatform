using EkgAnalysisPlatform.EkgSignalService.API.DTOs;
using EkgAnalysisPlatform.EkgSignalService.Domain.Models;
using EkgAnalysisPlatform.EkgSignalService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EkgAnalysisPlatform.EkgSignalService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EkgSignalsController : ControllerBase
    {
        private readonly IEkgSignalRepository _repository;
        private readonly ILogger<EkgSignalsController> _logger;
        
        public EkgSignalsController(IEkgSignalRepository repository, ILogger<EkgSignalsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EkgSignalDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EkgSignalDto>>> GetAll()
        {
            try
            {
                var signals = await _repository.GetAllAsync();
                var signalDtos = signals.Select(s => new EkgSignalDto
                {
                    Id = s.Id,
                    PatientCode = s.PatientCode,
                    RecordedAt = s.RecordedAt,
                    SamplingRate = s.SamplingRate,
                    Notes = s.Notes,
                    DeviceId = s.DeviceId,
                    RecordedBy = s.RecordedBy,
                    IsProcessed = s.IsProcessed,
                    DataPointsCount = s.DataPoints.Length
                });
                
                return Ok(signalDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EKG signals");
                return StatusCode(500, "An error occurred while retrieving EKG signals");
            }
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EkgSignalDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EkgSignalDto>> GetById(int id)
        {
            try
            {
                var signal = await _repository.GetByIdAsync(id);
                
                if (signal == null)
                {
                    return NotFound($"EKG signal with ID {id} not found");
                }
                
                var signalDto = new EkgSignalDto
                {
                    Id = signal.Id,
                    PatientCode = signal.PatientCode,
                    RecordedAt = signal.RecordedAt,
                    SamplingRate = signal.SamplingRate,
                    Notes = signal.Notes,
                    DeviceId = signal.DeviceId,
                    RecordedBy = signal.RecordedBy,
                    IsProcessed = signal.IsProcessed,
                    DataPointsCount = signal.DataPoints.Length
                };
                
                return Ok(signalDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EKG signal with ID {SignalId}", id);
                return StatusCode(500, "An error occurred while retrieving the EKG signal");
            }
        }
        
        [HttpGet("{id}/data")]
        [ProducesResponseType(typeof(double[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<double[]>> GetSignalData(int id)
        {
            try
            {
                var signal = await _repository.GetByIdAsync(id);
                
                if (signal == null)
                {
                    return NotFound($"EKG signal with ID {id} not found");
                }
                
                return Ok(signal.DataPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data for EKG signal with ID {SignalId}", id);
                return StatusCode(500, "An error occurred while retrieving the EKG signal data");
            }
        }
        
        [HttpGet("patient/{patientCode}")]
        [ProducesResponseType(typeof(IEnumerable<EkgSignalDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EkgSignalDto>>> GetByPatientCode(string patientCode)
        {
            try
            {
                var signals = await _repository.GetByPatientCodeAsync(patientCode);
                var signalDtos = signals.Select(s => new EkgSignalDto
                {
                    Id = s.Id,
                    PatientCode = s.PatientCode,
                    RecordedAt = s.RecordedAt,
                    SamplingRate = s.SamplingRate,
                    Notes = s.Notes,
                    DeviceId = s.DeviceId,
                    RecordedBy = s.RecordedBy,
                    IsProcessed = s.IsProcessed,
                    DataPointsCount = s.DataPoints.Length
                });
                
                return Ok(signalDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EKG signals for patient {PatientCode}", patientCode);
                return StatusCode(500, "An error occurred while retrieving the EKG signals");
            }
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(EkgSignalDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EkgSignalDto>> Create([FromBody] CreateEkgSignalDto createSignalDto)
        {
            try
            {
                var signal = new EkgSignal
                {
                    PatientCode = createSignalDto.PatientCode,
                    RecordedAt = createSignalDto.RecordedAt,
                    DataPoints = createSignalDto.DataPoints,
                    SamplingRate = createSignalDto.SamplingRate,
                    Notes = createSignalDto.Notes,
                    DeviceId = createSignalDto.DeviceId,
                    RecordedBy = createSignalDto.RecordedBy,
                    IsProcessed = false
                };
                
                var id = await _repository.AddAsync(signal);
                
                var createdSignalDto = new EkgSignalDto
                {
                    Id = id,
                    PatientCode = signal.PatientCode,
                    RecordedAt = signal.RecordedAt,
                    SamplingRate = signal.SamplingRate,
                    Notes = signal.Notes,
                    DeviceId = signal.DeviceId,
                    RecordedBy = signal.RecordedBy,
                    IsProcessed = signal.IsProcessed,
                    DataPointsCount = signal.DataPoints.Length
                };
                
                return CreatedAtAction(nameof(GetById), new { id }, createdSignalDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating EKG signal");
                return StatusCode(500, "An error occurred while creating the EKG signal");
            }
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEkgSignalDto updateSignalDto)
        {
            try
            {
                var existingSignal = await _repository.GetByIdAsync(id);
                
                if (existingSignal == null)
                {
                    return NotFound($"EKG signal with ID {id} not found");
                }
                
                if (updateSignalDto.Notes != null)
                {
                    existingSignal.Notes = updateSignalDto.Notes;
                }
                existingSignal.IsProcessed = updateSignalDto.IsProcessed;
                
                await _repository.UpdateAsync(existingSignal);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating EKG signal with ID {SignalId}", id);
                return StatusCode(500, "An error occurred while updating the EKG signal");
            }
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingSignal = await _repository.GetByIdAsync(id);
                
                if (existingSignal == null)
                {
                    return NotFound($"EKG signal with ID {id} not found");
                }
                
                await _repository.DeleteAsync(id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting EKG signal with ID {SignalId}", id);
                return StatusCode(500, "An error occurred while deleting the EKG signal");
            }
        }
    }
}