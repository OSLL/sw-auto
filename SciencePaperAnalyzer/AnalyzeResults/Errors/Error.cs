using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using AnalyzeResults.Settings;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public abstract class Error
    {
        public Error(ErrorType type, string name, string explanation, string tip, double errorCost, double weight,
                     List<ScopePair> grading, GradingType gType)
        {
            ErrorType = type;
            Name = name;
            Explanation = explanation;
            Tip = tip;
            ErrorCost = errorCost;
            Weight = weight;
            Grading = grading;
            GradingType = gType;
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

        [BsonElement("grading")]
        public List<ScopePair> Grading { get; }

        [BsonElement("gradingtype")]
        public GradingType GradingType { get; }
    }
}
