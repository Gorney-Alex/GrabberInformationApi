using CompetencyParser.Models;

namespace CompetencyParser.Parsers
{
    public interface IParser<T>
    {
        Task<List<T>> ParseAsync();

        Task SaveToFileAsync(List<T> data, string filePath);
    }
}
