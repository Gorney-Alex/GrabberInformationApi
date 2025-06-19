using CompetencyParser.Models;
using Newtonsoft.Json;
using System.Text;

namespace CompetencyParser.Services
{
    /// <summary>
    /// Сервис для создания HTML-отчетов с интерактивной визуализацией
    /// </summary>
    public class HtmlReportService
    {
        /// <summary>
        /// Создание интерактивного HTML-отчета
        /// </summary>
        public async Task CreateInteractiveHtmlReportAsync(List<Competency> competencies, string outputPath)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='ru'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>Анализ компетенций ИТ-специалистов</title>");
            html.AppendLine("    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
            html.AppendLine("    <style>");
            html.AppendLine(GetCssStyles());
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Заголовок
            html.AppendLine("    <div class='header'>");
            html.AppendLine("        <h1>Анализ компетенций ИТ-специалистов</h1>");
            html.AppendLine($"        <p>Отчет сгенерирован: {DateTime.Now:dd.MM.yyyy HH:mm}</p>");
            html.AppendLine("    </div>");

            // Навигация
            html.AppendLine("    <nav class='navigation'>");
            html.AppendLine("        <button onclick='showSection(\"overview\")'>Обзор</button>");
            html.AppendLine("        <button onclick='showSection(\"top-skills\")'>Топ навыки</button>");
            html.AppendLine("        <button onclick='showSection(\"sources\")'>По источникам</button>");
            html.AppendLine("        <button onclick='showSection(\"matrix\")'>Матрица</button>");
            html.AppendLine("    </nav>");

            // Секция обзора
            html.AppendLine("    <div id='overview' class='section active'>");
            await AddOverviewSection(html, competencies);
            html.AppendLine("    </div>");

            // Секция топ навыков
            html.AppendLine("    <div id='top-skills' class='section'>");
            await AddTopSkillsSection(html, competencies);
            html.AppendLine("    </div>");

            // Секция по источникам
            html.AppendLine("    <div id='sources' class='section'>");
            await AddSourcesSection(html, competencies);
            html.AppendLine("    </div>");

            // Секция матрицы
            html.AppendLine("    <div id='matrix' class='section'>");
            await AddMatrixSection(html, competencies);
            html.AppendLine("    </div>");

            html.AppendLine("    <script>");
            html.AppendLine(GetJavaScript(competencies));
            html.AppendLine("    </script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            await File.WriteAllTextAsync(outputPath, html.ToString(), Encoding.UTF8);
            Console.WriteLine($"Интерактивный HTML-отчет создан: {outputPath}");
        }

        private async Task AddOverviewSection(StringBuilder html, List<Competency> competencies)
        {
            html.AppendLine("        <h2>Общий обзор</h2>");
            html.AppendLine("        <div class='stats-grid'>");
            
            var totalCompetencies = competencies.Count;
            var totalMentions = competencies.Sum(c => c.MentionCount);
            var sourceStats = competencies.GroupBy(c => c.Source).ToDictionary(g => g.Key, g => g.Count());

            html.AppendLine($"            <div class='stat-card'>");
            html.AppendLine($"                <div class='stat-number'>{totalCompetencies}</div>");
            html.AppendLine($"                <div class='stat-label'>Всего компетенций</div>");
            html.AppendLine($"            </div>");

            html.AppendLine($"            <div class='stat-card'>");
            html.AppendLine($"                <div class='stat-number'>{totalMentions}</div>");
            html.AppendLine($"                <div class='stat-label'>Общие упоминания</div>");
            html.AppendLine($"            </div>");

            foreach (var source in sourceStats)
            {
                html.AppendLine($"            <div class='stat-card'>");
                html.AppendLine($"                <div class='stat-number'>{source.Value}</div>");
                html.AppendLine($"                <div class='stat-label'>{GetSourceName(source.Key)}</div>");
                html.AppendLine($"            </div>");
            }

            html.AppendLine("        </div>");
            html.AppendLine("        <canvas id='overviewChart' width='400' height='200'></canvas>");
        }

        private async Task AddTopSkillsSection(StringBuilder html, List<Competency> competencies)
        {
            html.AppendLine("        <h2>Топ-15 самых востребованных навыков</h2>");
            html.AppendLine("        <div class='skills-list'>");

            var topSkills = competencies
                .GroupBy(c => c.Name)
                .Select(g => new { Name = g.Key, Count = g.Sum(x => x.MentionCount) })
                .OrderByDescending(x => x.Count)
                .Take(15);

            foreach (var skill in topSkills)
            {
                var percentage = (double)skill.Count / competencies.Sum(c => c.MentionCount) * 100;
                html.AppendLine($"                <div class='skill-item'>");
                html.AppendLine($"                    <span class='skill-name'>{skill.Name}</span>");
                html.AppendLine($"                    <span class='skill-count'>{skill.Count} ({percentage:F1}%)</span>");
                html.AppendLine($"                    <div class='skill-bar' style='width: {Math.Min(percentage * 3, 100)}%'></div>");
                html.AppendLine($"                </div>");
            }

            html.AppendLine("        </div>");
        }

        private async Task AddSourcesSection(StringBuilder html, List<Competency> competencies)
        {
            html.AppendLine("        <h2>Анализ по источникам</h2>");
            html.AppendLine("        <div class='sources-container'>");
            
            var sources = Enum.GetValues<CompetencySource>();
            
            foreach (var source in sources)
            {
                var sourceCompetencies = competencies.Where(c => c.Source == source).ToList();
                if (!sourceCompetencies.Any()) continue;

                html.AppendLine($"            <div class='source-card'>");
                html.AppendLine($"                <h3>{GetSourceName(source)}</h3>");
                html.AppendLine($"                <p><strong>Компетенций:</strong> {sourceCompetencies.Count}</p>");
                html.AppendLine($"                <p><strong>Упоминаний:</strong> {sourceCompetencies.Sum(c => c.MentionCount)}</p>");
                
                var topInSource = sourceCompetencies.OrderByDescending(c => c.MentionCount).Take(5);
                html.AppendLine($"                <h4>Топ-5:</h4>");
                html.AppendLine($"                <ul>");
                foreach (var comp in topInSource)
                {
                    html.AppendLine($"                    <li>{comp.Name} ({comp.MentionCount})</li>");
                }
                html.AppendLine($"                </ul>");
                html.AppendLine($"            </div>");
            }
            
            html.AppendLine("        </div>");
        }

        private async Task AddMatrixSection(StringBuilder html, List<Competency> competencies)
        {
            html.AppendLine("        <h2>Матрица компетенций по источникам</h2>");
            html.AppendLine("        <div class='matrix-container'>");
            html.AppendLine("            <table class='matrix-table'>");
            
            var allCompetencies = competencies
                .GroupBy(c => c.Name)
                .OrderByDescending(g => g.Sum(x => x.MentionCount))
                .Take(20)
                .Select(g => g.Key)
                .ToList();
            
            var sources = new[] { CompetencySource.FGOS, CompetencySource.ProfessionalStandard, CompetencySource.Vacancy };

            // Заголовок
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Компетенция</th>");
            foreach (var source in sources)
            {
                html.AppendLine($"                        <th>{GetSourceName(source)}</th>");
            }
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var competencyName in allCompetencies)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td class='competency-name'>{competencyName}</td>");
                
                foreach (var source in sources)
                {
                    bool hasCompetency = competencies.Any(c => c.Name == competencyName && c.Source == source);
                    string cellClass = hasCompetency ? "has-competency" : "no-competency";
                    string symbol = hasCompetency ? "✓" : "✗";
                    html.AppendLine($"                        <td class='{cellClass}'>{symbol}</td>");
                }
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
        }

        private string GetCssStyles()
        {
            return @"
                body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; background: #f5f7fa; line-height: 1.6; }
                .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 2rem; text-align: center; }
                .header h1 { margin: 0; font-size: 2.5rem; }
                .navigation { background: white; padding: 1rem; text-align: center; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
                .navigation button { background: #667eea; color: white; border: none; padding: 0.8rem 1.5rem; margin: 0 0.5rem; border-radius: 25px; cursor: pointer; transition: all 0.3s; }
                .navigation button:hover { background: #764ba2; transform: translateY(-2px); }
                .section { display: none; padding: 2rem; max-width: 1200px; margin: 0 auto; }
                .section.active { display: block; }
                .stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin: 2rem 0; }
                .stat-card { background: white; padding: 1.5rem; border-radius: 10px; text-align: center; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
                .stat-number { font-size: 2rem; font-weight: bold; color: #667eea; }
                .stat-label { color: #666; margin-top: 0.5rem; }
                .skills-list { margin: 2rem 0; }
                .skill-item { background: white; margin: 0.5rem 0; padding: 1rem; border-radius: 8px; position: relative; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
                .skill-name { font-weight: bold; font-size: 1.1rem; }
                .skill-count { float: right; color: #667eea; font-weight: bold; }
                .skill-bar { position: absolute; bottom: 0; left: 0; height: 4px; background: linear-gradient(90deg, #667eea, #764ba2); }
                .sources-container { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 1rem; margin: 2rem 0; }
                .source-card { background: white; padding: 1.5rem; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
                .source-card h3 { color: #667eea; margin-top: 0; }
                .matrix-container { overflow-x: auto; margin: 2rem 0; }
                .matrix-table { width: 100%; border-collapse: collapse; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
                .matrix-table th, .matrix-table td { padding: 0.8rem; text-align: center; border-bottom: 1px solid #eee; }
                .matrix-table th { background: #667eea; color: white; font-weight: bold; }
                .competency-name { text-align: left !important; font-weight: bold; max-width: 200px; }
                .has-competency { background: #d4edda; color: #155724; font-weight: bold; }
                .no-competency { background: #f8d7da; color: #721c24; }
            ";
        }

        private string GetJavaScript(List<Competency> competencies)
        {
            return @"
                function showSection(sectionId) {
                    document.querySelectorAll('.section').forEach(s => s.classList.remove('active'));
                    document.getElementById(sectionId).classList.add('active');
                }
                
                // Инициализация при загрузке
                document.addEventListener('DOMContentLoaded', function() {
                    showSection('overview');
                });
            ";
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
    }
}
