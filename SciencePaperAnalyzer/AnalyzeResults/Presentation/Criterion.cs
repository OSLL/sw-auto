using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AnalyzeResults.Presentation
{
    [Serializable]
    [BsonKnownTypes(typeof(NumericalCriterion), typeof(BooleanCriterion))]
    public abstract class Criterion
    {
        public Criterion(string name, CriterionType type, string description = "", double factor = 0, bool isPlaceholder = false)
        {
            Name = name;
            Type = type;
            Description = description;
            Factor = factor;
            IsPlaceholder = isPlaceholder;
        }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("type")]
        public CriterionType Type { get; set; }

        [BsonElement("factor")]
        public double Factor { get; set; }

        [BsonElement("isPlaceholder")]
        public bool IsPlaceholder { get; set; }

        public abstract bool IsMet();

        public abstract string GetStringValue();

        public abstract string GetStringRequirements();

        public abstract double GetGradePart();

        public abstract string GetAdvice();
    }
}
