namespace CompetencyParser.Models
{
    public class ProfessionalStandardData
    {

        public string StandardCode { get; set; } = string.Empty;
        public string StandardName { get; set; } = string.Empty;
        public string GeneralLaborFunction { get; set; } = string.Empty;
        public string OtfCode { get; set; } = string.Empty;

        public List<string> LaborActions { get; set; } = new List<string>();
        public List<string> RequiredSkills { get; set; } = new List<string>();
        public List<string> RequiredKnowledge { get; set; } = new List<string>();

        public int QualificationLevel { get; set; }
    }
}
