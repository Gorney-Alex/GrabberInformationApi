using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

public class ProfStandardParser
{
    private readonly HttpClient client;

    public ProfStandardParser()
    {
        client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "MyParser/1.0 (email@example.com)");
    }

    public async Task<string> DownloadHtmlAsync(string url)
    {
        return await client.GetStringAsync(url);
    }

    public List<ProfStandardFunction> ExtractFunctions(string html, string standardName)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var functions = new List<ProfStandardFunction>();

        var nodes = doc.DocumentNode.SelectNodes("//*[contains(text(), 'Обобщенн')]");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var next = node.SelectSingleNode("following-sibling::*[1]");
                if (next != null)
                {
                    foreach (var li in next.SelectNodes(".//li|.//p"))
                    {
                        string text = li.InnerText.Trim();
                        if (text.Length > 20)
                        {
                            functions.Add(new ProfStandardFunction
                            {
                                Code = ExtractCode(text),
                                Description = text,
                                StandardName = standardName
                            });
                        }
                    }
                }
            }
        }

        return functions;
    }

    public List<ProfStandardAction> ExtractActions(string html, string standardName)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var actions = new List<ProfStandardAction>();

        var nodes = doc.DocumentNode.SelectNodes("//*[contains(text(), 'трудов')]");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var next = node.SelectSingleNode("following-sibling::*[1]");
                if (next != null)
                {
                    foreach (var li in next.SelectNodes(".//li|.//p"))
                    {
                        string text = li.InnerText.Trim();
                        if (text.Length > 15)
                        {
                            actions.Add(new ProfStandardAction
                            {
                                Code = ExtractCode(text),
                                Description = text,
                                StandardName = standardName
                            });
                        }
                    }
                }
            }
        }

        return actions;
    }

    private string ExtractCode(string text)
    {
        var match = Regex.Match(text, @"[A-Z]/\d+\.\d+");
        return match.Success ? match.Value : "Не указан";
    }
}
