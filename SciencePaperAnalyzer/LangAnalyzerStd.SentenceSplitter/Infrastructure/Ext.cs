using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LangAnalyzerStd.SentenceSplitter
{
    internal static class Extensions
    {
        public static HashSet<string> ToHashset(this IEnumerable<string> seq, bool toUpperInvariant)
        {
            var hs = new HashSet<string>(seq.Select(d => d.TrimEx(toUpperInvariant)).Where(d => !string.IsNullOrEmpty(d)));
            return hs;
        }
        public static HashSet<string> ToHashsetWithReverseValues(this IEnumerable<string> seq, bool toUpperInvariant)
        {
            var hs = new HashSet<string>(seq.Select(d => (d != null) ? new string(d.Trim().Reverse().ToArray()).ToUpperInvariantEx(toUpperInvariant) : null).Where(d => !string.IsNullOrEmpty(d)));
            return hs;
        }
        public static int GetItemMaxLength(this HashSet<string> hs)
        {
            return (hs.Count != 0) ? hs.Max(d => d.Length) : 0;
        }

        public static Dictionary<string, T> ToDictionary<T>(this IEnumerable<KeyValuePair<string, T>> seq, bool toUpperInvariant)
        {
            var dict = new Dictionary<string, T>();
            foreach (var pair in seq)
            {
                var key = pair.Key.TrimEx(toUpperInvariant);
                if (string.IsNullOrEmpty(key))
                    continue;

                if (dict.ContainsKey(key))
                    continue;

                dict.Add(key, pair.Value);
            }
            return dict;
        }
        public static int GetItemMaxKeyLength<T>(this Dictionary<string, T> dict)
        {
            return (dict.Count != 0) ? dict.Max(p => p.Key.Length) : 0;
        }
        public static int GetItemMinKeyLength<T>(this Dictionary<string, T> dict)
        {
            return (dict.Count != 0) ? dict.Min(p => p.Key.Length) : 0;
        }

        public static string TrimStartDot(this string value)
        {
            return value.TrimStart('.');
        }
        public static string TrimEndDot(this string value)
        {
            return value.TrimEnd('.');
        }

        public static bool AttrValueIsTrue(this XElement xe, string attrName)
        {
            var xa = xe.Attribute(attrName);
            if (xa != null)
            {
                if (bool.TryParse(xa.Value, out bool r))
                    return r;
            }
            return false;
        }

        private static string TrimEx(this string value, bool toUpperInvarian)
        {
            if (value == null)
                return null;
            return toUpperInvarian ? value.ToUpperInvariant() : value;
        }
        private static string ToUpperInvariantEx(this string value, bool toUpperInvarian)
        {
            return toUpperInvarian ? value.ToUpperInvariant() : value;
        }

        public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> t)
        {
            return t.SelectMany(_ => _);
        }

        private const char DOT = '.';
        private static readonly char[] SPLIT_BY_DOT = new[] { DOT };
        private static readonly char[] SPLIT_BY_SPACES = new[] { ' ', '\t', '\r', '\n' };
        public static NGram<BeforeNoProper> ToBeforeNoProper_ngrams(this XElement xe)
        {
            var words = xe.GetWordsArray();
            var unstick_from_digits = xe.AttrValueIsTrue("unstick-from-digits");

            var ngram = new NGram<BeforeNoProper>(words, new BeforeNoProper(unstick_from_digits));
            return ngram;
        }
        public static NGram<BeforeProperOrNumber> ToBeforeProperOrNumber_ngrams(this XElement xe)
        {
            var words = xe.GetWordsArray();
            var digits_before = xe.AttrValueIsTrue("digits-before");
            var slash_before = xe.AttrValueIsTrue("slash-before");
            var unstick_from_digits = xe.AttrValueIsTrue("unstick-from-digits");

            var ngram = new NGram<BeforeProperOrNumber>(words, new BeforeProperOrNumber(digits_before, slash_before, unstick_from_digits));
            return ngram;
        }

        private static string[] GetWordsArray(this XElement xe)
        {
            var words = xe.Value.Split(SPLIT_BY_DOT, StringSplitOptions.RemoveEmptyEntries);
            var word_list = new List<string>(words.Length);
            for (int i = 0, len = words.Length - 1; i <= len; i++)
            {
                var word = words[i].Trim();
                var words_by_space = word.Split(SPLIT_BY_SPACES, StringSplitOptions.RemoveEmptyEntries);
                if (words_by_space.Length == 1)
                {
                    word_list.Add(word + DOT);
                }
                else
                {
                    for (int j = 0, len_by_space = words_by_space.Length - 1; j <= len_by_space; j++)
                    {
                        word = words_by_space[j];
                        if (j == len_by_space)
                        {
                            word_list.Add(word + DOT);
                        }
                        else
                        {
                            word_list.Add(word);
                        }
                    }
                }
            }
            return word_list.ToArray();
        }
    }
}
