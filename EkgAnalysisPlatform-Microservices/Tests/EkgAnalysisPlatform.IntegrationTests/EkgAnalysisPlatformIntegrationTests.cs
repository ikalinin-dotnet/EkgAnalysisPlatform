using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EkgAnalysisPlatform.AnalysisService.API.DTOs;
using EkgAnalysisPlatform.EkgSignalService.API.DTOs;
using EkgAnalysisPlatform.PatientService.API.DTOs;
using EkgAnalysisPlatform.BatchProcessingService.API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace EkgAnalysisPlatform.IntegrationTests
{
    public class EkgAnalysisPlatformIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public EkgAnalysisPlatformIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CompleteWorkflow_PatientToAnalysis_Success()
        {
            // 1. Create a patient
            var patientCode = $"TEST_{Guid.NewGuid():N}";
            var createPatientDto = new CreatePatientDto
            {
                PatientCode = patientCode,
                Age = 65,
                Gender = "Male",
                ContactInfo = "test@example.com"
            };
            
            var patientResponse = await _client.PostAsJsonAsync("/api/patients", createPatientDto);
            patientResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var patientLocation = patientResponse.Headers.Location?.ToString();
            patientLocation.Should().NotBeNull();

            // 2. Create an EKG signal
            var createSignalDto = new CreateEkgSignalDto
            {
                PatientCode = patientCode,
                RecordedAt = DateTime.UtcNow,
                DataPoints = GenerateTestEkgData(1000),
                SamplingRate = 250,
                DeviceId = "TEST_DEVICE",
                RecordedBy = "Dr. Test"
            };
            
            var signalResponse = await _client.PostAsJsonAsync("/api/ekgsignals", createSignalDto);
            signalResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var signalLocation = signalResponse.Headers.Location?.ToString();
            signalLocation.Should().NotBeNull();
            var signalId = signalLocation.Split('/').Last();

            // 3. Request analysis
            var createRequestDto = new CreateAnalysisRequestDto
            {
                SignalReference = signalId,
                PatientCode = patientCode,
                RequestedBy = "Dr. Test",
                AnalysisType = "Standard",
                Priority = 1
            };
            
            var analysisResponse = await _client.PostAsJsonAsync("/api/analysis/requests", createRequestDto);
            analysisResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // 4. Wait a bit for background processing
            await Task.Delay(3000);

            // 5. Check analysis results
            var resultsResponse = await _client.GetAsync($"/api/analysis/results/patient/{patientCode}");
            resultsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var results = await resultsResponse.Content.ReadFromJsonAsync<List<AnalysisResultDto>>();
            results.Should().NotBeNull();
            // Note: Results might be empty if analysis hasn't completed yet in a real scenario
        }

        [Fact]
        public async Task BatchProcessing_CreateAndProcess_Success()
        {
            // Create multiple signals for batch processing
            var signalIds = new List<string>();
            
            for (int i = 0; i < 3; i++)
            {
                var patientCode = $"BATCH_{i}_{Guid.NewGuid():N[..8]}";
                
                // Create patient
                var createPatientDto = new CreatePatientDto
                {
                    PatientCode = patientCode,
                    Age = 50 + i,
                    Gender = i % 2 == 0 ? "Male" : "Female",
                    ContactInfo = $"{patientCode}@test.com"
                };
                
                await _client.PostAsJsonAsync("/api/patients", createPatientDto);
                
                // Create signal
                var createSignalDto = new CreateEkgSignalDto
                {
                    PatientCode = patientCode,
                    RecordedAt = DateTime.UtcNow.AddMinutes(-i),
                    DataPoints = GenerateTestEkgData(500),
                    SamplingRate = 250,
                    DeviceId = "BATCH_DEVICE",
                    RecordedBy = "Dr. Batch"
                };
                
                var signalResponse = await _client.PostAsJsonAsync("/api/ekgsignals", createSignalDto);
                var signalLocation = signalResponse.Headers.Location?.ToString();
                var signalId = signalLocation?.Split('/').Last();
                if (signalId != null)
                {
                    signalIds.Add(signalId);
                }
            }

            // Create batch job
            var createJobDto = new CreateBatchJobDto
            {
                JobType = "SignalAnalysis",
                Parameters = new Dictionary<string, string> { { "Algorithm", "Standard" } },
                ItemIds = signalIds
            };
            
            var jobResponse = await _client.PostAsJsonAsync("/api/batchjobs", createJobDto);
            jobResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var jobLocation = jobResponse.Headers.Location?.ToString();
            jobLocation.Should().NotBeNull();
            var jobId = jobLocation?.Split('/').Last();

            // Verify job was created
            var getJobResponse = await _client.GetAsync($"/api/batchjobs/{jobId}");
            getJobResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var job = await getJobResponse.Content.ReadFromJsonAsync<BatchJobDto>();
            job.Should().NotBeNull();
            job!.TotalItems.Should().Be(3);
        }

        [Fact]
        public async Task HealthChecks_AllServices_ReturnHealthy()
        {
            var response = await _client.GetAsync("/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Healthy");
        }

        private double[] GenerateTestEkgData(int samples)
        {
            var data = new double[samples];
            var random = new Random();
            
            for (int i = 0; i < samples; i++)
            {
                // Generate simple sine wave with some noise
                data[i] = Math.Sin(2 * Math.PI * i / 50.0) + (random.NextDouble() - 0.5) * 0.1;
            }
            
            return data;
        }
    }

    // Custom Web Application Factory for Integration Tests
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Override configurations for testing
                // Use in-memory databases, mock services, etc.
            });
        }
    }
}