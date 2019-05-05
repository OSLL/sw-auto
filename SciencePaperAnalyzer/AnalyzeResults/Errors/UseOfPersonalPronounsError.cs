using AnalyzeResults.Presentation;

namespace AnalyzeResults.Errors
{
    public class UseOfPersonalPronounsError : Error
    {
        public UseOfPersonalPronounsError(Word errorWord)
            : base(ErrorType.UseOfPersonalPronouns, "Использование личного местоимения", errorWord.Original,
                  "Использование личных местоимений запрещено. Проверьте, можно ли удалить это местоимение без потери смысла.")
        {
            ErrorWord = errorWord;
        }

        public Word ErrorWord { get; }
    }
}
