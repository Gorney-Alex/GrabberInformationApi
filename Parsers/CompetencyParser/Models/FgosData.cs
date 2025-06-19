namespace CompetencyParser.Models
{
    /// <summary>
    /// Модель для данных ФГОС ВО
    /// </summary>
    public class FgosData
    {
        /// <summary>
        /// Код направления подготовки (например, 02.04.03, 09.04.01)
        /// </summary>
        public string DirectionCode { get; set; } = string.Empty;

        /// <summary>
        /// Название направления подготовки
        /// </summary>
        public string DirectionName { get; set; } = string.Empty;

        /// <summary>
        /// Код компетенции (например, УК-1, ОПК-2)
        /// </summary>
        public string CompetencyCode { get; set; } = string.Empty;

        /// <summary>
        /// Описание компетенции
        /// </summary>
        public string CompetencyDescription { get; set; } = string.Empty;

        /// <summary>
        /// Индикаторы достижения компетенции
        /// </summary>
        public List<string> Indicators { get; set; } = new List<string>();

        /// <summary>
        /// Тип компетенции (универсальная, общепрофессиональная, профессиональная)
        /// </summary>
        public CompetencyType Type { get; set; }
    }

    /// <summary>
    /// Типы компетенций в ФГОС
    /// </summary>
    public enum CompetencyType
    {
        /// <summary>
        /// Универсальная компетенция (УК)
        /// </summary>
        Universal = 1,

        /// <summary>
        /// Общепрофессиональная компетенция (ОПК)
        /// </summary>
        GeneralProfessional = 2,

        /// <summary>
        /// Профессиональная компетенция (ПК)
        /// </summary>
        Professional = 3
    }
}
