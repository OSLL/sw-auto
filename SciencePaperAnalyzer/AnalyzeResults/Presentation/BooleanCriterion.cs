using System;
using AnalyzeResults.Helpers;
using MongoDB.Bson.Serialization.Attributes;

namespace AnalyzeResults.Presentation
{
    [Serializable]
    public class BooleanCriterion : Criterion
    {
        private string _advice;

        public BooleanCriterion(string name, bool value, double factor, string description = "",
            string advice = "",
            IntervalType lowerBoundType = IntervalType.Closed, IntervalType upperBoundType = IntervalType.Closed)
            : base(name, CriterionType.Boolean, description, factor)
        {
            Value = value;
            _advice = advice;
        }

        [BsonElement("value")]
        public bool Value { get; set; }

        public override string GetStringRequirements()
        {
            return $"Критерий должен быть удовлетворен";
        }

        public override string GetStringValue()
        {
            return Value? "Выполнен" : "Не выполнен";
        }

        public override bool IsMet()
        {
            return Value;
        }

        public override double GetGradePart()
        {
            return Factor * (IsMet() ? 1 : 0);
        }

        public override string GetAdvice()
        {
            if (IsMet())
                return string.Empty;

            return _advice;
        }
    }
}
