using AnalyzeResults.Presentation;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class UseOfPersonalPronounsError : Error
    {
        public UseOfPersonalPronounsError(Word errorWord)
            : base(ErrorType.UseOfPersonalPronouns, "Использование личного местоимения", errorWord.Original,
                  "Использование личных местоимений запрещено. Проверьте, можно ли удалить это местоимение без потери смысла.")
        {
            ErrorWord = errorWord;
        }

        [BsonElement("errorword")]
        public Word ErrorWord { get; }
    }
}
