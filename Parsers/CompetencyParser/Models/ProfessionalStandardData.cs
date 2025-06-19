namespace CompetencyParser.Models
{
    /// <summary>
    /// Модель для данных профессиональных стандартов
    /// </summary>
    public class ProfessionalStandardData
    {
        /// <summary>
        /// Код профессионального стандарта
        /// </summary>
        public string StandardCode { get; set; } = string.Empty;

        /// <summary>
        /// Название профессионального стандарта
        /// </summary>
        public string StandardName { get; set; } = string.Empty;

        /// <summary>
        /// Обобщенная трудовая функция (ОТФ)
        /// </summary>
        public string GeneralLaborFunction { get; set; } = string.Empty;

        /// <summary>
        /// Код ОТФ
        /// </summary>
        public string OtfCode { get; set; } = string.Empty;

        /// <summary>
        /// Трудовые действия
        /// </summary>
        public List<string> LaborActions { get; set; } = new List<string>();

        /// <summary>
        /// Необходимые умения
        /// </summary>
        public List<string> RequiredSkills { get; set; } = new List<string>();

        /// <summary>
        /// Необходимые знания
        /// </summary>
        public List<string> RequiredKnowledge { get; set; } = new List<string>();

        /// <summary>
        /// Уровень квалификации
        /// </summary>
        public int QualificationLevel { get; set; }
    }
}
