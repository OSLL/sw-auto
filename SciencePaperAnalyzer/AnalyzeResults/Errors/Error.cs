using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public abstract class Error
    {
        public Error(ErrorType type, string name, string explanation, string tip, double errorCost, double weight)
        {
            ErrorType = type;
            Name = name;
            Explanation = explanation;
            Tip = tip;
            ErrorCost = errorCost;
            Weight = weight;
        }

        [BsonElement("name")]
        public string Name { get; }

        [BsonElement("explanation")]
        public string Explanation { get; }

        [BsonElement("tip")]
        public string Tip { get; }

        [BsonElement("errortype")]
        public ErrorType ErrorType { get; }

        [BsonElement("errorcost")]
        public double ErrorCost { get; }

        [BsonElement("weight")]
        public double Weight { get; }
    }
}
