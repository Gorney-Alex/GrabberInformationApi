using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

public class FgosParser
{
    private readonly HttpClient client;

    public FgosParser()
    {
        client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "MyParser/1.0 (email@example.com)");
    }

    public async Task<string> DownloadPdfAsync(string url)
    {
        var bytes = await client.GetByteArrayAsync(url);
        var tempFile = Path.Combine(Path.GetTempPath(), $"fgos_{Guid.NewGuid()}.pdf");
        await File.WriteAllBytesAsync(tempFile, bytes);
        return tempFile;
    }

    public string ExtractText(string pdfPath)
    {
        using var pdf = PdfDocument.Open(pdfPath);
        string text = "";
        foreach (Page page in pdf.GetPages())
        {
            text += page.Text + "\n";
        }
        return text;
    }

    public List<FgosCompetency> ParseCompetencies(string text, string direction)
    {
        var competencies = new List<FgosCompetency>();
        var pattern = @"(УК-\d+|ОПК-\d+|ПК-\d+)\.?\s*([^.]+)\.";
        var matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
        {
            competencies.Add(new FgosCompetency
            {
                Code = match.Groups[1].Value.Trim(),
                Description = match.Groups[2].Value.Trim(),
                Type = GetType(match.Groups[1].Value),
                Direction = direction
            });
        }
        return competencies;
    }

    public List<FgosIndicator> ParseIndicators(string text, string direction)
    {
        var indicators = new List<FgosIndicator>();
        var pattern = @"(УК-\d+\.\d+|ОПК-\d+\.\d+|ПК-\d+\.\d+)\.?\s*([^.]+)\.";
        var matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
        {
            var indicatorCode = match.Groups[1].Value.Trim();
            var baseCode = indicatorCode.Split('.')[0];
            indicators.Add(new FgosIndicator
            {
                IndicatorCode = indicatorCode,
                CompetencyCode = baseCode,
                Description = match.Groups[2].Value.Trim(),
                Direction = direction
            });
        }
        return indicators;
    }

    private string GetType(string code)
    {
        if (code.StartsWith("УК")) return "Универсальная";
        if (code.StartsWith("ОПК")) return "Общепрофессиональная";
        if (code.StartsWith("ПК")) return "Профессиональная";
        return "Неопределенная";
    }
}
