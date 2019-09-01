using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class SourceNotReferencedError : Error
    {
        public SourceNotReferencedError(int number)
            : base(ErrorType.SourceNotReferenced, "Нет ссылки на источник", $"Источник №{number}",
                  "Необходимо хотя бы раз сослаться на каждый из перечисленных источников.")
        {
            Number = number;
        }

        [BsonElement("number")]
        public int Number { get; set; }
    }
}
