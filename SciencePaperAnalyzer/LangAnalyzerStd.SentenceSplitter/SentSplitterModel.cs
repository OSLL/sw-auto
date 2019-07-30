using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

using LangAnalyzerStd.Core;
using LangAnalyzerStd.Urls;

namespace LangAnalyzerStd.SentenceSplitter
{
    internal struct BeforeNoProper
    {
        public BeforeNoProper(bool unstickFromDigits)
        {
            UnstickFromDigits = unstickFromDigits;
        }

        public bool UnstickFromDigits;

        public override string ToString()
        {
            if (UnstickFromDigits)
                return ("unstick-from-digits: " + UnstickFromDigits);
            return string.Empty;
        }
    }

    internal struct BeforeProperOrNumber
    {
        public BeforeProperOrNumber(
            bool digitsBefore,
            bool slashBefore,
            bool unstickFromDigits) : this()
        {
            DigitsBefore = digitsBefore;
            SlashBefore = slashBefore;
            UnstickFromDigits = unstickFromDigits;

            DigitsBeforeOrSlashBefore = DigitsBefore | SlashBefore;
        }

        public bool DigitsBefore;
        public bool SlashBefore;
        public bool UnstickFromDigits;

        public bool DigitsBeforeOrSlashBefore
        {
            get;
            private set;
        }

        public override string ToString()
        {
            var v = default(string);
            if (DigitsBefore)
            {
                v = $"digits-before: {DigitsBefore}";
            }
            if (SlashBefore)
            {
                if (v != null) v += ", ";
                v += $"slash-before: {SlashBefore}";
            }
            if (UnstickFromDigits)
            {
                if (v != null) v += ", ";
                v += $"unstick-from-digits: {UnstickFromDigits}";
            }
            return v;
        }
    }

    internal struct Smile
    {
        public Smile(bool spaceBefore)
        {
            SpaceBefore = spaceBefore;
        }

        public bool SpaceBefore;

        public override string ToString()
        {
            if (SpaceBefore)
                return $"space-before: {SpaceBefore}";
            return string.Empty;
        }
    }

    unsafe public class SentSplitterModel : IDisposable
    {
        internal struct HashSet
        {
            public HashSet(HashSet<string> values) : this()
            {
                Values = values;
                ValuesMaxLength = Values.GetItemMaxLength();
            }

            public HashSet<string> Values
            {
                get;
                private set;
            }
            public int ValuesMaxLength
            {
                get;
                private set;
            }
        }

        internal struct Dictionary<TValue>
        {
            public Dictionary(Dictionary<string, TValue> values) : this()
            {
                Values = values;
                ValuesMaxLength = Values.GetItemMaxKeyLength();
                ValuesMinLength = Values.GetItemMinKeyLength();
                DiffBetweenMixAndMaxLength = (ValuesMaxLength - ValuesMinLength);
            }

            public Dictionary<string, TValue> Values
            {
                get;
                private set;
            }
            public int ValuesMaxLength
            {
                get;
                private set;
            }
            public int ValuesMinLength
            {
                get;
                private set;
            }
            public int DiffBetweenMixAndMaxLength
            {
                get;
                private set;
            }
        }

        [Flags]
        internal enum SentCharType : byte
        {
            __UNDEFINE__ = 0x0,

            Unconditional = 0x1,
            SmileBegin = 1 << 1,
            ExcludeInBracketAndQuote = 1 << 2,
            Dot = 1 << 3,
            ThreeDot = 1 << 4,
            RomanDigit = 1 << 5,
            AfterThreeDotAllowedPunctuation = 1 << 6,
            AfterBracketAllowedPunctuation4QMEP = 1 << 7
        }

        internal const string YANDEX_RU = "ЯНДЕКС.";
        internal const string YANDEX_EN = "YANDEX.";

        public SentSplitterModel(string sentSplitterResourcesXmlFilename)
        {
            var xdoc = XDocument.Load(sentSplitterResourcesXmlFilename);

            //-smiles-
            var smiles = from xe in xdoc.Root.Element("smiles").Elements()
                         select new KeyValuePair<string, Smile>(xe.Value.Trim(), new Smile(xe.AttrValueIsTrue("space-before")));
            Smiles = new Dictionary<Smile>(smiles.ToDictionary(false));

            //-interjections-
            var interjections = from xe in xdoc.Root.Element("interjections").Elements()
                                select xe.Value.Trim().TrimEndDot();
            Interjections = new HashSet(interjections.ToHashset(true));

            //-yandex-combinations-
            var yandexCombinations = from xe in xdoc.Root.Element("yandex-combinations").Elements()
                                     select xe.Value.Trim().TrimStartDot();
            YandexCombinations = new HashSet(yandexCombinations.ToHashset(true));

            //-file-extensions-
            var fileExtensions = from xe in xdoc.Root.Element("file-extensions").Elements()
                                 select xe.Value.Trim().TrimStartDot();
            FileExtensions = new HashSet(fileExtensions.ToHashset(true));

            //-before-no-proper-
            var beforeNoProper = (from xe in xdoc.Root.Element("before-no-proper").Elements()
                                  select xe.ToBeforeNoProper_ngrams()).ToArray();
            BeforeNoProperSearcher = new AhoCorasick<BeforeNoProper>(beforeNoProper);

            //-before-proper-or-number-
            var beforeProperOrNumber = (from xe in xdoc.Root.Element("before-proper-or-number").Elements()
                                        select xe.ToBeforeProperOrNumber_ngrams()).ToArray();
            BeforeProperOrNumberSearcher = new AhoCorasick<BeforeProperOrNumber>(beforeProperOrNumber);

            var SENTCHARTYPE_MAP = InitializeSentPotentialEnds(Smiles, beforeNoProper, beforeProperOrNumber);

            //--//
            _SENTCHARTYPE_MAP_GCHandle = GCHandle.Alloc(SENTCHARTYPE_MAP, GCHandleType.Pinned);
            this.SENTCHARTYPE_MAP = (SentCharType*)_SENTCHARTYPE_MAP_GCHandle.AddrOfPinnedObject().ToPointer();

            xdoc = null;
        }

        ~SentSplitterModel()
        {
            DisposeNativeResources();
        }

        public void Dispose()
        {
            DisposeNativeResources();

            GC.SuppressFinalize(this);
        }

        private void DisposeNativeResources()
        {
            if (SENTCHARTYPE_MAP != null)
            {
                _SENTCHARTYPE_MAP_GCHandle.Free();
                SENTCHARTYPE_MAP = null;
            }
        }

        internal Dictionary<Smile> Smiles
        {
            get;
            private set;
        }
        internal HashSet Interjections
        {
            get;
            private set;
        }
        internal HashSet YandexCombinations
        {
            get;
            private set;
        }
        internal HashSet FileExtensions
        {
            get;
            private set;
        }
        internal AhoCorasick<BeforeNoProper> BeforeNoProperSearcher
        {
            get;
            private set;
        }
        internal AhoCorasick<BeforeProperOrNumber> BeforeProperOrNumberSearcher
        {
            get;
            private set;
        }

        internal HashSet<string> UnstickFromDigits
        {
            get;
            private set;
        }

        internal int GetValuesMaxLength()
        {
            var valuesMaxLengths = new[]
            {
                Smiles            .ValuesMaxLength,
                Interjections     .ValuesMaxLength,
                YandexCombinations.ValuesMaxLength,
                FileExtensions    .ValuesMaxLength,
            };
            return valuesMaxLengths.Max();
        }
        internal int GetNgramMaxLength()
        {
            return Math.Max(BeforeNoProperSearcher.NgramMaxLength, BeforeProperOrNumberSearcher.NgramMaxLength);
        }

        private readonly GCHandle _SENTCHARTYPE_MAP_GCHandle;
        internal SentCharType* SENTCHARTYPE_MAP
        {
            get;
            private set;
        }

        private byte[] InitializeSentPotentialEnds(
            Dictionary<Smile> smiles,
            NGram<BeforeNoProper>[] beforeNoProper,
            NGram<BeforeProperOrNumber>[] beforeProperOrNumber)
        {
            var SENTCHARTYPE_MAP = new byte[char.MaxValue + 1];

            //-smile's-
            foreach (var c in smiles.Values.Keys.Select(k => k[0]))
            {
                SENTCHARTYPE_MAP[c] |= (byte)SentCharType.SmileBegin;
            }

            SENTCHARTYPE_MAP['!'] |= (byte)SentCharType.ExcludeInBracketAndQuote;
            SENTCHARTYPE_MAP['?'] |= (byte)SentCharType.ExcludeInBracketAndQuote;
            SENTCHARTYPE_MAP['…'] |= (byte)SentCharType.ExcludeInBracketAndQuote | (byte)SentCharType.ThreeDot;

            //-un-conditional-
            SENTCHARTYPE_MAP['\n'] = (byte)SentCharType.Unconditional;

            //-dot-
            SENTCHARTYPE_MAP['.'] = (byte)SentCharType.Dot;

            //-after ThreeDot allowed punctuation-
            SENTCHARTYPE_MAP[';'] |= (byte)SentCharType.AfterThreeDotAllowedPunctuation;
            SENTCHARTYPE_MAP[':'] |= (byte)SentCharType.AfterThreeDotAllowedPunctuation | (byte)SentCharType.AfterBracketAllowedPunctuation4QMEP;
            SENTCHARTYPE_MAP[','] |= (byte)SentCharType.AfterThreeDotAllowedPunctuation | (byte)SentCharType.AfterBracketAllowedPunctuation4QMEP;
            for (var c = char.MinValue; ; c++)
            {
                var ct = Xlat.CHARTYPE_MAP[c];
                if ((ct & CharType.IsHyphen) == CharType.IsHyphen)
                {
                    SENTCHARTYPE_MAP[c] |= (byte)SentCharType.AfterThreeDotAllowedPunctuation | (byte)SentCharType.AfterBracketAllowedPunctuation4QMEP;
                }
                else
                if ((ct & CharType.IsQuote) == CharType.IsQuote)
                {
                    SENTCHARTYPE_MAP[c] |= (byte)SentCharType.AfterThreeDotAllowedPunctuation;
                }

                if (c == char.MaxValue)
                {
                    break;
                }
            }

            //roman-digit
            SENTCHARTYPE_MAP['I'] |= (byte)SentCharType.RomanDigit;
            SENTCHARTYPE_MAP['V'] |= (byte)SentCharType.RomanDigit;
            SENTCHARTYPE_MAP['X'] |= (byte)SentCharType.RomanDigit;
            SENTCHARTYPE_MAP['C'] |= (byte)SentCharType.RomanDigit;
            SENTCHARTYPE_MAP['L'] |= (byte)SentCharType.RomanDigit;
            SENTCHARTYPE_MAP['M'] |= (byte)SentCharType.RomanDigit;

            foreach (var ngram in beforeProperOrNumber)
            {
                if (ngram.Value.DigitsBefore)
                {
                    if (ngram.Words.Length != 1 ||
                         ngram.Words[0].Length != 2 ||
                         ngram.Words[0][1] != '.'
                       )
                    {
                        throw new ArgumentException($"Value for <before-proper-or-number> items with attribute [ @digits-before='true' ] must be single word length of 2 with dot on end, wrong value: {ngram}");
                    }
                }
            }


            UnstickFromDigits = new HashSet<string>();
            foreach (var ngram in beforeNoProper)
            {
                if (ngram.Value.UnstickFromDigits)
                {
                    UnstickFromDigits.Add(ngram.Words[0]);
                }
            }
            foreach (var ngram in beforeProperOrNumber)
            {
                if (ngram.Value.UnstickFromDigits)
                {
                    UnstickFromDigits.Add(ngram.Words[0]);
                }
            }

            return SENTCHARTYPE_MAP;
        }
    }

    public class SentSplitterConfig
    {
        public SentSplitterConfig()
        {
            SplitBySmiles = true;
        }

        public SentSplitterConfig(string sentSplitterResourcesXmlFilename)
        {
            Model = new SentSplitterModel(sentSplitterResourcesXmlFilename);
            SplitBySmiles = true;
        }

        public SentSplitterConfig(string sentSplitterResourcesXmlFilename, string urlDetectorResourcesXmlFilename)
        {
            Model = new SentSplitterModel(sentSplitterResourcesXmlFilename);
            UrlDetectorConfig = new UrlDetectorConfig(urlDetectorResourcesXmlFilename);
            SplitBySmiles = true;
        }

        public SentSplitterModel Model
        {
            get;
            set;
        }
        public UrlDetectorConfig UrlDetectorConfig
        {
            get;
            set;
        }
        public bool SplitBySmiles
        {
            get;
            set;
        }
    }
}
