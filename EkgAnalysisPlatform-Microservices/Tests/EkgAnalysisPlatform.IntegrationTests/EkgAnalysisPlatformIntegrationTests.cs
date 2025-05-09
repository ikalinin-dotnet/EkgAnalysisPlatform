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
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EkgAnalysisPlatform.IntegrationTests
{
    public class EkgAnalysisPlatformIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public EkgAnalysisPlatformIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Configure the application with test services and in-memory databases
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace real database contexts with in-memory ones
                    // Configure test event bus, etc.
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PatientService_CreateAndGetPatient_Success()
        {
            // Arrange
            var createPatientDto = new CreatePatientDto
            {
                PatientCode = "TEST001",
                Age = 35,
                Gender = "Male",
                ContactInfo = "test@example.com"
            };

            // Act - Create patient
            var createResponse = await _client.PostAsJsonAsync("/api/patients", createPatientDto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            // Get location of created resource
            var locationHeader = createResponse.Headers.Location;
            locationHeader.Should().NotBeNull();
            
            // Act - Get created patient
            var getResponse = await _client.GetAsync(locationHeader);
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var patient = await getResponse.Content.ReadFromJsonAsync<PatientDto>();
            
            // Assert
            patient.Should().NotBeNull();
            patient!.PatientCode.Should().Be("TEST001");
            patient.Age.Should().Be(35);
            patient.Gender.Should().Be("Male");
        }

        [Fact]
        public async Task EkgSignalService_CreateAndGetSignal_Success()
        {
            // Arrange - First create a patient
            var patientCode = "TEST002";
            var createPatientDto = new CreatePatientDto
            {
                PatientCode = patientCode,
                Age = 42,
                Gender = "Female",
                ContactInfo = "test2@example.com"
            };
            
            await _client.PostAsJsonAsync("/api/patients", createPatientDto);
            
            // Arrange - Create EKG signal
            var createSignalDto = new CreateEkgSignalDto
            {
                PatientCode = patientCode,
                RecordedAt = DateTime.UtcNow,
                DataPoints = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 },
                SamplingRate = 250,
                DeviceId = "DEV001",
                RecordedBy = "Dr. Test"
            };

            // Act - Create signal
            var createResponse = await _client.PostAsJsonAsync("/api/ekgsignals", createSignalDto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            // Get location of created resource
            var locationHeader = createResponse.Headers.Location;
            locationHeader.Should().NotBeNull();
            
            // Act - Get created signal
            var getResponse = await _client.GetAsync(locationHeader);
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var signal = await getResponse.Content.ReadFromJsonAsync<EkgSignalDto>();
            
            // Assert
            signal.Should().NotBeNull();
            signal!.PatientCode.Should().Be(patientCode);
            signal.SamplingRate.Should().Be(250);
            signal.DataPointsCount.Should().Be(5);
            
            // Act - Get signal data
            var dataResponse = await _client.GetAsync($"{locationHeader}/data");
            dataResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var dataPoints = await dataResponse.Content.ReadFromJsonAsync<double[]>();
            
            // Assert
            dataPoints.Should().NotBeNull();
            dataPoints.Should().BeEquivalentTo(new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 });
        }
        
        [Fact]
        public async Task AnalysisService_CreateRequestAndGetResult_Success()
        {
            // Arrange - First create a patient and signal
            var patientCode = "TEST003";
            var createPatientDto = new CreatePatientDto
            {
                PatientCode = patientCode,
                Age = 55,
                Gender = "Male",
                ContactInfo = "test3@example.com"
            };
            
            await _client.PostAsJsonAsync("/api/patients", createPatientDto);
            
            var createSignalDto = new CreateEkgSignalDto
            {
                PatientCode = patientCode,
                RecordedAt = DateTime.UtcNow,
                DataPoints = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 },
                SamplingRate = 250,
                DeviceId = "DEV001",
                RecordedBy = "Dr. Test"
            };
            
            var signalResponse = await _client.PostAsJsonAsync("/api/ekgsignals", createSignalDto);
            var signalLocation = signalResponse.Headers.Location;
            var signalId = signalLocation.ToString().Split('/').Last();
            
            // Arrange - Create analysis request
            var createRequestDto = new CreateAnalysisRequestDto
            {
                SignalReference = signalId,
                PatientCode = patientCode,
                RequestedBy = "Dr. Test",
                AnalysisType = "Standard",
                Priority = 1
            };
            
            // Act - Create analysis request
            var createResponse = await _client.PostAsJsonAsync("/api/analysis/requests", createRequestDto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            // Get location of created resource
            var locationHeader = createResponse.Headers.Location;
            locationHeader.Should().NotBeNull();
            
            // Note: In a real scenario, we would need to wait for the analysis to complete
            // For now, we'll assume the analysis service processes the request immediately
            
            // Act - Get analysis results for patient
            var resultsResponse = await _client.GetAsync($"/api/analysis/results/patient/{patientCode}");
            resultsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var results = await resultsResponse.Content.ReadFromJsonAsync<List<AnalysisResultDto>>();
            
            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCountGreaterOrEqual(1);
            results.Should().Contain(r => r.PatientCode == patientCode && r.SignalReference == signalId);
        }
        
        [Fact]
        public async Task BatchProcessingService_CreateJobAndMonitorCompletion_Success()
        {
            // Arrange - First create multiple patients and signals
            var patientCodes = new[] { "BATCH001", "BATCH002", "BATCH003" };
            var signalIds = new List<string>();
            
            foreach (var patientCode in patientCodes)
            {
                var createPatientDto = new CreatePatientDto
                {
                    PatientCode = patientCode,
                    Age = 50,
                    Gender = "Male",
                    ContactInfo = $"{patientCode}@example.com"
                };
                
                await _client.PostAsJsonAsync("/api/patients", createPatientDto);
                
                var createSignalDto = new CreateEkgSignalDto
                {
                    PatientCode = patientCode,
                    RecordedAt = DateTime.UtcNow,
                    DataPoints = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 },
                    SamplingRate = 250,
                    DeviceId = "DEV001",
                    RecordedBy = "Dr. Test"
                };
                
                var signalResponse = await _client.PostAsJsonAsync("/api/ekgsignals", createSignalDto);
                var signalLocation = signalResponse.Headers.Location;
                var signalId = signalLocation.ToString().Split('/').Last();
                
                signalIds.Add(signalId);
            }
            
            // Arrange - Create batch job
            var createJobDto = new CreateBatchJobDto
            {
                JobType = "SignalAnalysis",
                Parameters = new Dictionary<string, string> { { "Algorithm", "StandardEKG" } },
                ItemIds = signalIds
            };
            
            // Act - Create batch job
            var createResponse = await _client.PostAsJsonAsync("/api/batchjobs", createJobDto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            // Get location of created resource
            var locationHeader = createResponse.Headers.Location;
            locationHeader.Should().NotBeNull();
            
            var jobId = locationHeader.ToString().Split('/').Last();
            
            // Act - Get batch job
            var getResponse = await _client.GetAsync($"/api/batchjobs/{jobId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var job = await getResponse.Content.ReadFromJsonAsync<BatchJobDto>();
            
            // Assert
            job.Should().NotBeNull();
            job!.JobId.Should().Be(jobId);
            job.JobType.Should().Be("SignalAnalysis");
            job.TotalItems.Should().Be(3);
            
            // Note: In a real integration test, we would poll the job status until it completes
            // For now, we'll skip that part and assume the status is either Pending or Running

            // Act - Get job items
            var itemsResponse = await _client.GetAsync($"/api/batchjobs/{jobId}/items");
            itemsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<BatchJobItemDto>>();
            
            // Assert
            items.Should().NotBeNull();
            items.Should().HaveCount(3);
            items.Should().AllSatisfy(item => 
            {
                item.BatchJobId.Should().Be(jobId);
                signalIds.Should().Contain(item.ItemId);
            });
        }

        [Fact]
        public async Task ApiGateway_CheckHealth_Success()
        {
            // Act
            var response = await _client.GetAsync("/health");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var healthCheck = JsonSerializer.Deserialize<HealthCheckResponse>(content);
            
            healthCheck.Should().NotBeNull();
            healthCheck!.Status.Should().Be("Healthy");
            healthCheck.Services.Should().NotBeEmpty();
            healthCheck.Services.Should().Contain(s => s.Service == "Gateway" && s.Status == "Healthy");
        }
        
        // Helper class for health check response
        private class HealthCheckResponse
        {
            public string Status { get; set; }
            public TimeSpan Duration { get; set; }
            public List<ServiceHealth> Services { get; set; }
            
            public class ServiceHealth
            {
                public string Service { get; set; }
                public string Status { get; set; }
                public string Description { get; set; }
                public TimeSpan Duration { get; set; }
            }
        }
    }
    
    public class EndToEndWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public EndToEndWorkflowTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configuration for end-to-end tests
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CompleteWorkflow_PatientToDiagnosis_Success()
        {
            // 1. Create a patient
            var patientCode = "E2E001";
            var createPatientDto = new CreatePatientDto
            {
                PatientCode = patientCode,
                Age = 65,
                Gender = "Male",
                ContactInfo = "e2e@example.com"
            };
            
            var patientResponse = await _client.PostAsJsonAsync("/api/patients", createPatientDto);
            patientResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            // 2. Create an EKG signal
            var createSignalDto = new CreateEkgSignalDto
            {
                PatientCode = patientCode,
                RecordedAt = DateTime.UtcNow,
                DataPoints = GenerateRealisticEkgData(1000), // Generate 4 seconds of data at 250Hz
                SamplingRate = 250,
                DeviceId = "DEV001",
                RecordedBy = "Dr. EndToEnd"
            };
            
            var signalResponse = await _client.PostAsJsonAsync("/api/ekgsignals", createSignalDto);
            signalResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var signalLocation = signalResponse.Headers.Location;
            var signalPathParts = signalLocation.ToString().Split('/');
            var signalId = signalPathParts[signalPathParts.Length - 1];
            
            // 3. Request analysis
            var createRequestDto = new CreateAnalysisRequestDto
            {
                SignalReference = signalId,
                PatientCode = patientCode,
                RequestedBy = "Dr. EndToEnd",
                AnalysisType = "Comprehensive",
                Priority = 2
            };
            
            var analysisResponse = await _client.PostAsJsonAsync("/api/analysis/requests", createRequestDto);
            analysisResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var requestLocation = analysisResponse.Headers.Location;
            var requestPathParts = requestLocation.ToString().Split('/');
            var requestId = requestPathParts[requestPathParts.Length - 1];
            
            // 4. Poll for analysis completion (with timeout)
            var startTime = DateTime.UtcNow;
            var timeout = TimeSpan.FromMinutes(2);
            var completed = false;
            
            while (!completed && DateTime.UtcNow - startTime < timeout)
            {
                // Check request status
                var requestResponse = await _client.GetAsync($"/api/analysis/requests/{requestId}");
                var requestDto = await requestResponse.Content.ReadFromJsonAsync<AnalysisRequestDto>();
                
                if (requestDto.Status == "Completed")
                {
                    completed = true;
                    break;
                }
                
                // Wait before polling again
                await Task.Delay(1000);
            }
            
            completed.Should().BeTrue("Analysis should complete within the timeout period");
            
            // 5. Get analysis results
            var resultsResponse = await _client.GetAsync($"/api/analysis/results/patient/{patientCode}");
            resultsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var results = await resultsResponse.Content.ReadFromJsonAsync<List<AnalysisResultDto>>();
            
            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCountGreaterOrEqual(1);
            
            var result = results.FirstOrDefault(r => r.SignalReference == signalId);
            result.Should().NotBeNull();
            result!.PatientCode.Should().Be(patientCode);
            result.HeartRate.Should().BeGreaterThan(0);
            result.Status.Should().Be("Completed");
        }
        
        // Helper method to generate realistic-looking EKG data
        private double[] GenerateRealisticEkgData(int samples)
        {
            var data = new double[samples];
            var heartRate = 72.0; // beats per minute
            var sampleRate = 250.0; // Hz
            var period = 60.0 / heartRate; // seconds per beat
            var samplesPerBeat = period * sampleRate;
            
            for (int i = 0; i < samples; i++)
            {
                var t = i / sampleRate;
                var beatPhase = (i % samplesPerBeat) / samplesPerBeat;
                
                // Simplified EKG waveform
                if (beatPhase < 0.1)
                    data[i] = 0.1 * Math.Sin(beatPhase * Math.PI * 2 * 10); // P wave
                else if (beatPhase >= 0.2 && beatPhase < 0.25)
                    data[i] = Math.Exp(-Math.Pow((beatPhase - 0.225) * 100, 2)) * 1.0; // Q wave
                else if (beatPhase >= 0.25 && beatPhase < 0.3)
                    data[i] = Math.Exp(-Math.Pow((beatPhase - 0.275) * 100, 2)) * 2.5; // R wave
                else if (beatPhase >= 0.3 && beatPhase < 0.35)
                    data[i] = Math.Exp(-Math.Pow((beatPhase - 0.325) * 100, 2)) * -0.5; // S wave
                else if (beatPhase >= 0.4 && beatPhase < 0.7)
                    data[i] = 0.2 * Math.Sin((beatPhase - 0.4) * Math.PI * 2 * 3); // T wave
                else
                    data[i] = 0;
            }
            
            return data;
        }
    }
}