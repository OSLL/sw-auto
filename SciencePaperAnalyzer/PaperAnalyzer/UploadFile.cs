using System.IO;

namespace PaperAnalyzer
{
    /// <summary>
    /// Представление загруженного файла в слое логики
    /// </summary>
    public class UploadFile
    {
        /// <summary>
        /// Название файла с расширением
        /// </summary>
        /// <example>"paper.pdf"</example>
        public string FileName { get; set; }

        /// <summary>
        /// Стрим с бинарными данными из файла
        /// </summary>
        public Stream DataStream { get; set; }

        /// <summary>
        /// Длина файла
        /// </summary>
        public long Length { get; set; }
    }
}
