﻿using AnalyzeResults.Presentation;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class UseOfForbiddenWordsError : Error
    {
        public UseOfForbiddenWordsError(string dictionaryName, Word errorWord, double errorCost, double weight)
            : base(ErrorType.UseOfForbiddenWord, "Использование запрещенного слова", errorWord.Original,
                  $"Данное слово включено в список запрещенных слов.",errorCost, weight)
        {
            ErrorWord = errorWord;
        }

        [BsonElement("errorword")]
        public Word ErrorWord { get; }
    }
}