using CompetencyParser.Models;
using CsvHelper;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace CompetencyParser.Services
{
    /// <summary>
    /// Сервис для анализа и агрегации компетенций
    /// </summary>
    public class CompetencyAnalyzer
    {        /// <summary>
        /// Анализ и агрегация данных из всех источников
        /// </summary>
        public async Task<List<Competency>> AnalyzeCompetenciesAsync(
            List<FgosData> fgosData,
            List<ProfessionalStandardData> profStandardData,
            List<VacancyData> vacancyData)
        {
            var competencies = new List<Competency>();

            Console.WriteLine("Начинаем анализ компетенций...");

            await Task.Delay(50); // Симуляция асинхронной работы

            // Анализ данных ФГОС
            competencies.AddRange(AnalyzeFgosData(fgosData));

            // Анализ профессиональных стандартов
            competencies.AddRange(AnalyzeProfessionalStandards(profStandardData));

            // Анализ вакансий
            competencies.AddRange(AnalyzeVacancies(vacancyData));

            // Агрегация и подсчет частотности
            var aggregatedCompetencies = AggregateCompetencies(competencies);

            Console.WriteLine($"Анализ завершен. Получено уникальных компетенций: {aggregatedCompetencies.Count}");

            return aggregatedCompetencies;
        }

        /// <summary>
        /// Анализ данных ФГОС
        /// </summary>
        private List<Competency> AnalyzeFgosData(List<FgosData> fgosData)
        {
            var competencies = new List<Competency>();
            var id = 1;

            foreach (var fgos in fgosData)
            {
                // Извлекаем ключевые слова из описания компетенций
                var keywords = ExtractKeywordsFromText(fgos.CompetencyDescription);

                foreach (var keyword in keywords)
                {
                    competencies.Add(new Competency
                    {
                        Id = id++,
                        Name = keyword,
                        Description = fgos.CompetencyDescription,
                        Source = CompetencySource.FGOS,
                        Category = GetCompetencyCategory(keyword),
                        MentionCount = 1
                    });
                }
            }

            return competencies;
        }

        /// <summary>
        /// Анализ профессиональных стандартов
        /// </summary>
        private List<Competency> AnalyzeProfessionalStandards(List<ProfessionalStandardData> profStandardData)
        {
            var competencies = new List<Competency>();
            var id = 1000;

            foreach (var profStandard in profStandardData)
            {
                // Анализируем навыки
                foreach (var skill in profStandard.RequiredSkills)
                {
                    var keywords = ExtractKeywordsFromText(skill);
                    foreach (var keyword in keywords)
                    {
                        competencies.Add(new Competency
                        {
                            Id = id++,
                            Name = keyword,
                            Description = skill,
                            Source = CompetencySource.ProfessionalStandard,
                            Category = GetCompetencyCategory(keyword),
                            MentionCount = 1
                        });
                    }
                }

                // Анализируем знания
                foreach (var knowledge in profStandard.RequiredKnowledge)
                {
                    var keywords = ExtractKeywordsFromText(knowledge);
                    foreach (var keyword in keywords)
                    {
                        competencies.Add(new Competency
                        {
                            Id = id++,
                            Name = keyword,
                            Description = knowledge,
                            Source = CompetencySource.ProfessionalStandard,
                            Category = GetCompetencyCategory(keyword),
                            MentionCount = 1
                        });
                    }
                }
            }

            return competencies;
        }

        /// <summary>
        /// Анализ вакансий
        /// </summary>
        private List<Competency> AnalyzeVacancies(List<VacancyData> vacancyData)
        {
            var competencies = new List<Competency>();
            var id = 2000;

            foreach (var vacancy in vacancyData)
            {
                foreach (var skill in vacancy.ExtractedSkills)
                {
                    competencies.Add(new Competency
                    {
                        Id = id++,
                        Name = skill,
                        Description = $"Навык из вакансии: {vacancy.Title}",
                        Source = CompetencySource.Vacancy,
                        Category = GetCompetencyCategory(skill),
                        MentionCount = 1
                    });
                }
            }

            return competencies;
        }

        /// <summary>
        /// Агрегация компетенций и подсчет частотности
        /// </summary>
        private List<Competency> AggregateCompetencies(List<Competency> competencies)
        {            var grouped = competencies
                .GroupBy(c => new { Name = c.Name.ToLower(), c.Source })
                .Select(g => new Competency
                {
                    Id = g.First().Id,
                    Name = g.First().Name,
                    Description = g.First().Description,
                    Source = g.Key.Source,
                    Category = g.First().Category,
                    MentionCount = g.Sum(x => x.MentionCount),
                    FrequencyPercent = 0 // Будет рассчитано позже
                })
                .ToList();

            // Рассчет частотности по источникам
            var totalBySource = grouped.GroupBy(c => c.Source)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.MentionCount));

            foreach (var competency in grouped)
            {
                if (totalBySource.ContainsKey(competency.Source) && totalBySource[competency.Source] > 0)
                {
                    competency.FrequencyPercent = Math.Round(
                        (double)competency.MentionCount / totalBySource[competency.Source] * 100, 2);
                }
            }

            return grouped.OrderByDescending(c => c.MentionCount).ToList();
        }

        /// <summary>
        /// Извлечение ключевых слов из текста
        /// </summary>
        private List<string> ExtractKeywordsFromText(string text)
        {
            var keywords = new List<string>();
            var itTerms = new[]
            {
                "программирование", "разработка", "тестирование", "анализ", "проектирование",
                "базы данных", "SQL", "алгоритм", "архитектура", "безопасность",
                "сеть", "система", "веб", "мобильная разработка", "DevOps",
                "машинное обучение", "искусственный интеллект", "большие данные",
                "облачные технологии", "микросервисы", "API", "фреймворк"
            };

            foreach (var term in itTerms)
            {
                if (text.ToLower().Contains(term.ToLower()))
                {
                    keywords.Add(term);
                }
            }

            return keywords.Distinct().ToList();
        }

        /// <summary>
        /// Определение категории компетенции
        /// </summary>
        private string GetCompetencyCategory(string competencyName)
        {
            var technical = new[] { "SQL", "программирование", "базы данных", "алгоритм", "архитектура", "API", "фреймворк" };
            var analytical = new[] { "анализ", "проектирование", "тестирование" };
            var management = new[] { "управление", "планирование", "координация" };

            if (technical.Any(t => competencyName.ToLower().Contains(t.ToLower())))
                return "Техническая";
            if (analytical.Any(a => competencyName.ToLower().Contains(a.ToLower())))
                return "Аналитическая";
            if (management.Any(m => competencyName.ToLower().Contains(m.ToLower())))
                return "Управленческая";

            return "Общая";
        }

        /// <summary>
        /// Сохранение результатов анализа в CSV
        /// </summary>
        public async Task SaveAnalysisResultsToCsvAsync(List<Competency> competencies, string filePath)
        {
            try
            {
                await using var writer = new StringWriter();
                await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                csv.WriteField("Компетенция");
                csv.WriteField("Источник");
                csv.WriteField("Описание");
                csv.WriteField("Частота в источнике (%)");
                csv.WriteField("Количество упоминаний");
                csv.WriteField("Категория");
                csv.NextRecord();

                foreach (var competency in competencies)
                {
                    csv.WriteField(competency.Name);
                    csv.WriteField(GetSourceName(competency.Source));
                    csv.WriteField(competency.Description);
                    csv.WriteField(competency.FrequencyPercent);
                    csv.WriteField(competency.MentionCount);
                    csv.WriteField(competency.Category);
                    csv.NextRecord();
                }

                await File.WriteAllTextAsync(filePath, writer.ToString(), Encoding.UTF8);
                Console.WriteLine($"Результаты анализа сохранены в CSV: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении в CSV: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получение читаемого названия источника
        /// </summary>
        private string GetSourceName(CompetencySource source)
        {
            return source switch
            {
                CompetencySource.FGOS => "ФГОС",
                CompetencySource.ProfessionalStandard => "Профстандарт",
                CompetencySource.Vacancy => "Вакансия",
                _ => "Неизвестно"
            };
        }

        /// <summary>
        /// Создание отчета по анализу
        /// </summary>
        public async Task GenerateAnalysisReportAsync(List<Competency> competencies, string reportPath)
        {
            var report = new StringBuilder();
            
            report.AppendLine("ОТЧЕТ ПО АНАЛИЗУ КОМПЕТЕНЦИЙ ИТ-СПЕЦИАЛИСТОВ");
            report.AppendLine("=".PadRight(50, '='));
            report.AppendLine($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}");
            report.AppendLine($"Общее количество компетенций: {competencies.Count}");
            report.AppendLine();

            // Статистика по источникам
            var sourceStats = competencies.GroupBy(c => c.Source)
                .Select(g => new { Source = g.Key, Count = g.Count() });

            report.AppendLine("СТАТИСТИКА ПО ИСТОЧНИКАМ:");
            report.AppendLine("-".PadRight(30, '-'));
            foreach (var stat in sourceStats)
            {
                report.AppendLine($"{GetSourceName(stat.Source)}: {stat.Count} компетенций");
            }
            report.AppendLine();

            // Топ-10 самых упоминаемых компетенций
            var topCompetencies = competencies.OrderByDescending(c => c.MentionCount).Take(10);
            report.AppendLine("ТОП-10 САМЫХ УПОМИНАЕМЫХ КОМПЕТЕНЦИЙ:");
            report.AppendLine("-".PadRight(40, '-'));
            int rank = 1;
            foreach (var comp in topCompetencies)
            {
                report.AppendLine($"{rank}. {comp.Name} - {comp.MentionCount} упоминаний ({comp.FrequencyPercent}%)");
                rank++;
            }
            report.AppendLine();

            // Статистика по категориям
            var categoryStats = competencies.GroupBy(c => c.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() });

            report.AppendLine("СТАТИСТИКА ПО КАТЕГОРИЯМ:");
            report.AppendLine("-".PadRight(30, '-'));
            foreach (var stat in categoryStats)
            {
                report.AppendLine($"{stat.Category}: {stat.Count} компетенций");
            }

            await File.WriteAllTextAsync(reportPath, report.ToString(), Encoding.UTF8);
            Console.WriteLine($"Отчет сохранен: {reportPath}");
        }
    }
}
