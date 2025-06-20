namespace CompetencyParser.Models
{
    public class FgosData
    {
        public string DirectionCode { get; set; } = string.Empty;
        public string DirectionName { get; set; } = string.Empty;
        public string CompetencyCode { get; set; } = string.Empty;
        public string CompetencyDescription { get; set; } = string.Empty;
        public List<string> Indicators { get; set; } = new List<string>();
        public CompetencyType Type { get; set; }
    }

    public enum CompetencyType
    {
        Universal = 1,
        GeneralProfessional = 2,
        Professional = 3
    }
}
