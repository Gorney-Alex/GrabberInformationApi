using CompetencyParser.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text;

namespace CompetencyParser.Parsers
{
    /// <summary>
    /// Парсер для данных ФГОС ВО
    /// </summary>
    public class FgosParser : IParser<FgosData>
    {
        private readonly HttpClient _httpClient;
        private readonly List<string> _targetDirections = new()
        {
            "02.04.03", // Математическое обеспечение и администрирование информационных систем
            "09.04.01", // Информатика и вычислительная техника
            "09.04.04"  // Программная инженерия
        };

        public FgosParser()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        /// <summary>
        /// Парсинг данных ФГОС
        /// </summary>
        public async Task<List<FgosData>> ParseAsync()
        {
            var result = new List<FgosData>();

            Console.WriteLine("Начинаем парсинг данных ФГОС ВО...");

            // Пока создаем тестовые данные, так как для реального парсинга нужны конкретные URL
            result.AddRange(await ParseFgosMockDataAsync());

            Console.WriteLine($"Парсинг ФГОС завершен. Получено записей: {result.Count}");
            return result;
        }

        /// <summary>
        /// Создание тестовых данных ФГОС (заменить на реальный парсинг)
        /// </summary>
        private async Task<List<FgosData>> ParseFgosMockDataAsync()
        {
            await Task.Delay(100); // Симуляция асинхронной работы
            
            return new List<FgosData>
            {
                new FgosData
                {
                    DirectionCode = "09.04.01",
                    DirectionName = "Информатика и вычислительная техника",
                    CompetencyCode = "ОПК-1",
                    CompetencyDescription = "Способен применять естественнонаучные и общеинженерные знания, методы математического анализа и моделирования в профессиональной деятельности",
                    Type = CompetencyType.GeneralProfessional,
                    Indicators = new List<string>
                    {
                        "Знает основы математики, физики, вычислительной техники и программирования",
                        "Умеет решать стандартные профессиональные задачи с применением естественнонаучных знаний",
                        "Владеет навыками теоретического и экспериментального исследования"
                    }
                },
                new FgosData
                {
                    DirectionCode = "09.04.01",
                    DirectionName = "Информатика и вычислительная техника",
                    CompetencyCode = "ОПК-2",
                    CompetencyDescription = "Способен разрабатывать оригинальные алгоритмы и программные средства для решения профессиональных задач",
                    Type = CompetencyType.GeneralProfessional,
                    Indicators = new List<string>
                    {
                        "Знает современные языки программирования и технологии разработки ПО",
                        "Умеет проектировать и разрабатывать программные системы",
                        "Владеет навыками разработки алгоритмов и программ"
                    }
                },
                new FgosData
                {
                    DirectionCode = "02.04.03",
                    DirectionName = "Математическое обеспечение и администрирование информационных систем",
                    CompetencyCode = "ПК-1",
                    CompetencyDescription = "Способен проводить исследования и разрабатывать математические модели информационных процессов",
                    Type = CompetencyType.Professional,
                    Indicators = new List<string>
                    {
                        "Знает методы математического моделирования информационных процессов",
                        "Умеет применять математические методы для решения прикладных задач",
                        "Владеет навыками разработки и анализа математических моделей"
                    }
                }
            };
        }

        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        public async Task SaveToFileAsync(List<FgosData> data, string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);

                await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
                Console.WriteLine($"Данные ФГОС сохранены в файл: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении данных ФГОС: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
