using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using AnalyzeResults.Settings;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class TableNotReferencedError : Error
    {
        public TableNotReferencedError(int number, double errorCost, double weight, Dictionary<int, double> grading, GradingType gType)
            : base(ErrorType.TableNotReferenced, "Нет ссылки на таблицу", $"Таблица №{number}",
                  "Необходимо хотя бы раз сослаться на каждую таблицу в правильном формате: табл. N", errorCost, weight, grading, gType)
        {
            Number = number;
        }

        [BsonElement("number")]
        public int Number { get; set; }
    }
}
