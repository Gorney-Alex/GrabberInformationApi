namespace CompetencyParser.Models
{
    public class VacancyData
    {
        public string VacancyId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public List<string> ExtractedSkills { get; set; } = new List<string>();
        public VacancySource Source { get; set; }
        public string Url { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public SalaryInfo? Salary { get; set; }
        public string City { get; set; } = string.Empty;
    }
    public enum VacancySource
    {
        HeadHunter = 1,
        SuperJob = 2,
        HabrCareer = 3,
        Other = 99
    }

    public class SalaryInfo
    {
        public decimal? From { get; set; }
        public decimal? To { get; set; }
        public string Currency { get; set; } = "RUR";
        public bool IsGross { get; set; }
    }
}
