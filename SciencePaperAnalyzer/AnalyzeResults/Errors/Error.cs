using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public abstract class Error
    {
        public Error(ErrorType type, string name, string explanation, string tip)
        {
            ErrorType = type;
            Name = name;
            Explanation = explanation;
            Tip = tip;
        }

        [BsonElement("name")]
        public string Name { get; }

        [BsonElement("explanation")]
        public string Explanation { get; }

        [BsonElement("tip")]
        public string Tip { get; }

        [BsonElement("errortype")]
        public ErrorType ErrorType { get; }
    }
}
