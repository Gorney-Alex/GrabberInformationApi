using CompetencyParser.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CompetencyParser.Parsers
{
    public class VacancyParser : IParser<VacancyData>
    {
        private readonly HttpClient _httpClient;
        private const string HH_API_BASE_URL = "https://api.hh.ru/vacancies";
        
        private readonly List<string> _itSkills = new()
        {
            "C#", "Java", "Python", "JavaScript", "TypeScript", "PHP", "Go", "Rust",
            "SQL", "MySQL", "PostgreSQL", "MongoDB", "Redis",
            "Docker", "Kubernetes", "Jenkins", "GitLab CI",
            "React", "Angular", "Vue", "Node.js", "Express",
            ".NET", "Spring", "Django", "Flask", "Laravel",
            "AWS", "Azure", "Google Cloud", "Terraform",
            "Git", "GitHub", "GitLab", "Bitbucket",
            "Linux", "Windows Server", "Ubuntu",
            "Agile", "Scrum", "DevOps", "CI/CD",
            "REST API", "GraphQL", "Microservices",
            "Machine Learning", "AI", "Data Science",
            "HTML", "CSS", "SASS", "LESS", "Bootstrap"
        };

        public VacancyParser()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "CompetencyParser/1.0 (educational_practice@university.edu)");        }

        public async Task<List<VacancyData>> ParseAsync()
        {
            var result = new List<VacancyData>();

            Console.WriteLine("Начинаем парсинг вакансий с HeadHunter...");

            try
            {
                var searchQueries = new[] { "программист", "разработчик", "системный аналитик", "архитектор ПО" };

                foreach (var query in searchQueries)
                {
                    Console.WriteLine($"Поиск вакансий по запросу: {query}");
                    var vacancies = await SearchVacanciesAsync(query);
                    result.AddRange(vacancies);

                    await Task.Delay(1000);
                }

                Console.WriteLine($"Парсинг вакансий завершен. Получено записей: {result.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при парсинге вакансий: {ex.Message}");
                result.AddRange(await ParseVacancyMockDataAsync());
            }

            return result;
        }

        private async Task<List<VacancyData>> SearchVacanciesAsync(string query, int pages = 2)
        {
            var result = new List<VacancyData>();

            for (int page = 0; page < pages; page++)
            {
                try
                {
                    var url = $"{HH_API_BASE_URL}?text={Uri.EscapeDataString(query)}&area=1&page={page}&per_page=20";
                    var response = await _httpClient.GetStringAsync(url);
                    var data = JObject.Parse(response);

                    var items = data["items"]?.ToObject<JArray>();
                    if (items == null) continue;

                    foreach (var item in items)
                    {
                        var vacancy = await ParseVacancyFromJsonAsync(item);
                        if (vacancy != null)
                        {
                            result.Add(vacancy);
                        }
                    }

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении страницы {page}: {ex.Message}");
                }
            }

            return result;
        }
        
        private async Task<VacancyData?> ParseVacancyFromJsonAsync(JToken vacancyJson)
        {
            try
            {
                var vacancy = new VacancyData
                {
                    VacancyId = vacancyJson["id"]?.ToString() ?? "",
                    Title = vacancyJson["name"]?.ToString() ?? "",
                    Company = vacancyJson["employer"]?["name"]?.ToString() ?? "",
                    Url = vacancyJson["alternate_url"]?.ToString() ?? "",
                    Source = VacancySource.HeadHunter
                };

                if (DateTime.TryParse(vacancyJson["published_at"]?.ToString(), out var publishedDate))
                {
                    vacancy.PublishedDate = publishedDate;
                }

                var salary = vacancyJson["salary"];
                if (salary != null && !salary.HasValues == false)
                {
                    vacancy.Salary = new SalaryInfo
                    {
                        From = salary["from"]?.ToObject<decimal?>(),
                        To = salary["to"]?.ToObject<decimal?>(),
                        Currency = salary["currency"]?.ToString() ?? "RUR",
                        IsGross = salary["gross"]?.ToObject<bool>() ?? false
                    };
                }

                vacancy.City = vacancyJson["area"]?["name"]?.ToString() ?? "";

                if (!string.IsNullOrEmpty(vacancy.VacancyId))
                {
                    await LoadVacancyDetailsAsync(vacancy);
                }

                return vacancy;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при парсинге вакансии: {ex.Message}");
                return null;
            }
        }

        private async Task LoadVacancyDetailsAsync(VacancyData vacancy)
        {
            try
            {
                var url = $"{HH_API_BASE_URL}/{vacancy.VacancyId}";
                var response = await _httpClient.GetStringAsync(url);
                var data = JObject.Parse(response);

                vacancy.Description = data["description"]?.ToString() ?? "";

                var fullText = $"{vacancy.Title} {vacancy.Description}";
                vacancy.ExtractedSkills = ExtractSkillsFromText(fullText);

                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке деталей вакансии {vacancy.VacancyId}: {ex.Message}");
            }
        }

        private List<string> ExtractSkillsFromText(string text)
        {
            var foundSkills = new List<string>();
            var lowerText = text.ToLower();

            foreach (var skill in _itSkills)
            {
                var pattern = $@"\b{Regex.Escape(skill.ToLower())}\b";
                if (Regex.IsMatch(lowerText, pattern, RegexOptions.IgnoreCase))
                {
                    foundSkills.Add(skill);
                }
            }

            return foundSkills.Distinct().ToList();
        }

        private async Task<List<VacancyData>> ParseVacancyMockDataAsync()
        {
            await Task.Delay(100);

            return new List<VacancyData>
            {
                new VacancyData
                {
                    VacancyId = "mock_1",
                    Title = "Senior .NET разработчик",
                    Company = "ТехКомпания",
                    Description = "Требуется опытный разработчик на C# и .NET для работы над крупными проектами",
                    Requirements = "Опыт работы с C#, .NET, SQL Server, Git, знание архитектурных паттернов",
                    ExtractedSkills = new List<string> { "C#", ".NET", "SQL", "Git", "REST API" },
                    Source = VacancySource.HeadHunter,
                    City = "Москва",
                    PublishedDate = DateTime.Now.AddDays(-3),
                    Salary = new SalaryInfo { From = 150000, To = 250000, Currency = "RUR" }
                },
                new VacancyData
                {
                    VacancyId = "mock_2",
                    Title = "Frontend разработчик React",
                    Company = "StartupXYZ",
                    Description = "Ищем Frontend разработчика для работы с современным стеком технологий",
                    Requirements = "Опыт с React, TypeScript, HTML5, CSS3, знание современных подходов к разработке",
                    ExtractedSkills = new List<string> { "React", "TypeScript", "JavaScript", "HTML", "CSS" },
                    Source = VacancySource.HeadHunter,
                    City = "Санкт-Петербург",
                    PublishedDate = DateTime.Now.AddDays(-1),
                    Salary = new SalaryInfo { From = 120000, To = 180000, Currency = "RUR" }
                },
                new VacancyData
                {
                    VacancyId = "mock_3",
                    Title = "DevOps инженер",
                    Company = "CloudTech",
                    Description = "Требуется DevOps инженер для автоматизации процессов разработки",
                    Requirements = "Docker, Kubernetes, Jenkins, AWS, Linux, Python для автоматизации",
                    ExtractedSkills = new List<string> { "Docker", "Kubernetes", "Jenkins", "AWS", "Linux", "Python" },
                    Source = VacancySource.HeadHunter,
                    City = "Новосибирск",
                    PublishedDate = DateTime.Now.AddDays(-2),
                    Salary = new SalaryInfo { From = 180000, To = 280000, Currency = "RUR" }
                }
            };       
        }

        public async Task SaveToFileAsync(List<VacancyData> data, string filePath)
        {
            try
            {   var json = JsonConvert.SerializeObject(data, Formatting.Indented);

                await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
                Console.WriteLine($"Данные вакансий сохранены в файл: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении данных вакансий: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
