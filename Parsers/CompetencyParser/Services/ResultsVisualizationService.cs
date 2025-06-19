using CompetencyParser.Models;
using System.Text;

namespace CompetencyParser.Services
{
    /// <summary>
    /// Сервис для создания различных представлений результатов анализа
    /// </summary>
    public class ResultsVisualizationService
    {
        /// <summary>
        /// А) Создание табличного представления результатов
        /// </summary>
        public async Task CreateTableRepresentationAsync(List<Competency> competencies, string outputPath)
        {
            var report = new StringBuilder();
            
            report.AppendLine("ТАБЛИЧНОЕ ПРЕДСТАВЛЕНИЕ РЕЗУЛЬТАТОВ СВЕДЕНИЯ ДАННЫХ");
            report.AppendLine("=".PadRight(60, '='));
            report.AppendLine();

            // Группировка по компетенциям для лучшего представления
            var groupedCompetencies = competencies
                .GroupBy(c => c.Name)
                .OrderByDescending(g => g.Sum(x => x.MentionCount))
                .Take(20); // Топ-20 для наглядности

            foreach (var group in groupedCompetencies)
            {
                report.AppendLine($"Компетенция: {group.Key}");
                report.AppendLine("-".PadRight(40, '-'));
                
                foreach (var comp in group.OrderByDescending(c => c.MentionCount))
                {
                    report.AppendLine($"Источник: {GetSourceName(comp.Source)}");
                    report.AppendLine($"Описание: {comp.Description}");
                    report.AppendLine($"Частота в источнике (%): {comp.FrequencyPercent:F2}%");
                    report.AppendLine($"Количество упоминаний: {comp.MentionCount}");
                    report.AppendLine();
                }
                report.AppendLine();
            }

            await File.WriteAllTextAsync(outputPath, report.ToString(), Encoding.UTF8);
            Console.WriteLine($"Табличное представление сохранено: {outputPath}");
        }

        /// <summary>
        /// Б) Создание фильтрации по источникам
        /// </summary>
        public async Task CreateSourceFilteringAsync(List<Competency> competencies, string outputDir)
        {
            // Создаем отдельные файлы для каждого источника
            var sources = Enum.GetValues<CompetencySource>();

            foreach (var source in sources)
            {
                var filteredCompetencies = competencies
                    .Where(c => c.Source == source)
                    .OrderByDescending(c => c.MentionCount)
                    .ToList();

                if (!filteredCompetencies.Any()) continue;

                var report = new StringBuilder();
                report.AppendLine($"КОМПЕТЕНЦИИ ИЗ ИСТОЧНИКА: {GetSourceName(source).ToUpper()}");
                report.AppendLine("=".PadRight(50, '='));
                report.AppendLine($"Всего компетенций: {filteredCompetencies.Count}");
                report.AppendLine($"Общее количество упоминаний: {filteredCompetencies.Sum(c => c.MentionCount)}");
                report.AppendLine();

                report.AppendLine("РАНЖИРОВАННЫЙ СПИСОК:");
                report.AppendLine("-".PadRight(30, '-'));
                
                int rank = 1;
                foreach (var comp in filteredCompetencies)
                {
                    report.AppendLine($"{rank,2}. {comp.Name} - {comp.MentionCount} упоминаний ({comp.FrequencyPercent:F2}%)");
                    rank++;
                }

                var fileName = Path.Combine(outputDir, $"filter_{GetSourceName(source).ToLower().Replace(" ", "_")}.txt");
                await File.WriteAllTextAsync(fileName, report.ToString(), Encoding.UTF8);
            }

            Console.WriteLine($"Фильтрация по источникам сохранена в папке: {outputDir}");
        }

        /// <summary>
        /// В) Создание HeatMap для визуализации частотности
        /// </summary>
        public async Task CreateHeatMapDataAsync(List<Competency> competencies, string outputPath)
        {
            var report = new StringBuilder();
            
            report.AppendLine("HEATMAP ДАННЫЕ ДЛЯ ВИЗУАЛИЗАЦИИ ЧАСТОТНОСТИ");
            report.AppendLine("=".PadRight(50, '='));
            report.AppendLine();

            // Создаем матрицу: компетенции x источники
            var allCompetencies = competencies.Select(c => c.Name).Distinct().OrderBy(x => x).ToList();
            var sources = Enum.GetValues<CompetencySource>().ToList();

            // Заголовок таблицы
            report.Append("Компетенция".PadRight(25));
            foreach (var source in sources)
            {
                report.Append($"{GetSourceName(source)}".PadRight(15));
            }
            report.AppendLine("Всего");
            report.AppendLine("-".PadRight(80, '-'));

            foreach (var competencyName in allCompetencies.Take(30)) // Топ-30 для наглядности
            {
                report.Append(competencyName.PadRight(25));
                
                int totalMentions = 0;
                foreach (var source in sources)
                {
                    var mentions = competencies
                        .Where(c => c.Name == competencyName && c.Source == source)
                        .Sum(c => c.MentionCount);
                    
                    totalMentions += mentions;
                    
                    // Цветовое кодирование через символы
                    string visualization = GetHeatMapSymbol(mentions);
                    report.Append($"{mentions,3}{visualization}".PadRight(15));
                }
                
                report.AppendLine($"{totalMentions,3}");
            }

            report.AppendLine();
            report.AppendLine("ЛЕГЕНДА ЦВЕТОВОГО КОДИРОВАНИЯ:");
            report.AppendLine("█ - Очень высокая частота (>20)");
            report.AppendLine("▓ - Высокая частота (10-20)");
            report.AppendLine("▒ - Средняя частота (5-9)");
            report.AppendLine("░ - Низкая частота (1-4)");
            report.AppendLine("  - Отсутствует (0)");

            await File.WriteAllTextAsync(outputPath, report.ToString(), Encoding.UTF8);
            Console.WriteLine($"HeatMap данные сохранены: {outputPath}");
        }

        /// <summary>
        /// Г) Создание данных для интерактивной визуализации
        /// </summary>
        public async Task CreateInteractiveVisualizationDataAsync(List<Competency> competencies, string outputDir)
        {
            // 1. Облако тегов
            await CreateTagCloudDataAsync(competencies, Path.Combine(outputDir, "tag_cloud_data.txt"));
            
            // 2. Топ-10 диаграмма
            await CreateTop10ChartDataAsync(competencies, Path.Combine(outputDir, "top10_chart_data.txt"));
            
            // 3. Круговая диаграмма по источникам
            await CreatePieChartDataAsync(competencies, Path.Combine(outputDir, "pie_chart_data.txt"));

            Console.WriteLine($"Данные для интерактивной визуализации сохранены в: {outputDir}");
        }

        /// <summary>
        /// Д) Текстовый анализ диспропорций
        /// </summary>
        public async Task CreateDisproportionAnalysisAsync(List<Competency> competencies, string outputPath)
        {
            var report = new StringBuilder();
            
            report.AppendLine("ТЕКСТОВЫЙ АНАЛИЗ ДИСПРОПОРЦИЙ МЕЖДУ ИСТОЧНИКАМИ");
            report.AppendLine("=".PadRight(55, '='));
            report.AppendLine();

            // Анализ компетенций, которые есть в вакансиях, но отсутствуют в ФГОС
            var vacancyCompetencies = competencies.Where(c => c.Source == CompetencySource.Vacancy)
                .Select(c => c.Name).Distinct().ToHashSet();
            
            var fgosCompetencies = competencies.Where(c => c.Source == CompetencySource.FGOS)
                .Select(c => c.Name).Distinct().ToHashSet();

            var profStandardCompetencies = competencies.Where(c => c.Source == CompetencySource.ProfessionalStandard)
                .Select(c => c.Name).Distinct().ToHashSet();

            report.AppendLine("1. КОМПЕТЕНЦИИ, ВОСТРЕБОВАННЫЕ НА РЫНКЕ, НО ОТСУТСТВУЮЩИЕ В ФГОС:");
            report.AppendLine("-".PadRight(60, '-'));
            
            var missingInFgos = vacancyCompetencies.Except(fgosCompetencies).OrderBy(x => x);
            foreach (var comp in missingInFgos.Take(15))
            {
                var vacancyMentions = competencies
                    .Where(c => c.Name == comp && c.Source == CompetencySource.Vacancy)
                    .Sum(c => c.MentionCount);
                report.AppendLine($"• {comp} - {vacancyMentions} упоминаний в вакансиях");
            }

            report.AppendLine();
            report.AppendLine("2. КОМПЕТЕНЦИИ ИЗ ФГОС, СЛАБО ПРЕДСТАВЛЕННЫЕ НА РЫНКЕ:");
            report.AppendLine("-".PadRight(60, '-'));

            var weakInMarket = fgosCompetencies.Except(vacancyCompetencies).OrderBy(x => x);
            foreach (var comp in weakInMarket)
            {
                report.AppendLine($"• {comp} - присутствует в ФГОС, отсутствует в вакансиях");
            }

            report.AppendLine();
            report.AppendLine("3. СТАТИСТИКА ПОКРЫТИЯ:");
            report.AppendLine("-".PadRight(30, '-'));
            report.AppendLine($"Всего уникальных компетенций в вакансиях: {vacancyCompetencies.Count}");
            report.AppendLine($"Всего уникальных компетенций в ФГОС: {fgosCompetencies.Count}");
            report.AppendLine($"Всего уникальных компетенций в профстандартах: {profStandardCompetencies.Count}");
            report.AppendLine($"Пересечение ФГОС и вакансий: {fgosCompetencies.Intersect(vacancyCompetencies).Count()}");
            report.AppendLine($"Покрытие ФГОС рынком: {(double)fgosCompetencies.Intersect(vacancyCompetencies).Count() / fgosCompetencies.Count * 100:F1}%");

            await File.WriteAllTextAsync(outputPath, report.ToString(), Encoding.UTF8);
            Console.WriteLine($"Анализ диспропорций сохранен: {outputPath}");
        }

        /// <summary>
        /// Е) Матрица сравнения компетенций по источникам
        /// </summary>
        public async Task CreateComparisonMatrixAsync(List<Competency> competencies, string outputPath)
        {
            var report = new StringBuilder();
            
            report.AppendLine("МАТРИЦА СРАВНЕНИЯ КОМПЕТЕНЦИЙ ПО ИСТОЧНИКАМ");
            report.AppendLine("=".PadRight(50, '='));
            report.AppendLine();

            var allCompetencies = competencies.Select(c => c.Name).Distinct().OrderBy(x => x).ToList();
            var sources = new[] { CompetencySource.FGOS, CompetencySource.ProfessionalStandard, CompetencySource.Vacancy };

            // Заголовок
            report.Append("Компетенция".PadRight(30));
            foreach (var source in sources)
            {
                report.Append(GetSourceName(source).PadRight(15));
            }
            report.AppendLine();
            report.AppendLine("-".PadRight(75, '-'));

            foreach (var competencyName in allCompetencies.Take(25)) // Топ-25
            {
                report.Append(competencyName.PadRight(30));
                
                foreach (var source in sources)
                {
                    bool hasCompetency = competencies.Any(c => c.Name == competencyName && c.Source == source);
                    string symbol = hasCompetency ? "да" : "нет";
                    report.Append($"{symbol}".PadRight(15));
                }
                report.AppendLine();
            }

            report.AppendLine();
            report.AppendLine("ЛЕГЕНДА:");
            report.AppendLine("да - Компетенция присутствует в источнике");
            report.AppendLine("нет - Компетенция отсутствует в источнике");

            await File.WriteAllTextAsync(outputPath, report.ToString(), Encoding.UTF8);
            Console.WriteLine($"Матрица сравнения сохранена: {outputPath}");
        }

        #region Вспомогательные методы

        private async Task CreateTagCloudDataAsync(List<Competency> competencies, string outputPath)
        {
            var report = new StringBuilder();
            report.AppendLine("ДАННЫЕ ДЛЯ ОБЛАКА ТЕГОВ");
            report.AppendLine("=".PadRight(30, '='));
            report.AppendLine();

            var tagData = competencies
                .GroupBy(c => c.Name)
                .Select(g => new { Name = g.Key, Weight = g.Sum(x => x.MentionCount) })
                .OrderByDescending(x => x.Weight)
                .Take(50);

            foreach (var tag in tagData)
            {
                report.AppendLine($"{tag.Name}:{tag.Weight}");
            }

            await File.WriteAllTextAsync(outputPath, report.ToString(), Encoding.UTF8);
        }

        private async Task CreateTop10ChartDataAsync(List<Competency> competencies, string outputPath)
        {
            var report = new StringBuilder();
            report.AppendLine("ДАННЫЕ ДЛЯ ТОП-10 ДИАГРАММЫ");
            report.AppendLine("=".PadRight(30, '='));
            report.AppendLine();

            var top10 = competencies
                .GroupBy(c => c.Name)
                .Select(g => new { Name = g.Key, Count = g.Sum(x => x.MentionCount) })
                .OrderByDescending(x => x.Count)
                .Take(10);

            foreach (var item in top10)
            {
                report.AppendLine($"{item.Name},{item.Count}");
            }

            await File.WriteAllTextAsync(outputPath, report.ToString(), Encoding.UTF8);
        }

        private async Task CreatePieChartDataAsync(List<Competency> competencies, string outputPath)
        {
            var report = new StringBuilder();
            report.AppendLine("ДАННЫЕ ДЛЯ КРУГОВОЙ ДИАГРАММЫ ПО ИСТОЧНИКАМ");
            report.AppendLine("=".PadRight(40, '='));
            report.AppendLine();

            var sourceData = competencies
                .GroupBy(c => c.Source)
                .Select(g => new { Source = GetSourceName(g.Key), Count = g.Count() });

            foreach (var item in sourceData)
            {
                report.AppendLine($"{item.Source},{item.Count}");
            }

            await File.WriteAllTextAsync(outputPath, report.ToString(), Encoding.UTF8);
        }

        private string GetHeatMapSymbol(int mentions)
        {
            return mentions switch
            {
                > 20 => "█",
                >= 10 => "▓",
                >= 5 => "▒",
                >= 1 => "░",
                _ => " "
            };
        }

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

        #endregion
    }
}
