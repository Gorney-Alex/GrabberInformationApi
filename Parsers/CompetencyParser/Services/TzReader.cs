using System.Text;

namespace CompetencyParser.Services
{
    /// <summary>
    /// Сервис для работы с техническим заданием
    /// </summary>
    public class TzReader
    {
        /// <summary>
        /// Чтение и анализ файла технического задания
        /// </summary>
        public async Task<string> ReadTzFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Файл технического задания не найден: {filePath}");
                }

                Console.WriteLine($"Чтение файла технического задания: {filePath}");
                
                var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                
                Console.WriteLine($"Файл успешно прочитан. Размер: {content.Length} символов");
                
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении файла ТЗ: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Извлечение ключевых требований из ТЗ
        /// </summary>
        public Dictionary<string, List<string>> ExtractRequirementsFromTz(string tzContent)
        {
            var requirements = new Dictionary<string, List<string>>();

            try
            {
                // Извлекаем информацию о направлениях ФГОС
                var fgosDirections = ExtractFgosDirections(tzContent);
                requirements["ФГОС направления"] = fgosDirections;

                // Извлекаем профессиональные стандарты
                var profStandards = ExtractProfessionalStandards(tzContent);
                requirements["Профессиональные стандарты"] = profStandards;

                // Извлекаем источники вакансий
                var vacancySources = ExtractVacancySources(tzContent);
                requirements["Источники вакансий"] = vacancySources;

                // Извлекаем технологии для поиска
                var technologies = ExtractTechnologies(tzContent);
                requirements["Технологии"] = technologies;

                Console.WriteLine("Ключевые требования извлечены из ТЗ:");
                foreach (var req in requirements)
                {
                    Console.WriteLine($"- {req.Key}: {req.Value.Count} элементов");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при извлечении требований: {ex.Message}");
            }

            return requirements;
        }

        /// <summary>
        /// Извлечение направлений ФГОС из ТЗ
        /// </summary>
        private List<string> ExtractFgosDirections(string content)
        {
            var directions = new List<string>();
            
            // Поиск упоминаний кодов направлений
            var patterns = new[] { "02.04.03", "09.04.01", "09.04.04" };
            
            foreach (var pattern in patterns)
            {
                if (content.Contains(pattern))
                {
                    directions.Add(pattern);
                }
            }

            return directions;
        }

        /// <summary>
        /// Извлечение профессиональных стандартов из ТЗ
        /// </summary>
        private List<string> ExtractProfessionalStandards(string content)
        {
            var standards = new List<string>();
            
            if (content.Contains("Программист"))
                standards.Add("Программист");
            if (content.Contains("Системный аналитик"))
                standards.Add("Системный аналитик");
            if (content.Contains("Архитектор ПО"))
                standards.Add("Архитектор ПО");

            return standards;
        }

        /// <summary>
        /// Извлечение источников вакансий из ТЗ
        /// </summary>
        private List<string> ExtractVacancySources(string content)
        {
            var sources = new List<string>();
            
            if (content.Contains("HeadHunter") || content.Contains("HH.ru"))
                sources.Add("HeadHunter");
            if (content.Contains("SuperJob"))
                sources.Add("SuperJob");

            return sources;
        }

        /// <summary>
        /// Извлечение технологий из ТЗ
        /// </summary>
        private List<string> ExtractTechnologies(string content)
        {
            var technologies = new List<string>();
            var techKeywords = new[] 
            { 
                "SQL", "Python", "Docker", "ClickHouse", "PostgreSQL", 
                "JavaScript", "C#", "Java", "Git", "API" 
            };

            foreach (var tech in techKeywords)
            {
                if (content.Contains(tech))
                {
                    technologies.Add(tech);
                }
            }

            return technologies;
        }

        /// <summary>
        /// Создание краткого отчета по ТЗ
        /// </summary>
        public async Task GenerateTzSummaryAsync(string tzContent, string outputPath)
        {
            try
            {
                var requirements = ExtractRequirementsFromTz(tzContent);
                var summary = new StringBuilder();

                summary.AppendLine("КРАТКИЙ АНАЛИЗ ТЕХНИЧЕСКОГО ЗАДАНИЯ");
                summary.AppendLine("=".PadRight(40, '='));
                summary.AppendLine($"Дата анализа: {DateTime.Now:dd.MM.yyyy HH:mm}");
                summary.AppendLine();

                summary.AppendLine("ОСНОВНЫЕ ЗАДАЧИ:");
                summary.AppendLine("1. Парсинг ФГОС ВО (извлечение компетенций и индикаторов)");
                summary.AppendLine("2. Парсинг профессиональных стандартов (ОТФ и трудовые действия)");
                summary.AppendLine("3. Парсинг вакансий (требования к навыкам и технологиям)");
                summary.AppendLine();

                foreach (var req in requirements)
                {
                    summary.AppendLine($"{req.Key.ToUpper()}:");
                    foreach (var item in req.Value)
                    {
                        summary.AppendLine($"- {item}");
                    }
                    summary.AppendLine();
                }

                summary.AppendLine("ОЖИДАЕМЫЕ РЕЗУЛЬТАТЫ:");
                summary.AppendLine("- Таблица компетенций с указанием источника и частотности");
                summary.AppendLine("- CSV файл для дальнейшего анализа");
                summary.AppendLine("- Подготовка данных для загрузки в ClickHouse");

                await File.WriteAllTextAsync(outputPath, summary.ToString(), Encoding.UTF8);
                Console.WriteLine($"Краткий анализ ТЗ сохранен: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании краткого анализа ТЗ: {ex.Message}");
                throw;
            }
        }
    }
}
