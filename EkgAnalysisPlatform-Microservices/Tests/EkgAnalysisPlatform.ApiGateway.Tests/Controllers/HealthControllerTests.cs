using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EkgAnalysisPlatform.ApiGateway.Controllers;
using EkgAnalysisPlatform.ApiGateway.HealthChecks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace EkgAnalysisPlatform.ApiGateway.Tests.Controllers
{
    public class HealthControllerTests
    {
        private readonly Mock<ILogger<HealthController>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private HttpClient _httpClient;
        private HealthController _controller;

        public HealthControllerTests()
        {
            _loggerMock = new Mock<ILogger<HealthController>>();
            _configurationMock = new Mock<IConfiguration>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _handlerMock = new Mock<HttpMessageHandler>();
            
            // Setup HTTP client with mocked handler
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://testserver/")
            };
            
            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            _controller = new HealthController(
                _loggerMock.Object,
                _configurationMock.Object,
                _httpClientFactoryMock.Object);
        }

        [Fact]
        public async Task CheckHealth_NoConfiguredServices_ReturnsOnlyGateway()
        {
            // Arrange
            var services = new List<ServiceHealthCheck>();
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(s => s.Get<List<ServiceHealthCheck>>()).Returns(services);
            
            _configurationMock
                .Setup(c => c.GetSection("HealthChecks:Services"))
                .Returns(configSection.Object);

            // Act
            var result = await _controller.CheckHealth();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var healthStatus = okResult.Value.Should().BeAssignableTo<Dictionary<string, string>>().Subject;
            healthStatus.Should().ContainSingle();
            healthStatus.Should().ContainKey("Gateway");
            healthStatus["Gateway"].Should().Be("Healthy");
        }

        [Fact]
        public async Task CheckHealth_AllServicesHealthy_ReturnsOk()
        {
            // Arrange
            var services = new List<ServiceHealthCheck>
            {
                new ServiceHealthCheck { Name = "PatientService", Uri = "http://patient-service/health" },
                new ServiceHealthCheck { Name = "EkgSignalService", Uri = "http://ekg-signal-service/health" }
            };
            
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(s => s.Get<List<ServiceHealthCheck>>()).Returns(services);
            
            _configurationMock
                .Setup(c => c.GetSection("HealthChecks:Services"))
                .Returns(configSection.Object);

            // Setup successful HTTP responses
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var result = await _controller.CheckHealth();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var healthStatus = okResult.Value.Should().BeAssignableTo<Dictionary<string, string>>().Subject;
            healthStatus.Should().HaveCount(3); // 2 services + gateway
            healthStatus["Gateway"].Should().Be("Healthy");
            healthStatus["PatientService"].Should().Be("Healthy");
            healthStatus["EkgSignalService"].Should().Be("Healthy");
        }

        [Fact]
        public async Task CheckHealth_OneServiceUnhealthy_ReturnsServiceUnavailable()
        {
            // Arrange
            var services = new List<ServiceHealthCheck>
            {
                new ServiceHealthCheck { Name = "PatientService", Uri = "http://patient-service/health" },
                new ServiceHealthCheck { Name = "EkgSignalService", Uri = "http://ekg-signal-service/health" }
            };
            
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(s => s.Get<List<ServiceHealthCheck>>()).Returns(services);
            
            _configurationMock
                .Setup(c => c.GetSection("HealthChecks:Services"))
                .Returns(configSection.Object);

            // Setup mixed HTTP responses (one success, one failure)
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("patient-service")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });
                
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ekg-signal-service")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await _controller.CheckHealth();

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
            
            var healthStatus = statusCodeResult.Value.Should().BeAssignableTo<Dictionary<string, string>>().Subject;
            healthStatus.Should().HaveCount(3); // 2 services + gateway
            healthStatus["Gateway"].Should().Be("Healthy");
            healthStatus["PatientService"].Should().Be("Healthy");
            healthStatus["EkgSignalService"].Should().StartWith("Unhealthy");
        }

        [Fact]
        public async Task CheckHealth_ServiceThrowsException_ReturnsServiceUnavailable()
        {
            // Arrange
            var services = new List<ServiceHealthCheck>
            {
                new ServiceHealthCheck { Name = "PatientService", Uri = "http://patient-service/health" }
            };
            
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(s => s.Get<List<ServiceHealthCheck>>()).Returns(services);
            
            _configurationMock
                .Setup(c => c.GetSection("HealthChecks:Services"))
                .Returns(configSection.Object);

            // Setup exception on HTTP request
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection refused"));

            // Act
            var result = await _controller.CheckHealth();

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
            
            var healthStatus = statusCodeResult.Value.Should().BeAssignableTo<Dictionary<string, string>>().Subject;
            healthStatus.Should().HaveCount(2); // 1 service + gateway
            healthStatus["Gateway"].Should().Be("Healthy");
            healthStatus["PatientService"].Should().StartWith("Unhealthy");
        }
    }
}