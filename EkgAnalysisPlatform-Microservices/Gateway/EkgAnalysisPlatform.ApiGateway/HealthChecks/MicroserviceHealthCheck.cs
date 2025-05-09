using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EkgAnalysisPlatform.ApiGateway.HealthChecks
{
    public class MicroserviceHealthCheck : IHealthCheck
    {
        private readonly string _serviceName;
        private readonly string _healthCheckUri;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MicroserviceHealthCheck> _logger;

        public MicroserviceHealthCheck(
            string serviceName,
            string healthCheckUri,
            IHttpClientFactory httpClientFactory,
            ILogger<MicroserviceHealthCheck> logger)
        {
            _serviceName = serviceName;
            _healthCheckUri = healthCheckUri;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Checking health for service {ServiceName} at {Uri}", _serviceName, _healthCheckUri);

            try
            {
                var httpClient = _httpClientFactory.CreateClient("HealthCheck");
                var response = await httpClient.GetAsync(_healthCheckUri, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy($"{_serviceName} is healthy.");
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return HealthCheckResult.Unhealthy(
                    $"{_serviceName} is unhealthy. Status code: {response.StatusCode}. Response: {content}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for service {ServiceName}", _serviceName);
                return HealthCheckResult.Unhealthy($"{_serviceName} is unhealthy. Error: {ex.Message}");
            }
        }
    }

    public class ServiceHealthCheck
    {
        public string Name { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
    }

    public class HealthChecksOptions
    {
        public List<ServiceHealthCheck> Services { get; set; } = new List<ServiceHealthCheck>();
    }

    public static class HealthChecksExtensions
    {
        public static IHealthChecksBuilder AddMicroserviceHealthChecks(
            this IHealthChecksBuilder builder,
            IConfiguration configuration)
        {
            var services = configuration
                .GetSection("HealthChecks:Services")
                .Get<List<ServiceHealthCheck>>() ?? new List<ServiceHealthCheck>();

            foreach (var service in services)
            {
                builder.Add(new HealthCheckRegistration(
                    service.Name,
                    sp => new MicroserviceHealthCheck(
                        service.Name,
                        service.Uri,
                        sp.GetRequiredService<IHttpClientFactory>(),
                        sp.GetRequiredService<ILogger<MicroserviceHealthCheck>>()
                    ),
                    null,
                    null));
            }

            return builder;
        }
    }
}