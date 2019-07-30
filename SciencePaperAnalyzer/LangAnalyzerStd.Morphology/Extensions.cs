using System;
using System.Collections.Generic;

namespace LangAnalyzerStd.Core
{
    internal static class Extensions
    {
        public static void ThrowIfNull(this object obj, string paramName)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }
        public static void ThrowIfNullOrWhiteSpace(this string text, string paramName)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(paramName);
        }
        
        public static void ThrowIfNullOrWhiteSpaceAnyElement(this IEnumerable<string> sequence, string paramName)
        {
            if (sequence == null)
                throw new ArgumentNullException(paramName);

            foreach (var c in sequence)
            {
                if (string.IsNullOrWhiteSpace(c))
                    throw new ArgumentNullException($"{paramName} => some collection element is NULL-or-WhiteSpace");
            }
        }
    }
}
