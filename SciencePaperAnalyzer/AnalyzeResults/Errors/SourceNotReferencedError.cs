using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class SourceNotReferencedError : Error
    {
        public SourceNotReferencedError(int number, double errorCost, double weight)
            : base(ErrorType.SourceNotReferenced, "Нет ссылки на источник", $"Источник №{number}",
                  "Необходимо хотя бы раз сослаться на каждый из перечисленных источников.", errorCost, weight)
        {
            Number = number;
        }

        [BsonElement("number")]
        public int Number { get; set; }
    }
}
