using Microsoft.AspNetCore.Mvc;

namespace EkgAnalysisPlatform.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        
        public HealthController(
            ILogger<HealthController> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<string, string>>> CheckHealth()
        {
            var healthStatus = new Dictionary<string, string>();
            var allHealthy = true;
            
            var services = _configuration.GetSection("HealthChecks:Services").Get<List<ServiceHealthCheck>>();
            
            if (services == null || !services.Any())
            {
                return Ok(new Dictionary<string, string> { { "Gateway", "Healthy" } });
            }
            
            foreach (var service in services)
            {
                try
                {
                    var response = await _httpClient.GetAsync(service.Uri);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        healthStatus.Add(service.Name, "Healthy");
                    }
                    else
                    {
                        healthStatus.Add(service.Name, $"Unhealthy ({(int)response.StatusCode})");
                        allHealthy = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking health for service {ServiceName}", service.Name);
                    healthStatus.Add(service.Name, $"Unhealthy ({ex.Message})");
                    allHealthy = false;
                }
            }
            
            healthStatus.Add("Gateway", "Healthy");
            
            if (!allHealthy)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, healthStatus);
            }
            
            return Ok(healthStatus);
        }
        
        private class ServiceHealthCheck
        {
            public string Name { get; set; } = string.Empty;
            public string Uri { get; set; } = string.Empty;
        }
    }
}