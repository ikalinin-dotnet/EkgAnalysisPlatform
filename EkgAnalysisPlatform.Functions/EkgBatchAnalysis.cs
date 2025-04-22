using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EkgAnalysisPlatform.Functions
{
    public class EkgBatchAnalysis
    {
        private readonly HttpClient _httpClient;
        
        public EkgBatchAnalysis()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7000/"); // Update with your API URL
        }
        
        [Function("EkgBatchAnalysis")]
        public async Task Run([TimerTrigger("0 */30 * * * *")] FunctionContext context) // Runs every 30 minutes
        {
            var logger = context.GetLogger("EkgBatchAnalysis");
            logger.LogInformation($"EKG Batch Analysis function executed at: {DateTime.Now}");
            
            try
            {
                // Get all unanalyzed signals
                var response = await _httpClient.GetAsync("api/EkgSignals/unanalyzed");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var signalIds = JsonSerializer.Deserialize<int[]>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (signalIds == null || signalIds.Length == 0)
                {
                    logger.LogInformation("No signals to analyze");
                    return;
                }
                
                logger.LogInformation($"Found {signalIds.Length} signals to analyze");
                
                // Process each signal
                foreach (var id in signalIds)
                {
                    logger.LogInformation($"Analyzing signal ID: {id}");
                    var analysisResponse = await _httpClient.PostAsync($"api/EkgSignals/{id}/analyze", null);
                    analysisResponse.EnsureSuccessStatusCode();
                }
                
                logger.LogInformation("Batch analysis completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in batch analysis: {ex.Message}");
            }
        }
    }
}