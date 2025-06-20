using CompetencyParser.Models;
using Newtonsoft.Json;
using System.Text;

namespace CompetencyParser.Parsers
{
    public class ProfessionalStandardParser : IParser<ProfessionalStandardData>
    {
        private readonly HttpClient _httpClient;

        public ProfessionalStandardParser()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<List<ProfessionalStandardData>> ParseAsync()
        {
            var result = new List<ProfessionalStandardData>();

            Console.WriteLine("Начинаем парсинг профессиональных стандартов...");

            result.AddRange(await ParseProfStandardMockDataAsync());

            Console.WriteLine($"Парсинг профстандартов завершен. Получено записей: {result.Count}");
            return result;
        }
        
        private async Task<List<ProfessionalStandardData>> ParseProfStandardMockDataAsync()
        {
            await Task.Delay(100);
            
            return new List<ProfessionalStandardData>
            {
                new ProfessionalStandardData
                {
                    StandardCode = "06.001",
                    StandardName = "Программист",
                    GeneralLaborFunction = "Разработка и отладка программного кода",
                    OtfCode = "А",
                    QualificationLevel = 3,
                    LaborActions = new List<string>
                    {
                        "Формализация и алгоритмизация поставленных задач",
                        "Написание программного кода с использованием языков программирования",
                        "Проверка и отладка программного кода",
                        "Интеграция программных модулей и компонент",
                        "Верификация готового программного продукта"
                    },
                    RequiredSkills = new List<string>
                    {
                        "Владение языками программирования (C#, Java, Python, JavaScript)",
                        "Работа с системами управления базами данных (SQL)",
                        "Использование систем контроля версий (Git)",
                        "Применение технологий веб-разработки (HTML, CSS, JavaScript)",
                        "Работа с интегрированными средами разработки"
                    },
                    RequiredKnowledge = new List<string>
                    {
                        "Языки программирования и работа с базами данных",
                        "Основы программной инженерии",
                        "Методологии разработки программного обеспечения",
                        "Основы информационной безопасности"
                    }
                },
                new ProfessionalStandardData
                {
                    StandardCode = "06.004",
                    StandardName = "Системный аналитик",
                    GeneralLaborFunction = "Проведение работ по обследованию и анализу деятельности организации",
                    OtfCode = "В",
                    QualificationLevel = 6,
                    LaborActions = new List<string>
                    {
                        "Анализ архитектуры информационной системы",
                        "Определение возможностей реализации требований к программному обеспечению",
                        "Проведение анализа рисков и определение мер по их минимизации",
                        "Планирование и координация проектных работ",
                        "Управление процессом разработки технических требований"
                    },
                    RequiredSkills = new List<string>
                    {
                        "Анализ и проектирование информационных систем",
                        "Моделирование бизнес-процессов (BPMN, UML)",
                        "Работа с требованиями к программному обеспечению",
                        "Знание архитектурных паттернов и принципов проектирования",
                        "Управление проектами (Agile, Scrum)"
                    },
                    RequiredKnowledge = new List<string>
                    {
                        "Методы анализа предметной области",
                        "Принципы построения информационных систем",
                        "Основы проектного управления",
                        "Методологии разработки программного обеспечения"
                    }
                },
                new ProfessionalStandardData
                {
                    StandardCode = "06.007",
                    StandardName = "Архитектор программного обеспечения",
                    GeneralLaborFunction = "Проектирование архитектуры программных средств",
                    OtfCode = "D",
                    QualificationLevel = 7,
                    LaborActions = new List<string>
                    {
                        "Анализ требований к программной архитектуре",
                        "Проектирование архитектуры программного обеспечения",
                        "Выбор технологий для реализации программного обеспечения",
                        "Документирование архитектуры программного обеспечения",
                        "Контроль соответствия разрабатываемых программных решений архитектуре"
                    },
                    RequiredSkills = new List<string>
                    {
                        "Проектирование архитектуры сложных программных систем",
                        "Знание архитектурных паттернов и принципов (SOLID, DDD)",
                        "Работа с микросервисной архитектурой",
                        "Контейнеризация приложений (Docker, Kubernetes)",
                        "Облачные технологии (AWS, Azure, Google Cloud)"
                    },
                    RequiredKnowledge = new List<string>
                    {
                        "Принципы построения программной архитектуры",
                        "Технологии разработки программного обеспечения",
                        "Методы обеспечения качества программного обеспечения",
                        "Основы информационной безопасности"
                    }
                }
            };        }

        public async Task SaveToFileAsync(List<ProfessionalStandardData> data, string filePath)
        {
            try
            {                var json = JsonConvert.SerializeObject(data, Formatting.Indented);

                await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
                Console.WriteLine($"Данные профстандартов сохранены в файл: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении данных профстандартов: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
