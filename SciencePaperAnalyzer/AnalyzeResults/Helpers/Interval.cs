using System;
using System.Runtime.Serialization;

namespace AnalyzeResults.Helpers
{
    [Serializable]
    public class Interval<T> where T : struct, IComparable
    {
        public T LowerBound { get; set; }
        public T UpperBound { get; set; }

        public IntervalType LowerBoundIntervalType { get; set; }
        public IntervalType UpperBoundIntervalType { get; set; }
        
        public Interval(
            T lowerBound,
            T upperBound,
            IntervalType lowerBoundIntervalType = IntervalType.Closed,
            IntervalType upperBoundIntervalType = IntervalType.Closed)
        {
            var a = lowerBound;
            var b = upperBound;
            var comparison = a.CompareTo(b);

            if (comparison > 0)
            {
                a = lowerBound;
                b = upperBound;
            }

            LowerBound = a;
            UpperBound = b;
            LowerBoundIntervalType = lowerBoundIntervalType;
            UpperBoundIntervalType = upperBoundIntervalType;
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
        //public Interval(SerializationInfo info, StreamingContext context)
        //{
        //    Console.WriteLine("called");
        //}
        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    Console.WriteLine("called");
        //}
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
