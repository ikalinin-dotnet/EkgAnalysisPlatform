using System.Collections.Generic;

namespace EkgAnalysisPlatform.AnalysisService.Domain.Models
{
    public class AnalysisAlgorithmConfig
    {
        public int Id { get; set; }
        public string AlgorithmName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public string Description { get; set; } = string.Empty;
    }
}