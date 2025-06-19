namespace CompetencyParser.Models
{
    /// <summary>
    /// Модель для данных вакансий
    /// </summary>
    public class VacancyData
    {
        /// <summary>
        /// Идентификатор вакансии
        /// </summary>
        public string VacancyId { get; set; } = string.Empty;

        /// <summary>
        /// Название вакансии
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Компания
        /// </summary>
        public string Company { get; set; } = string.Empty;

        /// <summary>
        /// Описание вакансии
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Требования к кандидату
        /// </summary>
        public string Requirements { get; set; } = string.Empty;

        /// <summary>
        /// Извлеченные навыки и технологии
        /// </summary>
        public List<string> ExtractedSkills { get; set; } = new List<string>();

        /// <summary>
        /// Источник вакансии (HeadHunter, SuperJob и т.д.)
        /// </summary>
        public VacancySource Source { get; set; }

        /// <summary>
        /// URL вакансии
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Дата публикации вакансии
        /// </summary>
        public DateTime PublishedDate { get; set; }

        /// <summary>
        /// Зарплата (если указана)
        /// </summary>
        public SalaryInfo? Salary { get; set; }

        /// <summary>
        /// Город
        /// </summary>
        public string City { get; set; } = string.Empty;
    }

    /// <summary>
    /// Источники вакансий
    /// </summary>
    public enum VacancySource
    {
        HeadHunter = 1,
        SuperJob = 2,
        HabrCareer = 3,
        Other = 99
    }

    /// <summary>
    /// Информация о зарплате
    /// </summary>
    public class SalaryInfo
    {
        /// <summary>
        /// Минимальная зарплата
        /// </summary>
        public decimal? From { get; set; }

        /// <summary>
        /// Максимальная зарплата
        /// </summary>
        public decimal? To { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; } = "RUR";

        /// <summary>
        /// Является ли зарплата до вычета налогов
        /// </summary>
        public bool IsGross { get; set; }
    }
}
