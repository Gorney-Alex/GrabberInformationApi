using CompetencyParser.Models;
using System.Text;

namespace CompetencyParser.Services
{  
    public class SimpleHtmlReportService
    {
        // Создаем HTML отчет
        public async Task CreateInteractiveHtmlReportAsync(List<Competency> competencies, string outputPath)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <title>Анализ компетенций ИТ-специалистов</title>");
            html.AppendLine("    <style>");
            
            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
            html.AppendLine("        .header { background: #007acc; color: white; padding: 20px; text-align: center; }");
            html.AppendLine("        .nav { background: white; padding: 10px; text-align: center; margin: 10px 0; }");
            html.AppendLine("        .nav button { background: #007acc; color: white; border: none; padding: 10px 20px; margin: 5px; cursor: pointer; }");
            html.AppendLine("        .nav button:hover { background: #005f99; }");
            html.AppendLine("        .section { display: none; padding: 20px; background: white; margin: 10px 0; }");
            html.AppendLine("        .section.active { display: block; }");
            html.AppendLine("        .stats { display: flex; gap: 20px; margin: 20px 0; }");
            html.AppendLine("        .stat-card { background: #e6f3ff; padding: 15px; border-radius: 5px; text-align: center; flex: 1; }");
            html.AppendLine("        .skill-item { background: #f9f9f9; margin: 5px 0; padding: 10px; border-left: 4px solid #007acc; }");
            html.AppendLine("        table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            html.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("        th { background: #007acc; color: white; }");
            html.AppendLine("        .has-comp { background: #d4edda; color: #155724; text-align: center; }");
            html.AppendLine("        .no-comp { background: #f8d7da; color: #721c24; text-align: center; }");
            
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Заголовок
            html.AppendLine("    <div class='header'>");
            html.AppendLine("        <h1>Анализ компетенций ИТ-специалистов</h1>");
            html.AppendLine($"        <p>Отчет создан: {DateTime.Now:dd.MM.yyyy HH:mm}</p>");
            html.AppendLine("        <p>Выполнил: Студент 3 курса</p>");
            html.AppendLine("    </div>");

            // Навигация
            html.AppendLine("    <div class='nav'>");
            html.AppendLine("        <button onclick='showSection(\"overview\")'>Обзор</button>");
            html.AppendLine("        <button onclick='showSection(\"top-skills\")'>Топ навыки</button>");
            html.AppendLine("        <button onclick='showSection(\"sources\")'>По источникам</button>");
            html.AppendLine("        <button onclick='showSection(\"matrix\")'>Матрица</button>");
            html.AppendLine("    </div>");

            // Секция обзора
            html.AppendLine("    <div id='overview' class='section active'>");
            AddOverviewSection(html, competencies);
            html.AppendLine("    </div>");

            // Секция топ навыков
            html.AppendLine("    <div id='top-skills' class='section'>");
            AddTopSkillsSection(html, competencies);
            html.AppendLine("    </div>");

            // Секция по источникам
            html.AppendLine("    <div id='sources' class='section'>");
            AddSourcesSection(html, competencies);
            html.AppendLine("    </div>");

            // Секция матрицы
            html.AppendLine("    <div id='matrix' class='section'>");
            AddMatrixSection(html, competencies);
            html.AppendLine("    </div>");

            // JavaScript
            html.AppendLine("    <script>");
            html.AppendLine("        function showSection(sectionId) {");
            html.AppendLine("            document.querySelectorAll('.section').forEach(s => s.classList.remove('active'));");
            html.AppendLine("            document.getElementById(sectionId).classList.add('active');");
            html.AppendLine("        }");
            html.AppendLine("    </script>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            await File.WriteAllTextAsync(outputPath, html.ToString(), Encoding.UTF8);
            Console.WriteLine($"HTML отчет создан: {outputPath}");
        }

        // Добавляем секцию обзора
        private void AddOverviewSection(StringBuilder html, List<Competency> competencies)
        {
            html.AppendLine("        <h2>Общий обзор</h2>");

            var totalCompetencies = competencies.Count;
            var totalMentions = competencies.Sum(c => c.MentionCount);
            var vacancyComp = competencies.Where(c => c.Source == CompetencySource.Vacancy).Count();
            var profComp = competencies.Where(c => c.Source == CompetencySource.ProfessionalStandard).Count();
            var fgosComp = competencies.Where(c => c.Source == CompetencySource.FGOS).Count();

            html.AppendLine("        <div class='stats'>");
            html.AppendLine("            <div class='stat-card'>");
            html.AppendLine($"                <h3>{totalCompetencies}</h3>");
            html.AppendLine("                <p>Всего компетенций</p>");
            html.AppendLine("            </div>");
            html.AppendLine("            <div class='stat-card'>");
            html.AppendLine($"                <h3>{totalMentions}</h3>");
            html.AppendLine("                <p>Общие упоминания</p>");
            html.AppendLine("            </div>");
            html.AppendLine("            <div class='stat-card'>");
            html.AppendLine($"                <h3>{vacancyComp}</h3>");
            html.AppendLine("                <p>Из вакансий</p>");
            html.AppendLine("            </div>");
            html.AppendLine("            <div class='stat-card'>");
            html.AppendLine($"                <h3>{profComp}</h3>");
            html.AppendLine("                <p>Из профстандартов</p>");
            html.AppendLine("            </div>");
            html.AppendLine("            <div class='stat-card'>");
            html.AppendLine($"                <h3>{fgosComp}</h3>");
            html.AppendLine("                <p>Из ФГОС</p>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
        }

        // Добавляем секцию топ навыков
        private void AddTopSkillsSection(StringBuilder html, List<Competency> competencies)
        {
            html.AppendLine("        <h2>Топ-15 самых востребованных навыков</h2>");

            var topSkills = competencies.OrderByDescending(c => c.MentionCount).Take(15);

            foreach (var skill in topSkills)
            {
                html.AppendLine("        <div class='skill-item'>");
                html.AppendLine($"            <strong>{skill.Name}</strong> - {skill.MentionCount} упоминаний ({skill.FrequencyPercent:F1}%)");
                html.AppendLine("        </div>");
            }
        }

        // Добавляем секцию по источникам
        private void AddSourcesSection(StringBuilder html, List<Competency> competencies)
        {
            html.AppendLine("        <h2>Анализ по источникам</h2>");

            var sources = new[] { CompetencySource.FGOS, CompetencySource.ProfessionalStandard, CompetencySource.Vacancy };
            var sourceNames = new[] { "ФГОС", "Профстандарты", "Вакансии" };

            html.AppendLine("        <div class='stats'>");
            for (int i = 0; i < sources.Length; i++)
            {
                var sourceComps = competencies.Where(c => c.Source == sources[i]).ToList();
                var topInSource = sourceComps.OrderByDescending(c => c.MentionCount).Take(5);

                html.AppendLine("            <div class='stat-card'>");
                html.AppendLine($"                <h3>{sourceNames[i]}</h3>");
                html.AppendLine($"                <p>Компетенций: {sourceComps.Count}</p>");
                html.AppendLine($"                <p>Упоминаний: {sourceComps.Sum(c => c.MentionCount)}</p>");
                html.AppendLine("                <h4>Топ-5:</h4>");
                html.AppendLine("                <ul>");
                foreach (var comp in topInSource)
                {
                    html.AppendLine($"                    <li>{comp.Name} ({comp.MentionCount})</li>");
                }
                html.AppendLine("                </ul>");
                html.AppendLine("            </div>");
            }
            html.AppendLine("        </div>");
        }

        // Добавляем матрицу сравнения
        private void AddMatrixSection(StringBuilder html, List<Competency> competencies)
        {
            html.AppendLine("        <h2>Матрица компетенций по источникам</h2>");

            var topCompetencies = competencies
                .GroupBy(c => c.Name)
                .OrderByDescending(g => g.Sum(x => x.MentionCount))
                .Take(20);

            html.AppendLine("        <table>");
            html.AppendLine("            <tr>");
            html.AppendLine("                <th>Компетенция</th>");
            html.AppendLine("                <th>ФГОС</th>");
            html.AppendLine("                <th>Профстандарт</th>");
            html.AppendLine("                <th>Вакансия</th>");
            html.AppendLine("            </tr>");

            foreach (var group in topCompetencies)
            {
                html.AppendLine("            <tr>");
                html.AppendLine($"                <td><strong>{group.Key}</strong></td>");

                var hasFgos = group.Any(c => c.Source == CompetencySource.FGOS);
                var hasProf = group.Any(c => c.Source == CompetencySource.ProfessionalStandard);
                var hasVacancy = group.Any(c => c.Source == CompetencySource.Vacancy);

                html.AppendLine($"                <td class='{(hasFgos ? "has-comp" : "no-comp")}'>{(hasFgos ? "✓" : "✗")}</td>");
                html.AppendLine($"                <td class='{(hasProf ? "has-comp" : "no-comp")}'>{(hasProf ? "✓" : "✗")}</td>");
                html.AppendLine($"                <td class='{(hasVacancy ? "has-comp" : "no-comp")}'>{(hasVacancy ? "✓" : "✗")}</td>");
                html.AppendLine("            </tr>");
            }

            html.AppendLine("        </table>");
        }
    }
}
