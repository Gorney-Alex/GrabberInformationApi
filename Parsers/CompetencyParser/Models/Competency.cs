namespace CompetencyParser.Models
{
    public class Competency
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CompetencySource Source { get; set; }
        public string Category { get; set; } = string.Empty;
        public double FrequencyPercent { get; set; }
        public int MentionCount { get; set; }
        public DateTime ParsedDate { get; set; } = DateTime.Now;
    }
    public enum CompetencySource
    {
        FGOS = 1,
        ProfessionalStandard = 2,
        Vacancy = 3
    }
}
