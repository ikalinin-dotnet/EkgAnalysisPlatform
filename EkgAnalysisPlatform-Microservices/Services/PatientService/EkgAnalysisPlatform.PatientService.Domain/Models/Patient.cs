namespace EkgAnalysisPlatform.PatientService.Domain.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; }
        public string ContactInfo { get; set; } = string.Empty;
    }
}