using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var parser = new HHruParser();

        string[] queries = { "Программист", "Разработчик игр" };
        int pagesToFetch = 3;

        try
        {
            var competencies = await parser.ParseVacanciesAsync(queries, pagesToFetch);

            Console.WriteLine("\nТаблица компетенций:");
            Console.WriteLine("{0,-25} | {1,-15} | {2,-30} | {3,10}", "Компетенция", "Источник", "Описание", "Частота (%)");
            Console.WriteLine(new string('-', 90));
            foreach (var comp in competencies)
            {
                Console.WriteLine("{0,-25} | {1,-15} | {2,-30} | {3,10:F2}",
                    comp.Name, comp.Source, comp.Description, comp.FrequencyPercent);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}
