using CompetencyParser.Models;

namespace CompetencyParser.Parsers
{
    /// <summary>
    /// Базовый интерфейс для всех парсеров
    /// </summary>
    /// <typeparam name="T">Тип данных, которые возвращает парсер</typeparam>
    public interface IParser<T>
    {
        /// <summary>
        /// Парсинг данных из источника
        /// </summary>
        /// <returns>Список распарсенных данных</returns>
        Task<List<T>> ParseAsync();

        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        /// <param name="data">Данные для сохранения</param>
        /// <param name="filePath">Путь к файлу</param>
        Task SaveToFileAsync(List<T> data, string filePath);
    }
}
