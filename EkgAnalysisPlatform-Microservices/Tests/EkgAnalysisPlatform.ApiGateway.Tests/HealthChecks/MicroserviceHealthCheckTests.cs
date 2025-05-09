namespace EkgAnalysisPlatform.ApiGateway.Tests.HealthChecks
{
    public class MicroserviceHealthCheckTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<MicroserviceHealthCheck>> _loggerMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private HttpClient _httpClient;
        private MicroserviceHealthCheck _healthCheck;

        public MicroserviceHealthCheckTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<MicroserviceHealthCheck>>();
            _handlerMock = new Mock<HttpMessageHandler>();
            
            // Setup HTTP client with mocked handler
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://testserver/")
            };
            
            _httpClientFactoryMock
                .Setup(x => x.CreateClient("HealthCheck"))
                .Returns(_httpClient);

            _healthCheck = new MicroserviceHealthCheck(
                "TestService",
                "http://test-service/health",
                _httpClientFactoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CheckHealthAsync_ServiceIsHealthy_ReturnsHealthy()
        {
            // Arrange
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{ \"status\": \"Healthy\" }")
                });

            // Act
            var result = await _healthCheck.CheckHealthAsync(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext());

            // Assert
            result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy);
            result.Description.Should().Be("TestService is healthy.");
        }

        [Fact]
        public async Task CheckHealthAsync_ServiceIsUnhealthy_ReturnsUnhealthy()
        {
            // Arrange
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("{ \"error\": \"Service unavailable\" }")
                });

            // Act
            var result = await _healthCheck.CheckHealthAsync(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext());

            // Assert
            result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);
            result.Description.Should().StartWith("TestService is unhealthy. Status code: InternalServerError");
        }

        [Fact]
        public async Task CheckHealthAsync_ExceptionThrown_ReturnsUnhealthy()
        {
            // Arrange
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection refused"));

            // Act
            var result = await _healthCheck.CheckHealthAsync(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext());

            // Assert
            result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);
            result.Description.Should().StartWith("TestService is unhealthy. Error: Connection refused");
        }
    }
}