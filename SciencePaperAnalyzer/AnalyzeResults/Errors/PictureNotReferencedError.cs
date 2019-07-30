namespace AnalyzeResults.Errors
{
    public class PictureNotReferencedError : Error
    {
        public PictureNotReferencedError(int number)
            : base(ErrorType.PictureNotReferenced, "Нет ссылки на рисунок", $"Рисунок №{number}",
                  "Необходимо хотя бы раз сослаться на каждый рисунок в правильном формате: рис. N")
        {
            Number = number;
        }

        public int Number { get; set; }
    }
}
