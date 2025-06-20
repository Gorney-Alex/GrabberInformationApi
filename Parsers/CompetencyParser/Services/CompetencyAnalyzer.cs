using CompetencyParser.Models;
using CsvHelper;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace CompetencyParser.Services
{
    public class CompetencyAnalyzer
    {
        public async Task<List<Competency>> AnalyzeCompetenciesAsync(
            List<FgosData> fgosData,
            List<ProfessionalStandardData> profStandardData,
            List<VacancyData> vacancyData)
        {
            var competencies = new List<Competency>();

            Console.WriteLine("Начинаем анализ компетенций...");

            await Task.Delay(50);

            competencies.AddRange(ProcessFgosData(fgosData));
            competencies.AddRange(ProcessProfStandards(profStandardData));
            competencies.AddRange(ProcessVacancies(vacancyData));

            var finalCompetencies = CountCompetencies(competencies);

            Console.WriteLine($"Анализ завершен. Получено уникальных компетенций: {finalCompetencies.Count}");

            return finalCompetencies;
        }

        private List<Competency> ProcessFgosData(List<FgosData> fgosData)
        {
            var competencies = new List<Competency>();
            int id = 1;

            foreach (var fgos in fgosData)
            {
                string[] keywords = GetKeywords(fgos.CompetencyDescription);

                foreach (string keyword in keywords)
                {
                    competencies.Add(new Competency
                    {
                        Id = id++,
                        Name = keyword,
                        Description = fgos.CompetencyDescription,
                        Source = CompetencySource.FGOS,
                        Category = GetCategory(keyword),
                        MentionCount = 1
                    });
                }
            }

            return competencies;
        }

        private List<Competency> ProcessProfStandards(List<ProfessionalStandardData> profStandardData)
        {
            var competencies = new List<Competency>();
            int id = 1000;

            foreach (var profStandard in profStandardData)
            {
                // Обрабатываем навыки
                foreach (string skill in profStandard.RequiredSkills)
                {
                    string[] keywords = GetKeywords(skill);
                    foreach (string keyword in keywords)
                    {
                        competencies.Add(new Competency
                        {
                            Id = id++,
                            Name = keyword,
                            Description = skill,
                            Source = CompetencySource.ProfessionalStandard,
                            Category = GetCategory(keyword),
                            MentionCount = 1
                        });
                    }
                }

                foreach (string knowledge in profStandard.RequiredKnowledge)
                {
                    string[] keywords = GetKeywords(knowledge);
                    foreach (string keyword in keywords)
                    {
                        competencies.Add(new Competency
                        {
                            Id = id++,
                            Name = keyword,
                            Description = knowledge,
                            Source = CompetencySource.ProfessionalStandard,
                            Category = GetCategory(keyword),
                            MentionCount = 1
                        });
                    }
                }
            }

            return competencies;
        }

        private List<Competency> ProcessVacancies(List<VacancyData> vacancyData)
        {
            var competencies = new List<Competency>();
            int id = 2000;

            foreach (var vacancy in vacancyData)
            {
                foreach (string skill in vacancy.ExtractedSkills)
                {
                    competencies.Add(new Competency
                    {
                        Id = id++,
                        Name = skill,
                        Description = $"Навык из вакансии: {vacancy.Title}",
                        Source = CompetencySource.Vacancy,
                        Category = GetCategory(skill),
                        MentionCount = 1
                    });
                }
            }

            return competencies;
        }
        private List<Competency> CountCompetencies(List<Competency> competencies)
        {
            var grouped = new List<Competency>();
            var competencyGroups = new Dictionary<string, List<Competency>>();
            
            foreach (var comp in competencies)
            {
                string key = comp.Name.ToLower() + "_" + comp.Source.ToString();
                if (!competencyGroups.ContainsKey(key))
                {
                    competencyGroups[key] = new List<Competency>();
                }
                competencyGroups[key].Add(comp);
            }
            
            foreach (var group in competencyGroups)
            {
                var firstComp = group.Value.First();

                var newComp = new Competency
                {
                    Id = firstComp.Id,
                    Name = firstComp.Name,
                    Description = firstComp.Description,
                    Source = firstComp.Source,
                    Category = firstComp.Category,
                    MentionCount = group.Value.Count,
                    FrequencyPercent = 0
                };

                grouped.Add(newComp);
                
            }
            
            var sourceStats = new Dictionary<CompetencySource, int>();
            foreach (var comp in grouped)
            {
                if (!sourceStats.ContainsKey(comp.Source))
                    sourceStats[comp.Source] = 0;
                sourceStats[comp.Source] += comp.MentionCount;
            }
            
            foreach (var comp in grouped)
            {
                if (sourceStats[comp.Source] > 0)
                {
                    comp.FrequencyPercent = Math.Round(
                        (double)comp.MentionCount / sourceStats[comp.Source] * 100, 2);
                }
            }

            return grouped.OrderByDescending(c => c.MentionCount).ToList();
        }

        private string[] GetKeywords(string text)
        {
            string[] itTerms = {
                "программирование", "разработка", "тестирование", "анализ", "проектирование",
                "базы данных", "SQL", "алгоритм", "архитектура", "безопасность",
                "сеть", "система", "веб", "мобильная разработка", "DevOps",
                "машинное обучение", "искусственный интеллект", "большие данные",
                "облачные технологии", "микросервисы", "API", "фреймворк"
            };

            var foundKeywords = new List<string>();
            string lowerText = text.ToLower();
            
            foreach (string term in itTerms)
            {
                if (lowerText.Contains(term.ToLower()))
                {
                    foundKeywords.Add(term);
                }
            }

            return foundKeywords.ToArray();
        }

        private string GetCategory(string competencyName)
        {
            string name = competencyName.ToLower();
            
            if (name.Contains("sql") || name.Contains("программирование") || 
                name.Contains("базы данных") || name.Contains("алгоритм") || 
                name.Contains("архитектура") || name.Contains("api"))
                return "Техническая";
            
            if (name.Contains("анализ") || name.Contains("проектирование") || 
                name.Contains("тестирование"))
                return "Аналитическая";
            
            return "Общая";
        }

        public async Task SaveAnalysisResultsToCsvAsync(List<Competency> competencies, string filePath)
        {
            try
            {
                var lines = new List<string>();
                
                lines.Add("Компетенция,Источник,Описание,Частота в источнике (%),Количество упоминаний,Категория");
                
                foreach (var comp in competencies)
                {
                    string sourceName = comp.Source.ToString();
                    if (comp.Source == CompetencySource.FGOS) sourceName = "ФГОС";
                    if (comp.Source == CompetencySource.ProfessionalStandard) sourceName = "Профстандарт";
                    if (comp.Source == CompetencySource.Vacancy) sourceName = "Вакансия";
                    
                    string line = $"{comp.Name},{sourceName},{comp.Description},{comp.FrequencyPercent},{comp.MentionCount},{comp.Category}";
                    lines.Add(line);
                }
                
                await File.WriteAllLinesAsync(filePath, lines, Encoding.UTF8);
                Console.WriteLine($"Результаты анализа сохранены в CSV: {filePath}");
            }
            catch (Exception ex)
            {                Console.WriteLine($"Ошибка при сохранении в CSV: {ex.Message}");
            }
        }

        public async Task GenerateAnalysisReportAsync(List<Competency> competencies, string reportPath)
        {
            var lines = new List<string>();
            
            lines.Add("ОТЧЕТ ПО АНАЛИЗУ КОМПЕТЕНЦИЙ ИТ-СПЕЦИАЛИСТОВ");
            lines.Add("==================================================");
            lines.Add($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}");
            lines.Add($"Общее количество компетенций: {competencies.Count}");
            lines.Add("");

            var vacancyCount = competencies.Where(c => c.Source == CompetencySource.Vacancy).Count();
            var profCount = competencies.Where(c => c.Source == CompetencySource.ProfessionalStandard).Count();
            var fgosCount = competencies.Where(c => c.Source == CompetencySource.FGOS).Count();

            lines.Add("СТАТИСТИКА ПО ИСТОЧНИКАМ:");
            lines.Add("------------------------------");
            lines.Add($"Вакансия: {vacancyCount} компетенций");
            lines.Add($"Профстандарт: {profCount} компетенций");
            lines.Add($"ФГОС: {fgosCount} компетенций");
            lines.Add("");

            var top10 = competencies.OrderByDescending(c => c.MentionCount).Take(10).ToList();
            lines.Add("ТОП-10 САМЫХ УПОМИНАЕМЫХ КОМПЕТЕНЦИЙ:");
            lines.Add("----------------------------------------");
            for (int i = 0; i < top10.Count; i++)
            {
                var comp = top10[i];
                lines.Add($"{i + 1}. {comp.Name} - {comp.MentionCount} упоминаний ({comp.FrequencyPercent:F2}%)");
            }
            lines.Add("");

            var techCount = competencies.Where(c => c.Category == "Техническая").Count();
            var analCount = competencies.Where(c => c.Category == "Аналитическая").Count();
            var genCount = competencies.Where(c => c.Category == "Общая").Count();

            lines.Add("СТАТИСТИКА ПО КАТЕГОРИЯМ:");
            lines.Add("------------------------------");
            lines.Add($"Техническая: {techCount} компетенций");
            lines.Add($"Общая: {genCount} компетенций");
            lines.Add($"Аналитическая: {analCount} компетенций");
            await File.WriteAllLinesAsync(reportPath, lines, Encoding.UTF8);
            Console.WriteLine($"Отчет сохранен: {reportPath}");
        }
    }
}
