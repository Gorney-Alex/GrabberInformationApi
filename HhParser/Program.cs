using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var hhParser = new HHruParser();
        string[] queries = { "Программист", "Разработчик игр" };
        int pagesToFetch = 3;
        var hhCompetencies = await hhParser.ParseVacanciesAsync(queries, pagesToFetch);

        var fgosParser = new FgosParser();
        string fgosUrl = "http://fgosvo.ru/uploadfiles/fgos/090401.pdf";
        string pdfPath = await fgosParser.DownloadPdfAsync(fgosUrl);
        string fgosText = fgosParser.ExtractText(pdfPath);
        var fgosCompetencies = fgosParser.ParseCompetencies(fgosText, "09.04.01");
        var fgosIndicators = fgosParser.ParseIndicators(fgosText, "09.04.01");

        var profParser = new ProfStandardParser();
        string profUrl = "https://ваш_реальный_ссылка_на_профстандарт.html";
        string profHtml = await profParser.DownloadHtmlAsync(profUrl);
        var profFunctions = profParser.ExtractFunctions(profHtml, "Программист");
        var profActions = profParser.ExtractActions(profHtml, "Программист");

        Console.WriteLine("\n===== ВАКАНСИИ HH.ru =====");
        Console.WriteLine("{0,-25} | {1,-15} | {2,-30} | {3,10}", "Компетенция", "Источник", "Описание", "Частота (%)");
        Console.WriteLine(new string('-', 90));
        foreach (var comp in hhCompetencies)
        {
            Console.WriteLine("{0,-25} | {1,-15} | {2,-30} | {3,10:F2}",
                comp.Name, comp.Source, comp.Description, comp.FrequencyPercent);
        }

        Console.WriteLine("\n===== ФГОС =====");
        Console.WriteLine("{0,-10} | {1,-70} | {2,-20}", "Код", "Описание", "Тип");
        Console.WriteLine(new string('-', 110));
        foreach (var fg in fgosCompetencies)
        {
            Console.WriteLine("{0,-10} | {1,-70} | {2,-20}", fg.Code, fg.Description, fg.Type);
        }

        Console.WriteLine("\n===== ИНДИКАТОРЫ ФГОС =====");
        Console.WriteLine("{0,-15} | {1,-10} | {2,-70}", "Код Индикатора", "Компетенция", "Описание");
        Console.WriteLine(new string('-', 100));
        foreach (var ind in fgosIndicators)
        {
            Console.WriteLine("{0,-15} | {1,-10} | {2,-70}", ind.IndicatorCode, ind.CompetencyCode, ind.Description);
        }

        Console.WriteLine("\n===== Профстандарт - Обобщенные трудовые функции =====");
        Console.WriteLine("{0,-10} | {1,-80}", "Код", "Описание");
        Console.WriteLine(new string('-', 100));
        foreach (var f in profFunctions)
        {
            Console.WriteLine("{0,-10} | {1,-80}", f.Code, f.Description);
        }

        Console.WriteLine("\n===== Профстандарт - Трудовые действия =====");
        Console.WriteLine("{0,-10} | {1,-80}", "Код", "Описание");
        Console.WriteLine(new string('-', 100));
        foreach (var a in profActions)
        {
            Console.WriteLine("{0,-10} | {1,-80}", a.Code, a.Description);
        }

        Console.WriteLine("\nВсе данные успешно собраны и выведены!");
    }
}
