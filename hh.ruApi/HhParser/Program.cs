using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

class CompetencyResult
{
    public string Name { get; set; }
    public string Source { get; set; }
    public string Description { get; set; }
    public double FrequencyPercent { get; set; }
}

class Program
{
    static readonly HttpClient client = new HttpClient();

    static async Task Main()
    {
        if (!client.DefaultRequestHeaders.Contains("User-Agent"))
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        string[] searchQueries = {"Программист", "Разработчик игр"};
        int pagesToFetch = 3;

        Dictionary<string, int> skillFrequency = new Dictionary<string, int>();
        int totalVacancies = 0;

        try
        {
            foreach (string searchQuery in searchQueries)
            {
                Console.WriteLine($"\nПоиск по запросу: {searchQuery}");
                
                for (int page = 0; page < pagesToFetch; page++)
                {
                    string url = $"https://api.hh.ru/vacancies?text={Uri.EscapeDataString(searchQuery)}&page={page}&per_page=20";
                    Console.WriteLine($"Запрос: {url}");
                    
                    await Task.Delay(1000);
                    
                    HttpResponseMessage resp = await client.GetAsync(url);
                    if (!resp.IsSuccessStatusCode)
                    {
                        string errorContent = await resp.Content.ReadAsStringAsync();
                        Console.WriteLine($"HTTP ошибка {(int)resp.StatusCode}: {resp.ReasonPhrase}");
                        Console.WriteLine($"Ответ: {errorContent}");
                        
                        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            Console.WriteLine("Слишком много запросов");
                            await Task.Delay(5000);
                        }
                        continue;
                    }
                    string response = await resp.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(response);

                    var items = data["items"];
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var idToken = item?["id"];
                            if (idToken != null)
                            {
                                string vacancyId = idToken.ToString();
                                totalVacancies++;
                                await ParseVacancy(vacancyId, skillFrequency);
                            }
                        }
                    }
                }
            }

            var competencies = new List<CompetencyResult>();
            foreach (var skill in skillFrequency.OrderByDescending(kv => kv.Value).Take(20))
            {
                competencies.Add(new CompetencyResult
                {
                    Name = skill.Key,
                    Source = "hh.ru",
                    Description = "Описание отсутствует",
                    FrequencyPercent = totalVacancies > 0 ? (skill.Value * 100.0 / totalVacancies) : 0
                });
            }

            Console.WriteLine("\nТаблица компетенций:");
            Console.WriteLine("{0,-25} | {1,-15} | {2,-30} | {3,10}", "Компетенция", "Источник", "Описание", "Частота (%)");
            Console.WriteLine(new string('-', 90));
            foreach (var comp in competencies)
            {
                Console.WriteLine("{0,-25} | {1,-15} | {2,-30} | {3,10:F2}", comp.Name, comp.Source, comp.Description, comp.FrequencyPercent);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка HTTP: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static async Task ParseVacancy(string vacancyId, Dictionary<string, int> skillFrequency)
    {
        try
        {
            string url = $"https://api.hh.ru/vacancies/{vacancyId}";
            
            await Task.Delay(500);
            
            string response = await client.GetStringAsync(url);
            JObject vacancy = JObject.Parse(response);

            var keySkills = vacancy["key_skills"];
            if (keySkills != null)
            {
                foreach (var skill in keySkills)
                {
                    var nameToken = skill?["name"];
                    if (nameToken != null)
                    {
                        string name = nameToken.ToString();
                        if (skillFrequency.ContainsKey(name))
                            skillFrequency[name]++;
                        else
                            skillFrequency[name] = 1;
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при получении вакансии {vacancyId}: {ex.Message}");
        }
    }
}
