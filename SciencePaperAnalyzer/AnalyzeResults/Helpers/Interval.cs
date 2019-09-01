using System;

namespace AnalyzeResults.Helpers
{
    [Serializable]
    public struct Interval<T> where T : struct, IComparable
    {
        public T LowerBound { get; private set; }
        public T UpperBound { get; private set; }

        public IntervalType LowerBoundIntervalType { get; private set; }
        public IntervalType UpperBoundIntervalType { get; private set; }

        public Interval(
            T lowerbound,
            T upperbound,
            IntervalType lowerboundIntervalType = IntervalType.Closed,
            IntervalType upperboundIntervalType = IntervalType.Closed)
            : this()
        {
            var a = lowerbound;
            var b = upperbound;
            var comparison = a.CompareTo(b);

            if (comparison > 0)
            {
                a = upperbound;
                b = lowerbound;
            }

            LowerBound = a;
            UpperBound = b;
            LowerBoundIntervalType = lowerboundIntervalType;
            UpperBoundIntervalType = upperboundIntervalType;
        }

        public bool Contains(T point)
        {
            if (LowerBound.GetType() != typeof(T)
                || UpperBound.GetType() != typeof(T))
            {
                throw new ArgumentException("Type mismatch", "point");
            }

            var lower = LowerBoundIntervalType == IntervalType.Open
                ? LowerBound.CompareTo(point) < 0
                : LowerBound.CompareTo(point) <= 0;
            var upper = UpperBoundIntervalType == IntervalType.Open
                ? UpperBound.CompareTo(point) > 0
                : UpperBound.CompareTo(point) >= 0;

            return lower && upper;
        }

        public override string ToString()
        {
            return string.Format(
                "{0}{1}, {2}{3}",
                LowerBoundIntervalType == IntervalType.Open ? "(" : "[",
                LowerBound,
                UpperBound,
                UpperBoundIntervalType == IntervalType.Open ? ")" : "]"
            );
        }
    }

    public static class Interval
    {
        public static Interval<double> Range(double lowerbound, double upperbound, IntervalType lowerboundIntervalType = IntervalType.Closed, IntervalType upperboundIntervalType = IntervalType.Closed)
        {
            return new Interval<double>(lowerbound, upperbound, lowerboundIntervalType, upperboundIntervalType);
        }

        public static Interval<decimal> Range(decimal lowerbound, decimal upperbound, IntervalType lowerboundIntervalType = IntervalType.Closed, IntervalType upperboundIntervalType = IntervalType.Closed)
        {
            return new Interval<decimal>(lowerbound, upperbound, lowerboundIntervalType, upperboundIntervalType);
        }

        public static Interval<int> Range(int lowerbound, int upperbound, IntervalType lowerboundIntervalType = IntervalType.Closed, IntervalType upperboundIntervalType = IntervalType.Closed)
        {
            return new Interval<int>(lowerbound, upperbound, lowerboundIntervalType, upperboundIntervalType);
        }
    }

    public enum IntervalType
    {
        Open,
        Closed
    }
}
