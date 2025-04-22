using EkgAnalysisPlatform.Core.Interfaces;
using EkgAnalysisPlatform.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EkgAnalysisPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EkgSignalsController : ControllerBase
{
    private readonly IEkgSignalRepository _repository;
    private readonly IEkgAnalysisService _analysisService;

    public EkgSignalsController(IEkgSignalRepository repository, IEkgAnalysisService analysisService)
    {
        _repository = repository;
        _analysisService = analysisService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EkgSignal>>> GetAll()
    {
        var signals = await _repository.GetAllAsync();
        return Ok(signals);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EkgSignal>> GetById(int id)
    {
        var signal = await _repository.GetByIdAsync(id);
        if (signal == null)
            return NotFound();

        return Ok(signal);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<IEnumerable<EkgSignal>>> GetByPatientId(int patientId)
    {
        var signals = await _repository.GetByPatientIdAsync(patientId);
        return Ok(signals);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(EkgSignal signal)
    {
        var id = await _repository.AddAsync(signal);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPost("{id}/analyze")]
    public async Task<ActionResult<AnalysisResult>> Analyze(int id)
    {
        var signal = await _repository.GetByIdAsync(id);
        if (signal == null)
            return NotFound();

        var result = await _analysisService.AnalyzeSignalAsync(signal);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}