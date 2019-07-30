namespace LangAnalyzerStd.Сrfsuite
{
    /// <summary>
    /// Аттрибут линейного CRF алгоритма
    /// </summary>
    public sealed class CRFAttribute
    {
        public CRFAttribute(char attributeName, int position, int columnIndex)
        {
            AttributeName = attributeName;
            Position = position;
            ColumnIndex = columnIndex;
        }

        /// <summary>
        /// Название аттрибута
        /// </summary>
        public readonly char AttributeName;

        /// <summary>
        /// Индекс позиции аттрибута
        /// </summary>
        public readonly int Position;

        /// <summary>
        /// Индекс столбца
        /// </summary>
        public readonly int ColumnIndex;

        public override string ToString()
        {
            return '[' + AttributeName + ":" + Position + "], position: " + Position + ", column-index: " + ColumnIndex;
        }
    };
}