using CompetencyParser.Models;
using System.Text;

namespace CompetencyParser.Services
{
    public class SimpleVisualizationService
    {
        public async Task CreateTableRepresentationAsync(List<Competency> competencies, string outputPath)
        {
            var lines = new List<string>();
            
            lines.Add("ТАБЛИЦА РЕЗУЛЬТАТОВ");
            lines.Add("==================");
            lines.Add("");

            var topCompetencies = competencies
                .GroupBy(c => c.Name)
                .OrderByDescending(g => g.Sum(x => x.MentionCount))
                .Take(20);

            foreach (var group in topCompetencies)
            {
                lines.Add($"Компетенция: {group.Key}");
                lines.Add("----------------------------");
                
                foreach (var comp in group.OrderByDescending(c => c.MentionCount))
                {
                    string sourceName = GetSimpleSourceName(comp.Source);
                    
                    lines.Add($"Источник: {sourceName}");
                    lines.Add($"Описание: {comp.Description}");
                    lines.Add($"Частота (%): {comp.FrequencyPercent:F2}%");
                    lines.Add($"Упоминаний: {comp.MentionCount}");
                    lines.Add("");
                }
                lines.Add("");
            }

            await File.WriteAllLinesAsync(outputPath, lines, Encoding.UTF8);
            Console.WriteLine($"Табличное представление сохранено: {outputPath}");
        }

        public async Task CreateSourceFilteringAsync(List<Competency> competencies, string outputDir)
        {
            var sources = new[] { CompetencySource.FGOS, CompetencySource.ProfessionalStandard, CompetencySource.Vacancy };
            var sourceFiles = new[] { "фгос", "профстандарт", "вакансия" };

            for (int i = 0; i < sources.Length; i++)
            {
                var filtered = competencies
                    .Where(c => c.Source == sources[i])
                    .OrderByDescending(c => c.MentionCount)
                    .ToList();

                if (filtered.Count == 0) continue;

                var lines = new List<string>();
                lines.Add($"КОМПЕТЕНЦИИ ИЗ ИСТОЧНИКА: {sourceFiles[i].ToUpper()}");
                lines.Add("==================================================");
                lines.Add($"Всего компетенций: {filtered.Count}");
                lines.Add($"Общее количество упоминаний: {filtered.Sum(c => c.MentionCount)}");
                lines.Add("");
                lines.Add("СПИСОК ПО УБЫВАНИЮ:");
                lines.Add("------------------------------");
                
                for (int j = 0; j < filtered.Count; j++)
                {
                    var comp = filtered[j];
                    lines.Add($"{j + 1,2}. {comp.Name} - {comp.MentionCount} упоминаний ({comp.FrequencyPercent:F2}%)");
                }

                string fileName = Path.Combine(outputDir, $"filter_{sourceFiles[i]}.txt");
                await File.WriteAllLinesAsync(fileName, lines, Encoding.UTF8);
            }

            Console.WriteLine($"Фильтрация по источникам сохранена в папке: {outputDir}");
        }

        public async Task CreateHeatMapDataAsync(List<Competency> competencies, string outputPath)
        {
            var lines = new List<string>();
            
            lines.Add("ДАННЫЕ ДЛЯ ТЕПЛОВОЙ КАРТЫ");
            lines.Add("=========================");
            lines.Add("");
            lines.Add("Формат: Компетенция | ФГОС | Профстандарт | Вакансия");
            lines.Add("");

            var competencyGroups = competencies
                .GroupBy(c => c.Name)
                .OrderByDescending(g => g.Sum(x => x.MentionCount))
                .Take(30);

            foreach (var group in competencyGroups)
            {
                var fgosCount = group.Where(c => c.Source == CompetencySource.FGOS).Sum(c => c.MentionCount);
                var profCount = group.Where(c => c.Source == CompetencySource.ProfessionalStandard).Sum(c => c.MentionCount);
                var vacancyCount = group.Where(c => c.Source == CompetencySource.Vacancy).Sum(c => c.MentionCount);

                lines.Add($"{group.Key} | {fgosCount} | {profCount} | {vacancyCount}");
            }

            await File.WriteAllLinesAsync(outputPath, lines, Encoding.UTF8);
            Console.WriteLine($"HeatMap данные сохранены: {outputPath}");
        }

        public async Task CreateInteractiveVisualizationDataAsync(List<Competency> competencies, string outputDir)
        {
            var tagCloudLines = new List<string>();
            tagCloudLines.Add("ДАННЫЕ ДЛЯ ОБЛАКА ТЕГОВ");
            tagCloudLines.Add("=======================");
            tagCloudLines.Add("");

            var topSkills = competencies.OrderByDescending(c => c.MentionCount).Take(50);
            foreach (var skill in topSkills)
            {
                tagCloudLines.Add($"{skill.Name}: {skill.MentionCount}");
            }

            await File.WriteAllLinesAsync(Path.Combine(outputDir, "tag_cloud_data.txt"), tagCloudLines, Encoding.UTF8);

            var top10Lines = new List<string>();
            top10Lines.Add("ДАННЫЕ ДЛЯ ТОП-10 ДИАГРАММЫ");
            top10Lines.Add("============================");
            top10Lines.Add("");

            var top10 = competencies.OrderByDescending(c => c.MentionCount).Take(10);
            foreach (var skill in top10)
            {
                top10Lines.Add($"{skill.Name}: {skill.MentionCount}");
            }

            await File.WriteAllLinesAsync(Path.Combine(outputDir, "top10_chart_data.txt"), top10Lines, Encoding.UTF8);

            var pieLines = new List<string>();
            pieLines.Add("ДАННЫЕ ДЛЯ КРУГОВОЙ ДИАГРАММЫ");
            pieLines.Add("==============================");
            pieLines.Add("");

            var sourceStats = competencies.GroupBy(c => c.Source)
                .Select(g => new { Source = g.Key, Count = g.Count() });

            foreach (var stat in sourceStats)
            {
                string sourceName = GetSimpleSourceName(stat.Source);
                pieLines.Add($"{sourceName}: {stat.Count}");
            }

            await File.WriteAllLinesAsync(Path.Combine(outputDir, "pie_chart_data.txt"), pieLines, Encoding.UTF8);

            Console.WriteLine($"Данные для интерактивной визуализации сохранены в: {outputDir}");
        }

        public async Task CreateDisproportionAnalysisAsync(List<Competency> competencies, string outputPath)
        {
            var lines = new List<string>();
            
            lines.Add("АНАЛИЗ РАЗЛИЧИЙ МЕЖДУ ИСТОЧНИКАМИ");
            lines.Add("=================================");
            lines.Add("");

            var vacancySkills = competencies.Where(c => c.Source == CompetencySource.Vacancy).Select(c => c.Name).Distinct();
            var fgosSkills = competencies.Where(c => c.Source == CompetencySource.FGOS).Select(c => c.Name).Distinct();
            var missingInFgos = vacancySkills.Except(fgosSkills).ToList();

            lines.Add("1. КОМПЕТЕНЦИИ, НУЖНЫЕ НА РАБОТЕ, НО ОТСУТСТВУЮЩИЕ В ФГОС:");
            lines.Add("------------------------------------------------------------");
            foreach (var skill in missingInFgos.Take(15))
            {
                var count = competencies.Where(c => c.Name == skill && c.Source == CompetencySource.Vacancy).Sum(c => c.MentionCount);
                lines.Add($"• {skill} - {count} упоминаний в вакансиях");
            }
            lines.Add("");

            var unusedFgos = fgosSkills.Except(vacancySkills).ToList();
            lines.Add("2. КОМПЕТЕНЦИИ ИЗ ФГОС, КОТОРЫЕ МАЛО НУЖНЫ НА РЫНКЕ:");
            lines.Add("------------------------------------------------------------");
            foreach (var skill in unusedFgos)
            {
                lines.Add($"• {skill} - есть в ФГОС, но нет в вакансиях");
            }
            lines.Add("");

            lines.Add("3. ОБЩАЯ СТАТИСТИКА:");
            lines.Add("------------------------------");
            lines.Add($"Всего компетенций в вакансиях: {vacancySkills.Count()}");
            lines.Add($"Всего компетенций в ФГОС: {fgosSkills.Count()}");
            lines.Add($"Пересекающихся компетенций: {vacancySkills.Intersect(fgosSkills).Count()}");

            await File.WriteAllLinesAsync(outputPath, lines, Encoding.UTF8);
            Console.WriteLine($"Анализ диспропорций сохранен: {outputPath}");
        }

        public async Task CreateComparisonMatrixAsync(List<Competency> competencies, string outputPath)
        {
            var lines = new List<string>();
            
            lines.Add("МАТРИЦА СРАВНЕНИЯ КОМПЕТЕНЦИЙ ПО ИСТОЧНИКАМ");
            lines.Add("============================================");
            lines.Add("");
            lines.Add("Формат: Компетенция | ФГОС | Профстандарт | Вакансия");
            lines.Add("+ есть компетенция, - нет компетенции");
            lines.Add("");

            var topCompetencies = competencies
                .GroupBy(c => c.Name)
                .OrderByDescending(g => g.Sum(x => x.MentionCount))
                .Take(25);

            foreach (var group in topCompetencies)
            {
                var hasFgos = group.Any(c => c.Source == CompetencySource.FGOS);
                var hasProf = group.Any(c => c.Source == CompetencySource.ProfessionalStandard);
                var hasVacancy = group.Any(c => c.Source == CompetencySource.Vacancy);

                string fgosSymbol = hasFgos ? "+" : "-";
                string profSymbol = hasProf ? "+" : "-";
                string vacancySymbol = hasVacancy ? "+" : "-";

                lines.Add($"{group.Key.PadRight(25)} | {fgosSymbol,5} | {profSymbol,12} | {vacancySymbol,8}");
            }

            await File.WriteAllLinesAsync(outputPath, lines, Encoding.UTF8);
            Console.WriteLine($"Матрица сравнения сохранена: {outputPath}");
        }
        private string GetSimpleSourceName(CompetencySource source)
        {
            switch (source)
            {
                case CompetencySource.FGOS:
                    return "ФГОС";
                case CompetencySource.ProfessionalStandard:
                    return "Профстандарт";
                case CompetencySource.Vacancy:
                    return "Вакансия";
                default:
                    return "Неизвестно";
            }
        }
    }
}
