namespace AnalyzeResults.Presentation
{
    /// <summary>
    /// Метрики процесса анализа 
    /// </summary>
    public class PaperAnalysisProcessingMetrics
    {
        /// <summary>
        /// Время извлечения текста из исходного файла в миллисекундах
        /// </summary>
        public long TextExtractionTime { get; set; }

        /// <summary>
        /// Время анализа в миллисекундах (не включает время извлечение текста)
        /// </summary>
        public long AnalyzingTime { get; set; }

        /// <summary>
        /// Длина извлеченного из файла текста
        /// </summary>
        public long StrLength { get; set; }
    }
}
