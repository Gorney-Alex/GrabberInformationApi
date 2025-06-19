namespace CompetencyParser.Models
{
    /// <summary>
    /// Модель компетенции для анализа ИТ-специалистов
    /// </summary>
    public class Competency
    {
        /// <summary>
        /// Уникальный идентификатор компетенции
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название компетенции (например, "SQL", "Python", "Docker")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание компетенции
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Источник компетенции (ФГОС, Профстандарт, Вакансия)
        /// </summary>
        public CompetencySource Source { get; set; }

        /// <summary>
        /// Категория компетенции (техническая, soft skills и т.д.)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Частота упоминания в источнике (в процентах)
        /// </summary>
        public double FrequencyPercent { get; set; }

        /// <summary>
        /// Количество упоминаний
        /// </summary>
        public int MentionCount { get; set; }

        /// <summary>
        /// Дата парсинга
        /// </summary>
        public DateTime ParsedDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Источники компетенций
    /// </summary>
    public enum CompetencySource
    {
        /// <summary>
        /// Федеральный государственный образовательный стандарт
        /// </summary>
        FGOS = 1,

        /// <summary>
        /// Профессиональный стандарт
        /// </summary>
        ProfessionalStandard = 2,

        /// <summary>
        /// Вакансия с сайтов поиска работы
        /// </summary>
        Vacancy = 3
    }
}
