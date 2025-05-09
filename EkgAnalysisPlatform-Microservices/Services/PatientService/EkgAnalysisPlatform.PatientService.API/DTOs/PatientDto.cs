namespace EkgAnalysisPlatform.PatientService.API.DTOs
{
    public class PatientDto
    {
        public int Id { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
    }
    
    public class CreatePatientDto
    {
        public string PatientCode { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
    }
    
    public class UpdatePatientDto
    {
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
    }
}