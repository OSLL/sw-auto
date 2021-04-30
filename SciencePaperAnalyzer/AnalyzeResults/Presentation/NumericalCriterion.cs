using System;
using AnalyzeResults.Helpers;
using MongoDB.Bson.Serialization.Attributes;

namespace AnalyzeResults.Presentation
{
    [Serializable]
    public class NumericalCriterion : Criterion
    {

        public NumericalCriterion(string name, double value, double lowerBound, double upperBound, double factor, string description = "",
            string adviceToLower = "", string adviceToRaise = "",
            IntervalType lowerBoundType = IntervalType.Closed, IntervalType upperBoundType = IntervalType.Closed, bool isPlaceholder = false)
            : base(name, CriterionType.Numerical, description, factor, isPlaceholder)
        {
            Interval = new Interval<double>(lowerBound, upperBound, lowerBoundType, upperBoundType);
            Value = value;
            AdviceToLower = adviceToLower;
            AdviceToRaise = adviceToRaise;
        }

        [BsonElement("interval")]
        public Interval<double> Interval { get; set; }

        [BsonElement("value")]
        public double Value { get; set; }

        [BsonElement("adviceToLower")]
        private string AdviceToLower { get; set; }
        [BsonElement("adviceToHigher")]
        private string AdviceToRaise { get; set; }

        public override string GetStringRequirements()
        {
            return $"Значение критерия должно находиться в интервале {Interval.ToString()}";
        }

        public override string GetStringValue()
        {
            return string.Format("{0:0.000}", Value);
        }

        public override bool IsMet()
        {
            return Interval.Contains(Value);
        }

        public override double GetGradePart()
        {
            return IsPlaceholder?0:( Factor * (IsMet() ? 1 : 0));
        }

        public override string GetAdvice()
        {
            if (IsMet())
                return string.Empty;

            return Value > Interval.UpperBound ? AdviceToLower : AdviceToRaise;
        }
    }
}
