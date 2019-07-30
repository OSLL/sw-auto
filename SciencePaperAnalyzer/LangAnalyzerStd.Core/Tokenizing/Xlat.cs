using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LangAnalyzerStd.Core
{
    [Flags]
    public enum CharType : ushort
    {
        __UNDEFINE__ = 0x0,

        IsUpper = 0x1,
        IsLower = 1 << 1,
        IsLetter = 1 << 2,
        IsDigit = 1 << 3,

        IsWhiteSpace = 1 << 4,
        IsPunctuation = 1 << 5,

        IsUrlBreak = 1 << 6,
        IsURIschemesChar = 1 << 7,

        IsQuote = 1 << 8,
        IsQuoteLeft = IsQuote | (1 << 9),
        IsQuoteRight = IsQuote | (1 << 10),
        IsQuoteDoubleSided = IsQuote | (1 << 11),
        IsBracket = 1 << 12,
        IsBracketLeft = IsBracket | (1 << 13),
        IsBracketRight = IsBracket | (1 << 14),
        IsQuoteOrBracket = IsQuote | IsBracket,

        IsHyphen = (1 << 15),
    }

    /// <summary>
    /// xlat-fusking-super-class
    /// </summary>

    public
    static class Xlat
    {
        public static readonly CharType[] CHARTYPE_MAP = new CharType[char.MaxValue + 1];
        private static readonly char[] QUOTES_LEFT = new[]
        { '«', //0x00AB, 0171
            '‹', //0x2039, 8249
            '„', //0x201E, 8222
            '“', //0x201C, 8220                                                              
        };
        private static readonly char[] QUOTES_RIGHT = new[]
        {
            '»', //0x00BB, 0187
            '›', //0x203A, 8250
            '”', //0x201D, 8221
            '‟' //0x201F, 8223
        };
        private static readonly char QUOTE_LEFT_RIGHT = '"'; //0x0022, 0034
        private static readonly char[] QUOTES_DOUBLE_SIDED = new[]
        {
            '‛', //0x201B, 8219 - не встречается
            '‚', //0x201A, 8218 - не встречается
            '‘', //0x2018, 8216  - не встречается
            '’', //0x2019, 8217 - не встречается в качестве кавычки                                                                      
            '\'',//
            QUOTE_LEFT_RIGHT
        };
        private static readonly char[] BRACKETS_LEFT = new[]
        {
            '(', '‹', '{', '['
        };
        private static readonly char[] BRACKETS_RIGHT = new[]
        {
            ')', '›', '}', ']'
        };
        private static readonly char[] HYPHENS = new[]
        {
            '-', '—', '–'
        };
        public static readonly char[] UPPER_INVARIANT_MAP = new char[char.MaxValue + 1];
        public static readonly char[] WHITESPACE_CHARS;

        static Xlat()
        {
            for (var c = char.MinValue;; c++)
            {
                if (char.IsUpper(c))
                    CHARTYPE_MAP[c] |= CharType.IsUpper;

                if (char.IsLower(c))
                    CHARTYPE_MAP[c] |= CharType.IsLower;

                if (char.IsLetter(c))
                    CHARTYPE_MAP[c] |= CharType.IsLetter;

                if (char.IsDigit(c))
                    CHARTYPE_MAP[c] |= CharType.IsDigit;

                var isWhiteSpace = char.IsWhiteSpace(c);
                if (isWhiteSpace)
                    CHARTYPE_MAP[c] |= CharType.IsWhiteSpace;

                var isPunctuation = char.IsPunctuation(c);
                if (isPunctuation)
                    CHARTYPE_MAP[c] |= CharType.IsPunctuation;

                #region [.is-url-break.]
                if (isWhiteSpace || (isPunctuation && !IsAscii(c)) || (c == '\0'))
                {
                    CHARTYPE_MAP[c] |= CharType.IsUrlBreak;
                }
                #endregion

                if (IsURIschemes(c))
                    CHARTYPE_MAP[c] |= CharType.IsURIschemesChar;

                if (c == char.MaxValue)
                    break;
            }

            foreach (var c in HYPHENS)
            {
                CHARTYPE_MAP[c] |= CharType.IsHyphen;
            }

            foreach (var c in QUOTES_LEFT)
            {
                CHARTYPE_MAP[c] |= CharType.IsQuoteLeft;
            }
            foreach (var c in QUOTES_RIGHT)
            {
                CHARTYPE_MAP[c] |= CharType.IsQuoteRight;
            }
            foreach (var c in QUOTES_DOUBLE_SIDED)
            {
                CHARTYPE_MAP[c] |= CharType.IsQuoteDoubleSided;
            }
            CHARTYPE_MAP[QUOTE_LEFT_RIGHT] |= CharType.IsQuoteRight | CharType.IsQuoteLeft;

            foreach (var c in BRACKETS_LEFT)
            {
                CHARTYPE_MAP[c] |= CharType.IsBracketLeft;
            }
            foreach (var c in BRACKETS_RIGHT)
            {
                CHARTYPE_MAP[c] |= CharType.IsBracketRight;
            }

            char c2;
            for (var c = char.MinValue;; c++)
            {
                switch (c)
                {
                    case 'ё':     // '\u0451'
                        c2 = 'е'; // '\u0435';
                        break;

                    case 'Ё':     // '\u0401'
                        c2 = 'Е'; // '\u0415';
                        break;

                    default:
                        c2 = c;
                        break;
                }

                UPPER_INVARIANT_MAP[c] = char.ToUpperInvariant(c2);

                if (c == char.MaxValue)
                {
                    break;
                }
            }

            var wsc = new List<char>();
            for (var c = char.MinValue;; c++)
            {
                if (char.IsWhiteSpace(c))
                    wsc.Add(c);

                if (c == char.MaxValue)
                {
                    break;
                }
            }
            WHITESPACE_CHARS = wsc.ToArray();
        }

        public static bool IsDot(char ch)
        {
            switch (ch)
            {
                case '.':
                case char.MaxValue:
                    return true;

                default:
                    return false;
            }
        }
        public static bool IsAscii(char ch)
        {
            return 0 <= ch && ch <= 127;
        }
        public static bool IsURIschemes(char ch)
        {
            if (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z'))
            {
                return true;
            }
            switch (ch)
            {
                case '-':
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsURIschemesPathSeparator(char ch)
        {
            switch (ch)
            {
                case '/':
                case '\\':
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsDegree(char ch)
        {
            switch (ch)
            {
                case '°':
                case 'º':
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsSlash(char ch)
        {
            switch (ch)
            {
                case '/':
                case '\\':
                    return true;

                default:
                    return false;
            }
        }

        public static char[] Create_LOWER_INVARIANT_MAP()
        {
            var lower_invariant_map = new char[char.MaxValue + 1];
            for (char ch = char.MinValue, ch2;; ch++)
            {
                switch (ch)
                {
                    case 'ё':     // '\u0451'
                        ch2 = 'е'; // '\u0435';
                        break;

                    case 'Ё':     // '\u0401'
                        ch2 = 'Е'; // '\u0415';
                        break;

                    default:
                        ch2 = ch;
                        break;
                }

                lower_invariant_map[ch] = char.ToLowerInvariant(ch2);

                if (ch == char.MaxValue)
                {
                    break;
                }
            }
            return lower_invariant_map;
        }
    }



    unsafe public
    sealed class XlatUnsafe
    {
        /// <summary>
        /// Обозначение начала предложения (в формате CRFSuit)
        /// </summary> 
        public const string BEGIN_OF_SENTENCE = "__BOS__";
        /// <summary>
        /// Обозначение конца предложения (в формате CRFSuit)
        /// </summary> 
        public const string END_OF_SENTENCE = "__EOS__";
        public const string INPUTTYPE_OTHER = "O";

        public readonly byte* InputtypeOtherPtrBase;
        public readonly byte* PosInputtypeOtherPtrBase;
        public readonly byte* NerInputtypeOtherPtrBase;
        public readonly byte* BeginOfSentencePtrBase;
        public readonly byte* EndOfSentencePtrBase;


        public readonly CharType* _CHARTYPE_MAP;		
        public readonly char* _UPPER_INVARIANT_MAP;

        private XlatUnsafe()
        {
            //-1-            
            var inputtypeOtherBytes = Encoding.UTF8.GetBytes(INPUTTYPE_OTHER);
            var inputtypeOtherBytesGCHandle = GCHandle.Alloc(inputtypeOtherBytes, GCHandleType.Pinned);
            InputtypeOtherPtrBase = (byte*)inputtypeOtherBytesGCHandle.AddrOfPinnedObject().ToPointer();

            //-1-            
            var posInputtypeOtherBytes = Encoding.UTF8.GetBytes(INPUTTYPE_OTHER /*POSINPUTTYPE_OTHER*/ );
            var posInputtypeOtherBytesGCHandle = GCHandle.Alloc(posInputtypeOtherBytes, GCHandleType.Pinned);
            PosInputtypeOtherPtrBase = (byte*)posInputtypeOtherBytesGCHandle.AddrOfPinnedObject().ToPointer();

            //-1-            
            var nerInputtypeOtherBytes = Encoding.UTF8.GetBytes(INPUTTYPE_OTHER /*NERINPUTTYPE_OTHER*/ );
            var nerInputtypeOtherBytesGCHandle = GCHandle.Alloc(nerInputtypeOtherBytes, GCHandleType.Pinned);
            NerInputtypeOtherPtrBase = (byte*)nerInputtypeOtherBytesGCHandle.AddrOfPinnedObject().ToPointer();

            //-2-
            var beginOfSentenceBytes = Encoding.UTF8.GetBytes(BEGIN_OF_SENTENCE);
            var beginOfSentenceBytesGCHandle = GCHandle.Alloc(beginOfSentenceBytes, GCHandleType.Pinned);
            BeginOfSentencePtrBase = (byte*)beginOfSentenceBytesGCHandle.AddrOfPinnedObject().ToPointer();

            //-3-
            var endOfSentenceBytes = Encoding.UTF8.GetBytes(END_OF_SENTENCE);
            var endOfSentenceBytesGCHandle = GCHandle.Alloc(endOfSentenceBytes, GCHandleType.Pinned);
            EndOfSentencePtrBase = (byte*)endOfSentenceBytesGCHandle.AddrOfPinnedObject().ToPointer();


            //-4-
            var _x_ = new ushort[Xlat.CHARTYPE_MAP.Length];
            for (int i = 0; i < _x_.Length; i++)
            {
                _x_[i] = (ushort)Xlat.CHARTYPE_MAP[i];
            }
            var ctmGCHandle = GCHandle.Alloc(_x_ /*xlat.CHARTYPE_MAP*/, GCHandleType.Pinned);
            _CHARTYPE_MAP = (CharType*)ctmGCHandle.AddrOfPinnedObject().ToPointer();
		
			
            //-5-
            var uimGCHandle = GCHandle.Alloc(Xlat.UPPER_INVARIANT_MAP, GCHandleType.Pinned);
            _UPPER_INVARIANT_MAP = (char*)uimGCHandle.AddrOfPinnedObject().ToPointer();
        }

        public static readonly XlatUnsafe Inst = new XlatUnsafe();

        public bool IsUpper(char ch)
        {
            return (_CHARTYPE_MAP[ch] & CharType.IsUpper) == CharType.IsUpper;
        }
        public bool IsLower(char ch)
        {
            return (_CHARTYPE_MAP[ch] & CharType.IsLower) == CharType.IsLower;
        }
        public bool IsLetter(char ch)
        {
            return (_CHARTYPE_MAP[ch] & CharType.IsLetter) == CharType.IsLetter;
        }
        public bool IsDigit(char ch)
        {
            return (_CHARTYPE_MAP[ch] & CharType.IsDigit) == CharType.IsDigit;
        }
        public bool IsWhiteSpace(char ch)
        {
            return (_CHARTYPE_MAP[ch] & CharType.IsWhiteSpace) == CharType.IsWhiteSpace;
        }
        public bool IsPunctuation(char ch)
        {
            return (_CHARTYPE_MAP[ch] & CharType.IsPunctuation) == CharType.IsPunctuation;
        }
        public bool IsHyphen(char ch)
        {
            return (_CHARTYPE_MAP[ch] & CharType.IsHyphen) == CharType.IsHyphen;
        }
    }
}
