using CompetencyParser.Models;
using CompetencyParser.Parsers;
using CompetencyParser.Services;
using System.Text;

namespace CompetencyParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== СИСТЕМА АНАЛИЗА КОМПЕТЕНЦИЙ ИТ-СПЕЦИАЛИСТОВ ===");
            Console.WriteLine();

            try
            {
                // Путь к файлу технического задания
                var tzFilePath = @"c:\Users\Home\Desktop\UniversityPractice\GrabberInformationApi\TZ.txt";
                
                // Создание папки для результатов
                var outputDir = @"c:\Users\Home\Desktop\UniversityPractice\GrabberInformationApi\Parsers\CompetencyParser\Results";
                Directory.CreateDirectory(outputDir);

                // 1. Чтение и анализ технического задания
                Console.WriteLine("1. АНАЛИЗ ТЕХНИЧЕСКОГО ЗАДАНИЯ");
                Console.WriteLine("-".PadRight(40, '-'));
                
                var tzReader = new TzReader();
                var tzContent = await tzReader.ReadTzFileAsync(tzFilePath);
                var requirements = tzReader.ExtractRequirementsFromTz(tzContent);
                
                // Сохранение краткого анализа ТЗ
                await tzReader.GenerateTzSummaryAsync(tzContent, Path.Combine(outputDir, "tz_analysis.txt"));
                Console.WriteLine();

                // 2. Парсинг данных ФГОС
                Console.WriteLine("2. ПАРСИНГ ДАННЫХ ФГОС");
                Console.WriteLine("-".PadRight(40, '-'));
                
                var fgosParser = new FgosParser();
                var fgosData = await fgosParser.ParseAsync();
                await fgosParser.SaveToFileAsync(fgosData, Path.Combine(outputDir, "fgos_data.json"));
                Console.WriteLine();

                // 3. Парсинг профессиональных стандартов
                Console.WriteLine("3. ПАРСИНГ ПРОФЕССИОНАЛЬНЫХ СТАНДАРТОВ");
                Console.WriteLine("-".PadRight(40, '-'));
                
                var profStandardParser = new ProfessionalStandardParser();
                var profStandardData = await profStandardParser.ParseAsync();
                await profStandardParser.SaveToFileAsync(profStandardData, Path.Combine(outputDir, "prof_standards_data.json"));
                Console.WriteLine();

                // 4. Парсинг вакансий
                Console.WriteLine("4. ПАРСИНГ ВАКАНСИЙ");
                Console.WriteLine("-".PadRight(40, '-'));
                
                var vacancyParser = new VacancyParser();
                var vacancyData = await vacancyParser.ParseAsync();
                await vacancyParser.SaveToFileAsync(vacancyData, Path.Combine(outputDir, "vacancy_data.json"));
                Console.WriteLine();

                // 5. Анализ и агрегация данных
                Console.WriteLine("5. АНАЛИЗ И АГРЕГАЦИЯ КОМПЕТЕНЦИЙ");
                Console.WriteLine("-".PadRight(40, '-'));
                
                var analyzer = new CompetencyAnalyzer();
                var competencies = await analyzer.AnalyzeCompetenciesAsync(fgosData, profStandardData, vacancyData);
                  // Сохранение результатов
                await analyzer.SaveAnalysisResultsToCsvAsync(competencies, Path.Combine(outputDir, "competencies_analysis.csv"));
                await analyzer.GenerateAnalysisReportAsync(competencies, Path.Combine(outputDir, "analysis_report.txt"));
                Console.WriteLine();

                // 6. СОЗДАНИЕ РАЗЛИЧНЫХ ПРЕДСТАВЛЕНИЙ РЕЗУЛЬТАТОВ
                Console.WriteLine("6. СПОСОБЫ ПРЕДСТАВЛЕНИЯ РЕЗУЛЬТАТОВ");
                Console.WriteLine("-".PadRight(40, '-'));
                
                var visualizationService = new ResultsVisualizationService();
                
                // А) Табличное представление
                Console.WriteLine("А) Создание табличного представления...");
                await visualizationService.CreateTableRepresentationAsync(competencies, 
                    Path.Combine(outputDir, "table_representation.txt"));
                
                // Б) Фильтрация по источникам
                Console.WriteLine("Б) Настройка фильтрации по источникам...");
                await visualizationService.CreateSourceFilteringAsync(competencies, outputDir);
                
                // В) HeatMap для визуализации частотности
                Console.WriteLine("В) Создание HeatMap данных...");
                await visualizationService.CreateHeatMapDataAsync(competencies, 
                    Path.Combine(outputDir, "heatmap_data.txt"));
                
                // Г) Данные для интерактивной визуализации
                Console.WriteLine("Г) Подготовка данных для интерактивной визуализации...");
                await visualizationService.CreateInteractiveVisualizationDataAsync(competencies, outputDir);
                
                // Д) Текстовый анализ диспропорций
                Console.WriteLine("Д) Анализ диспропорций между источниками...");
                await visualizationService.CreateDisproportionAnalysisAsync(competencies, 
                    Path.Combine(outputDir, "disproportion_analysis.txt"));
                  // Е) Матрица сравнения
                Console.WriteLine("Е) Создание матрицы сравнения...");
                await visualizationService.CreateComparisonMatrixAsync(competencies, 
                    Path.Combine(outputDir, "comparison_matrix.txt"));
                
                // Ж) Интерактивный HTML-отчет
                Console.WriteLine("Ж) Создание интерактивного HTML-отчета...");
                var htmlReportService = new HtmlReportService();
                await htmlReportService.CreateInteractiveHtmlReportAsync(competencies,
                    Path.Combine(outputDir, "interactive_report.html"));
                
                Console.WriteLine();

                // 7. ИТОГОВАЯ СТАТИСТИКА
                Console.WriteLine("7. ИТОГОВАЯ СТАТИСТИКА");
                Console.WriteLine("-".PadRight(40, '-'));
                
                DisplayStatistics(fgosData, profStandardData, vacancyData, competencies);
                
                Console.WriteLine();
                Console.WriteLine($"Все результаты сохранены в папке: {outputDir}");
                Console.WriteLine("Парсинг и анализ завершен успешно!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine($"Стек вызовов: {ex.StackTrace}");
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        /// <summary>
        /// Отображение итоговой статистики
        /// </summary>
        private static void DisplayStatistics(
            List<FgosData> fgosData,
            List<ProfessionalStandardData> profStandardData,
            List<VacancyData> vacancyData,
            List<Competency> competencies)
        {
            Console.WriteLine($"Данные ФГОС: {fgosData.Count} записей");
            Console.WriteLine($"Профессиональные стандарты: {profStandardData.Count} записей");
            Console.WriteLine($"Вакансии: {vacancyData.Count} записей");
            Console.WriteLine($"Уникальных компетенций: {competencies.Count}");
            Console.WriteLine();

            // Топ-5 самых упоминаемых компетенций
            var top5 = competencies.OrderByDescending(c => c.MentionCount).Take(5);
            Console.WriteLine("ТОП-5 самых упоминаемых компетенций:");
            int rank = 1;
            foreach (var comp in top5)
            {
                Console.WriteLine($"{rank}. {comp.Name} - {comp.MentionCount} упоминаний ({GetSourceName(comp.Source)})");
                rank++;
            }
            Console.WriteLine();

            // Статистика по источникам
            var sourceStats = competencies.GroupBy(c => c.Source)
                .Select(g => new { Source = g.Key, Count = g.Count() });

            Console.WriteLine("Распределение по источникам:");
            foreach (var stat in sourceStats)
            {
                Console.WriteLine($"- {GetSourceName(stat.Source)}: {stat.Count} компетенций");
            }
        }

        /// <summary>
        /// Получение читаемого названия источника
        /// </summary>
        private static string GetSourceName(CompetencySource source)
        {
            return source switch
            {
                CompetencySource.FGOS => "ФГОС",
                CompetencySource.ProfessionalStandard => "Профстандарт", 
                CompetencySource.Vacancy => "Вакансия",
                _ => "Неизвестно"
            };
        }
    }
}
