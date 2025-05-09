using EkgAnalysisPlatform.AnalysisService.API.DTOs;
using EkgAnalysisPlatform.AnalysisService.Domain.Models;
using EkgAnalysisPlatform.AnalysisService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EkgAnalysisPlatform.AnalysisService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisRequestRepository _requestRepository;
        private readonly IAnalysisResultRepository _resultRepository;
        private readonly ILogger<AnalysisController> _logger;
        
        public AnalysisController(
            IAnalysisRequestRepository requestRepository,
            IAnalysisResultRepository resultRepository,
            ILogger<AnalysisController> logger)
        {
            _requestRepository = requestRepository;
            _resultRepository = resultRepository;
            _logger = logger;
        }
        
        [HttpGet("requests")]
        [ProducesResponseType(typeof(IEnumerable<AnalysisRequestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AnalysisRequestDto>>> GetAllRequests()
        {
            try
            {
                var requests = await _requestRepository.GetAllAsync();
                var requestDtos = requests.Select(r => new AnalysisRequestDto
                {
                    Id = r.Id,
                    SignalReference = r.SignalReference,
                    PatientCode = r.PatientCode,
                    RequestedAt = r.RequestedAt,
                    Status = r.Status.ToString(),
                    RequestedBy = r.RequestedBy,
                    AnalysisType = r.AnalysisType,
                    Priority = r.Priority
                });
                
                return Ok(requestDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis requests");
                return StatusCode(500, "An error occurred while retrieving analysis requests");
            }
        }
        
        [HttpGet("requests/{id}")]
        [ProducesResponseType(typeof(AnalysisRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AnalysisRequestDto>> GetRequestById(int id)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(id);
                
                if (request == null)
                {
                    return NotFound($"Analysis request with ID {id} not found");
                }
                
                var requestDto = new AnalysisRequestDto
                {
                    Id = request.Id,
                    SignalReference = request.SignalReference,
                    PatientCode = request.PatientCode,
                    RequestedAt = request.RequestedAt,
                    Status = request.Status.ToString(),
                    RequestedBy = request.RequestedBy,
                    AnalysisType = request.AnalysisType,
                    Priority = request.Priority
                };
                
                return Ok(requestDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis request with ID {RequestId}", id);
                return StatusCode(500, "An error occurred while retrieving the analysis request");
            }
        }
        
        [HttpPost("requests")]
        [ProducesResponseType(typeof(AnalysisRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AnalysisRequestDto>> CreateRequest([FromBody] CreateAnalysisRequestDto createRequestDto)
        {
            try
            {
                var request = new AnalysisRequest
                {
                    SignalReference = createRequestDto.SignalReference,
                    PatientCode = createRequestDto.PatientCode,
                    RequestedAt = DateTime.UtcNow,
                    Status = AnalysisStatus.Pending,
                    RequestedBy = createRequestDto.RequestedBy,
                    AnalysisType = createRequestDto.AnalysisType,
                    Priority = createRequestDto.Priority
                };
                
                var id = await _requestRepository.AddAsync(request);
                
                var createdRequestDto = new AnalysisRequestDto
                {
                    Id = id,
                    SignalReference = request.SignalReference,
                    PatientCode = request.PatientCode,
                    RequestedAt = request.RequestedAt,
                    Status = request.Status.ToString(),
                    RequestedBy = request.RequestedBy,
                    AnalysisType = request.AnalysisType,
                    Priority = request.Priority
                };
                
                return CreatedAtAction(nameof(GetRequestById), new { id }, createdRequestDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating analysis request");
                return StatusCode(500, "An error occurred while creating the analysis request");
            }
        }
        
        [HttpGet("results")]
        [ProducesResponseType(typeof(IEnumerable<AnalysisResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AnalysisResultDto>>> GetAllResults()
        {
            try
            {
                var results = await _resultRepository.GetAllAsync();
                var resultDtos = results.Select(r => new AnalysisResultDto
                {
                    Id = r.Id,
                    SignalReference = r.SignalReference,
                    PatientCode = r.PatientCode,
                    HeartRate = r.HeartRate,
                    HasArrhythmia = r.HasArrhythmia,
                    QRSDuration = r.QRSDuration,
                    PRInterval = r.PRInterval,
                    QTInterval = r.QTInterval,
                    AnalyzedAt = r.AnalyzedAt,
                    AnalyzerVersion = r.AnalyzerVersion,
                    Status = r.Status.ToString(),
                    DiagnosticNotes = r.DiagnosticNotes
                });
                
                return Ok(resultDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis results");
                return StatusCode(500, "An error occurred while retrieving analysis results");
            }
        }
        
        [HttpGet("results/{id}")]
        [ProducesResponseType(typeof(AnalysisResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AnalysisResultDto>> GetResultById(int id)
        {
            try
            {
                var result = await _resultRepository.GetByIdAsync(id);
                
                if (result == null)
                {
                    return NotFound($"Analysis result with ID {id} not found");
                }
                
                var resultDto = new AnalysisResultDto
                {
                    Id = result.Id,
                    SignalReference = result.SignalReference,
                    PatientCode = result.PatientCode,
                    HeartRate = result.HeartRate,
                    HasArrhythmia = result.HasArrhythmia,
                    QRSDuration = result.QRSDuration,
                    PRInterval = result.PRInterval,
                    QTInterval = result.QTInterval,
                    AnalyzedAt = result.AnalyzedAt,
                    AnalyzerVersion = result.AnalyzerVersion,
                    Status = result.Status.ToString(),
                    DiagnosticNotes = result.DiagnosticNotes
                };
                
                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis result with ID {ResultId}", id);
                return StatusCode(500, "An error occurred while retrieving the analysis result");
            }
        }
        
        [HttpGet("results/patient/{patientCode}")]
        [ProducesResponseType(typeof(IEnumerable<AnalysisResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AnalysisResultDto>>> GetResultsByPatientCode(string patientCode)
        {
            try
            {
                var results = await _resultRepository.GetByPatientCodeAsync(patientCode);
                var resultDtos = results.Select(r => new AnalysisResultDto
                {
                    Id = r.Id,
                    SignalReference = r.SignalReference,
                    PatientCode = r.PatientCode,
                    HeartRate = r.HeartRate,
                    HasArrhythmia = r.HasArrhythmia,
                    QRSDuration = r.QRSDuration,
                    PRInterval = r.PRInterval,
                    QTInterval = r.QTInterval,
                    AnalyzedAt = r.AnalyzedAt,
                    AnalyzerVersion = r.AnalyzerVersion,
                    Status = r.Status.ToString(),
                    DiagnosticNotes = r.DiagnosticNotes
                });
                
                return Ok(resultDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis results for patient {PatientCode}", patientCode);
                return StatusCode(500, "An error occurred while retrieving the analysis results");
            }
        }
    }
}