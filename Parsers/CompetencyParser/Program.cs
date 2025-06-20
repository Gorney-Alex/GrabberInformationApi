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
            Console.WriteLine("Программа анализа компетенций");
            Console.WriteLine("Выполнил: Студент 3 курса");
            Console.WriteLine();            try
            {
  // Папка для сохранения результатов
                string outputDir = @"c:\Users\Home\Desktop\UniversityPractice\GrabberInformationApi\Parsers\CompetencyParser\Results";
                Directory.CreateDirectory(outputDir);

                // Шаг 1: Парсим данные ФГОС
                Console.WriteLine("1. Парсим данные ФГОС...");
                
                FgosParser fgosParser = new FgosParser();
                List<FgosData> fgosData = await fgosParser.ParseAsync();
                await fgosParser.SaveToFileAsync(fgosData, Path.Combine(outputDir, "fgos_data.json"));
                Console.WriteLine("Готово!");                // Шаг 2: Парсим профессиональные стандарты
                Console.WriteLine("2. Парсим профессиональные стандарты...");
                
                ProfessionalStandardParser profStandardParser = new ProfessionalStandardParser();
                List<ProfessionalStandardData> profStandardData = await profStandardParser.ParseAsync();
                await profStandardParser.SaveToFileAsync(profStandardData, Path.Combine(outputDir, "prof_standards_data.json"));
                Console.WriteLine("Готово!");

                // Шаг 3: Парсим вакансии
                Console.WriteLine("3. Парсим вакансии...");
                
                VacancyParser vacancyParser = new VacancyParser();
                List<VacancyData> vacancyData = await vacancyParser.ParseAsync();
                await vacancyParser.SaveToFileAsync(vacancyData, Path.Combine(outputDir, "vacancy_data.json"));
                Console.WriteLine("Готово!");

                // Шаг 4: Анализируем компетенции
                Console.WriteLine("4. Анализируем компетенции...");
                
                CompetencyAnalyzer analyzer = new CompetencyAnalyzer();
                List<Competency> competencies = await analyzer.AnalyzeCompetenciesAsync(fgosData, profStandardData, vacancyData);
                
                // Сохраняем результаты
                await analyzer.SaveAnalysisResultsToCsvAsync(competencies, Path.Combine(outputDir, "competencies_analysis.csv"));
                await analyzer.GenerateAnalysisReportAsync(competencies, Path.Combine(outputDir, "analysis_report.txt"));
                Console.WriteLine("Готово!");

                // Шаг 5: Создаем разные представления результатов
                Console.WriteLine("5. Создаем представления результатов...");
                
                SimpleVisualizationService visualizationService = new SimpleVisualizationService();
                // Создаем таблицу
                Console.WriteLine("- Создаем таблицу");
                await visualizationService.CreateTableRepresentationAsync(competencies, 
                    Path.Combine(outputDir, "table_representation.txt"));
                
                // Создаем фильтры по источникам
                Console.WriteLine("- Создаем фильтры по источникам");
                await visualizationService.CreateSourceFilteringAsync(competencies, outputDir);
                
                // Создаем данные для тепловой карты
                Console.WriteLine("- Создаем данные для тепловой карты");
                await visualizationService.CreateHeatMapDataAsync(competencies, 
                    Path.Combine(outputDir, "heatmap_data.txt"));
                
                // Создаем данные для графиков
                Console.WriteLine("- Создаем данные для графиков");
                await visualizationService.CreateInteractiveVisualizationDataAsync(competencies, outputDir);
                
                // Анализируем различия между источниками
                Console.WriteLine("- Анализируем различия между источниками");
                await visualizationService.CreateDisproportionAnalysisAsync(competencies, 
                    Path.Combine(outputDir, "disproportion_analysis.txt"));
                
                // Создаем матрицу сравнения
                Console.WriteLine("- Создаем матрицу сравнения");
                await visualizationService.CreateComparisonMatrixAsync(competencies, 
                    Path.Combine(outputDir, "comparison_matrix.txt"));
                  // Создаем HTML отчет
                Console.WriteLine("- Создаем HTML отчет");
                SimpleHtmlReportService htmlReportService = new SimpleHtmlReportService();
                await htmlReportService.CreateInteractiveHtmlReportAsync(competencies,
                    Path.Combine(outputDir, "interactive_report.html"));
                
                Console.WriteLine("Готово!");

                // Показываем статистику
                Console.WriteLine();
                Console.WriteLine("ИТОГОВАЯ СТАТИСТИКА:");
                ShowStatistics(fgosData, profStandardData, vacancyData, competencies);                
                Console.WriteLine();
                Console.WriteLine($"Все файлы сохранены в папке: {outputDir}");
                Console.WriteLine("Анализ завершен!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        // Простая функция для показа статистики
        private static void ShowStatistics(
            List<FgosData> fgosData,
            List<ProfessionalStandardData> profStandardData,
            List<VacancyData> vacancyData,
            List<Competency> competencies)
        {
            Console.WriteLine($"Данные ФГОС: {fgosData.Count} записей");
            Console.WriteLine($"Профессиональные стандарты: {profStandardData.Count} записей");
            Console.WriteLine($"Вакансии: {vacancyData.Count} записей");
            Console.WriteLine($"Найдено компетенций: {competencies.Count}");
            Console.WriteLine();

            // Показываем топ-5 компетенций
            var top5 = competencies.OrderByDescending(c => c.MentionCount).Take(5);
            Console.WriteLine("ТОП-5 компетенций:");
            int num = 1;
            foreach (var comp in top5)
            {
                string sourceName = comp.Source.ToString();
                if (comp.Source == CompetencySource.FGOS) sourceName = "ФГОС";
                if (comp.Source == CompetencySource.ProfessionalStandard) sourceName = "Профстандарт";
                if (comp.Source == CompetencySource.Vacancy) sourceName = "Вакансия";
                
                Console.WriteLine($"{num}. {comp.Name} - {comp.MentionCount} раз ({sourceName})");
                num++;
            }
        }
    }
}
