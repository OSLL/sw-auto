using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AnalyzeResults.Presentation
{
    [Serializable]
    public abstract class Criterion
    {
        public Criterion(string name, CriterionType type, string description = "")
        {
            Name = name;
            Type = type;
            Description = description;
        }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("type")]
        public CriterionType Type { get; set; }

        public abstract bool IsMet();

        public abstract string GetStringValue();

        public abstract string GetStringRequirements();

        public abstract double GetGradePart();

        public abstract string GetAdvice();
    }
}
