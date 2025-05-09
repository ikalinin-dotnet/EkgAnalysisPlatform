using EkgAnalysisPlatform.PatientService.API.DTOs;
using EkgAnalysisPlatform.PatientService.Domain.Models;
using EkgAnalysisPlatform.PatientService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EkgAnalysisPlatform.PatientService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientRepository _repository;
        private readonly ILogger<PatientsController> _logger;
        
        public PatientsController(IPatientRepository repository, ILogger<PatientsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PatientDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll()
        {
            try
            {
                var patients = await _repository.GetAllAsync();
                var patientDtos = patients.Select(p => new PatientDto
                {
                    Id = p.Id,
                    PatientCode = p.PatientCode,
                    Age = p.Age,
                    Gender = p.Gender,
                    ContactInfo = p.ContactInfo
                });
                
                return Ok(patientDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients");
                return StatusCode(500, "An error occurred while retrieving patients");
            }
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientDto>> GetById(int id)
        {
            try
            {
                var patient = await _repository.GetByIdAsync(id);
                
                if (patient == null)
                {
                    return NotFound($"Patient with ID {id} not found");
                }
                
                var patientDto = new PatientDto
                {
                    Id = patient.Id,
                    PatientCode = patient.PatientCode,
                    Age = patient.Age,
                    Gender = patient.Gender,
                    ContactInfo = patient.ContactInfo
                };
                
                return Ok(patientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient with ID {PatientId}", id);
                return StatusCode(500, "An error occurred while retrieving the patient");
            }
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PatientDto>> Create([FromBody] CreatePatientDto createPatientDto)
        {
            try
            {
                var patient = new Patient
                {
                    PatientCode = createPatientDto.PatientCode,
                    Age = createPatientDto.Age,
                    Gender = createPatientDto.Gender,
                    ContactInfo = createPatientDto.ContactInfo,
                    RegisteredDate = DateTime.UtcNow
                };
                
                var id = await _repository.AddAsync(patient);
                
                var createdPatientDto = new PatientDto
                {
                    Id = id,
                    PatientCode = patient.PatientCode,
                    Age = patient.Age,
                    Gender = patient.Gender,
                    ContactInfo = patient.ContactInfo
                };
                
                return CreatedAtAction(nameof(GetById), new { id }, createdPatientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                return StatusCode(500, "An error occurred while creating the patient");
            }
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto updatePatientDto)
        {
            try
            {
                var existingPatient = await _repository.GetByIdAsync(id);
                
                if (existingPatient == null)
                {
                    return NotFound($"Patient with ID {id} not found");
                }
                
                existingPatient.Age = updatePatientDto.Age;
                existingPatient.Gender = updatePatientDto.Gender;
                existingPatient.ContactInfo = updatePatientDto.ContactInfo;
                
                await _repository.UpdateAsync(existingPatient);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient with ID {PatientId}", id);
                return StatusCode(500, "An error occurred while updating the patient");
            }
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingPatient = await _repository.GetByIdAsync(id);
                
                if (existingPatient == null)
                {
                    return NotFound($"Patient with ID {id} not found");
                }
                
                await _repository.DeleteAsync(id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient with ID {PatientId}", id);
                return StatusCode(500, "An error occurred while deleting the patient");
            }
        }
    }
}