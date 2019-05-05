namespace AnalyzeResults.Errors
{
    public class SourceNotReferencedError : Error
    {
        public SourceNotReferencedError(int number)
            : base(ErrorType.SourceNotReferenced, "Использование личного местоимения", $"Источник №{number}",
                  "Необходимо хотя бы раз сослаться на каждый из перечисленных источников.")
        {
            Number = number;
        }

        public int Number { get; set; }
    }
}
