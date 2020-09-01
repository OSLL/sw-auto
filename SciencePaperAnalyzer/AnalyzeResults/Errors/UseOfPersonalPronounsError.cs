using AnalyzeResults.Presentation;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using AnalyzeResults.Settings;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class UseOfPersonalPronounsError : Error
    {
        public UseOfPersonalPronounsError(Word errorWord, double errorCost, double weight, List<ScopePair> grading, GradingType gType)
            : base(ErrorType.UseOfPersonalPronouns, "Использование личного местоимения", errorWord.Original,
                  "Использование личных местоимений запрещено. Проверьте, можно ли удалить это местоимение без потери смысла.", errorCost, weight, grading, gType)
        {
            ErrorWord = errorWord;
        }

        [BsonElement("errorword")]
        public Word ErrorWord { get; }
    }
}
