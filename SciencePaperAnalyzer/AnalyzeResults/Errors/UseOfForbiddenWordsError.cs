using AnalyzeResults.Presentation;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using AnalyzeResults.Settings;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class UseOfForbiddenWordsError : Error
    {
        public UseOfForbiddenWordsError(string dictionaryName, Word errorWord, double errorCost, double weight, List<ScopePair> grading, GradingType gType)
            : base(ErrorType.UseOfForbiddenWord, "Использование запрещенного слова", errorWord?.Original,
                  $"Данное слово включено в список запрещенных слов.",errorCost, weight, grading, gType)
        {
            ErrorWord = errorWord;
            DictionaryName = dictionaryName;
        }

        [BsonElement("errorword")]
        public Word ErrorWord { get; }

        [BsonElement("dictionaryname")]
        public string DictionaryName { get; }
    }
}
