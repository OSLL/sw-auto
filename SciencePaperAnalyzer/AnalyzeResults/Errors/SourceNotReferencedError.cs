using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using AnalyzeResults.Settings;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class SourceNotReferencedError : Error
    {
        public SourceNotReferencedError(int number, double errorCost, double weight, List<ScopePair> grading, GradingType gType)
            : base(ErrorType.SourceNotReferenced, "Нет ссылки на источник", $"Источник №{number}",
                  "Необходимо хотя бы раз сослаться на каждый из перечисленных источников.", errorCost, weight, grading, gType)
        {
            Number = number;
        }

        [BsonElement("number")]
        public int Number { get; set; }
    }
}
