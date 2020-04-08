using System.IO;

namespace TextExtractor
{
    /// <summary>
    /// Интерфейс извлечения текста из файла
    /// </summary>
    public interface ITextExtractor
    {
        /// <summary>
        /// Возвращает текст из файлового потока
        /// </summary>
        string ExtractTextFromFileStream(Stream fileStream);
    }
}
