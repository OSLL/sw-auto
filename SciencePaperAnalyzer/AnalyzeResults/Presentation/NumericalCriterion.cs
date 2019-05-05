using System;
using System.Collections.Generic;
using System.Text;
using AnalyzeResults.Helpers;

namespace AnalyzeResults.Presentation
{
    public class NumericalCriterion<T> : Criterion where T : struct, IComparable
    {
        public NumericalCriterion(string name, T value, T lowerBound, T upperBound, string description = "",
            IntervalType lowerBoundType = IntervalType.Closed, IntervalType upperBoundType = IntervalType.Closed)
            : base(name, CriterionType.Numerical, description)
        {
            Interval = new Interval<T>(lowerBound, upperBound, lowerBoundType, upperBoundType);
            Value = value;
        }

        public Interval<T> Interval { get; set; }

        public T Value { get; set; }

        public override string GetStringRequirements()
        {
            return $"Значение критерия должно находиться в интервале {Interval.ToString()}";
        }

        public override string GetStringValue()
        {
            return typeof(T) == typeof(double) ? string.Format("{0:0.000}", Value) : Value.ToString();
        }

        public override bool IsMet()
        {
            return Interval.Contains(Value);
        }
    }
}
