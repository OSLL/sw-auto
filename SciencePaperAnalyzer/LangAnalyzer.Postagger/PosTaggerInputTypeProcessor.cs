using System;
using System.Collections.Generic;

using LangAnalyzer.Core;
using LangAnalyzer.Tokenizing;

namespace LangAnalyzer.Postagger
{
    internal struct PosTaggerInputTypeProcessorFactory : IPosTaggerInputTypeProcessorFactory
    {
        private readonly IPosTaggerInputTypeProcessor _posTaggerInputTypeProcessor;

        internal PosTaggerInputTypeProcessorFactory(PosTaggerResourcesModel model,
            LanguageTypeEnum languageType)
        {
            switch (languageType)
            {
                case LanguageTypeEnum.Ru:
                    _posTaggerInputTypeProcessor = new PosTaggerInputTypeProcessorRu(model.Numbers, model.Abbreviations);
                    break;

                case LanguageTypeEnum.En:
                    _posTaggerInputTypeProcessor = new PosTaggerInputTypeProcessorEn(model.Numbers, model.Abbreviations);
                    break;

                default:
                    throw new ArgumentException(languageType.ToString());
            }
        }

        public IPosTaggerInputTypeProcessor CreateInstance()
        {
            return _posTaggerInputTypeProcessor;
        }
    }

    /// <summary>
    /// Обработчик Графематических характеристик.Подкрепляет к словам определенные признаки
    /// </summary>
    unsafe internal sealed class PosTaggerInputTypeProcessorRu : IPosTaggerInputTypeProcessor
    {
        private readonly HashSet<string> _numbers;
        private readonly HashSet<string> _abbreviations;
        private readonly CharType* _CTM;

        public PosTaggerInputTypeProcessorRu(HashSet<string> numbers, HashSet<string> abbreviations)
        {
            _numbers = numbers;
            _abbreviations = abbreviations;
            _CTM = XlatUnsafe.Inst._CHARTYPE_MAP;
        }

        unsafe private static int LastPositionOfHyphen(char* _base, int length)
        {
            for (length--; 0 <= length; length--)
            {
                var ct = *(XlatUnsafe.Inst._CHARTYPE_MAP + *(_base + length));
                if ((ct & CharType.IsHyphen) == CharType.IsHyphen)
                {
                    return length + 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Слово на латинице
        /// </summary>
        unsafe private static bool IsLatin(char* _base, int length)
        {
            var hasLatinLetter = false;
            for (int i = 0; i < length; i++)
            {
                var ch = *(_base + i);

                if (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z'))
                {
                    hasLatinLetter = true;
                    continue;
                }

                if ((*(XlatUnsafe.Inst._CHARTYPE_MAP + ch) & CharType.IsLetter) == CharType.IsLetter)
                {
                    return false;
                }
            }

            return hasLatinLetter;
        }

        /// <summary>
        /// Римская цифра
        /// </summary>
        private static bool IsRomanSymbol(char ch)
        {
            switch (ch)
            {
                case 'I':
                case 'V':
                case 'X':
                case 'L':
                case 'C':
                case 'D':
                case 'M':
                    return true;
            }
            return false;
        }

        unsafe public PosTaggerInputTypeResult GetResult(char* _base, int length, Word word)
        {
            int digitsCount = 0,
                upperCount = 0,
                lowerCount = 0,
                hyphenCount = 0,
                pointCount = 0,
                romanNumberCount = 0;
            int firstHyphenIndex = -1;

            #region main cycle
            for (int i = 0; i < length; i++)
            {
                var ch = *(_base + i);
                var ct = *(_CTM + ch);
                if ((ct & CharType.IsDigit) == CharType.IsDigit)
                {
                    digitsCount++;
                }
                else if ((ct & CharType.IsLower) == CharType.IsLower)
                {
                    lowerCount++;
                }
                else if ((ct & CharType.IsUpper) == CharType.IsUpper)
                {
                    upperCount++;
                    if (IsRomanSymbol(ch))
                        romanNumberCount++;
                }
                else if ((ct & CharType.IsHyphen) == CharType.IsHyphen)
                {
                    hyphenCount++;

                    if ((firstHyphenIndex == -1) && (i != 0) && (digitsCount == 0) && (i == lowerCount + upperCount))
                    {
                        firstHyphenIndex = i;
                    }
                }
                else if (Xlat.IsDot(ch))
                {
                    pointCount++;
                }
            }
            #endregion

            if (pointCount == 0)
            {
                if ((digitsCount == 0) && (0 < romanNumberCount) && ((romanNumberCount == length) || (romanNumberCount == length - hyphenCount)))
                    return PosTaggerInputTypeResult.Num;

                if (IsLatin(_base, length))
                    return PosTaggerInputTypeResult.AllLat;
            }


            if ((lowerCount == 0) && (upperCount == 0))
            {
                /// цифры в любой комбинации со знаками препинаний без букв - NUM
                if (digitsCount != 0)
                    return PosTaggerInputTypeResult.Num;

                var _first_ch = *_base;
                switch (_first_ch)
                {
                    // запятая - Com
                    case ',': return PosTaggerInputTypeResult.Com;
                    // двоеточие - Col
                    case ':': return PosTaggerInputTypeResult.Col;
                }

                var _first_ct = *(_CTM + _first_ch);
                // дефис - Dush
                if ((_first_ct & CharType.IsHyphen) == CharType.IsHyphen)
                    return PosTaggerInputTypeResult.Dash;
            }
            else
            if ((digitsCount == 0) && (firstHyphenIndex == -1))
            {
                switch (pointCount)
                {
                    case 0:
                        if ((hyphenCount == 0) && _numbers.Contains(word.valueUpper))
                        {
                            return PosTaggerInputTypeResult.CreateNum();
                        }
                        break;
                    case 1:
                        if ((hyphenCount == 0) && (Xlat.IsDot(*(_base + length - 1))) && _numbers.Contains(word.valueUpper))
                        {
                            return PosTaggerInputTypeResult.CreateNum();
                        }
                        break;
                    default:
                        if ((hyphenCount == 0) && _abbreviations.Contains(word.valueOriginal))
                        {
                            return PosTaggerInputTypeResult.IsAbbreviation;
                        }
                        break;
                }
            }


            var first_ch = *_base;
            var first_ct = *(_CTM + first_ch);

            var isFirstUpper = (1 < length) && ((first_ct & CharType.IsUpper) == CharType.IsUpper);
            if (isFirstUpper)
            {
                if ((lowerCount != 0) && (0 < upperCount) && (pointCount == 0))
                    return PosTaggerInputTypeResult.FstC;

                if (pointCount != 0)
                {
                    var ch = *(_base + 1);
                    if (Xlat.IsDot(ch))
                        return PosTaggerInputTypeResult.OneCP;
                }
            }


            if ((first_ct & CharType.IsDigit) == CharType.IsDigit)
                return PosTaggerInputTypeResult.Num;

            if (firstHyphenIndex != -1)
            {
                var firstNumberWord = word.valueUpper.Substring(0, firstHyphenIndex);
                if (_numbers.Contains(firstNumberWord))
                {
                    var p = LastPositionOfHyphen(_base, length);
                    var v = word.valueUpper.Substring(p);
                    return PosTaggerInputTypeResult.CreateNum(v);
                }
            }

            if ((digitsCount == 0) && (lowerCount == 0) && (upperCount == 0))
            {
                return PosTaggerInputTypeResult.IsPunctuation;
            }

            return PosTaggerInputTypeResult.O;
        }
    }

    /// <summary>
    /// Обработчик Графематических характеристик.Подкрепляет к словам определенные признаки
    /// </summary>
    unsafe internal sealed class PosTaggerInputTypeProcessorEn : IPosTaggerInputTypeProcessor
    {
        private readonly HashSet<string> _numbers;
        private readonly HashSet<string> _abbreviations;
        private readonly CharType* _CTM;

        public PosTaggerInputTypeProcessorEn(HashSet<string> numbers, HashSet<string> abbreviations)
        {
            _numbers = numbers;
            _abbreviations = abbreviations;
            _CTM = XlatUnsafe.Inst._CHARTYPE_MAP;
        }

        unsafe private static int LastPositionOfHyphen(char* _base, int length)
        {
            for (length--; 0 <= length; length--)
            {
                var ct = *(XlatUnsafe.Inst._CHARTYPE_MAP + *(_base + length));
                if ((ct & CharType.IsHyphen) == CharType.IsHyphen)
                {
                    return length + 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Римская цифра
        /// </summary>
        private static bool IsRomanSymbol(char ch)
        {
            switch (ch)
            {
                case 'I':
                case 'V':
                case 'X':
                case 'L':
                case 'C':
                case 'D':
                case 'M':
                    return true;
            }
            return false;
        }

        unsafe public PosTaggerInputTypeResult GetResult(char* _base, int length, Word word)
        {
            int digitsCount = 0,
                upperCount = 0,
                lowerCount = 0,
                hyphenCount = 0,
                pointCount = 0,
                romanNumberCount = 0;
            int firstHyphenIndex = -1;

            #region main cycle
            for (int i = 0; i < length; i++)
            {
                var ch = *(_base + i);
                var ct = *(_CTM + ch);
                if ((ct & CharType.IsDigit) == CharType.IsDigit)
                {
                    digitsCount++;
                }
                else if ((ct & CharType.IsLower) == CharType.IsLower)
                {
                    lowerCount++;
                }
                else if ((ct & CharType.IsUpper) == CharType.IsUpper)
                {
                    upperCount++;
                    if (IsRomanSymbol(ch))
                        romanNumberCount++;
                }
                else if ((ct & CharType.IsHyphen) == CharType.IsHyphen)
                {
                    hyphenCount++;

                    if ((firstHyphenIndex == -1) && (i != 0) && (digitsCount == 0) && (i == lowerCount + upperCount))
                    {
                        firstHyphenIndex = i;
                    }
                }
                else if (Xlat.IsDot(ch))
                {
                    pointCount++;
                }
            }
            #endregion

            if (pointCount == 0)
            {
                if ((digitsCount == 0) && (0 < romanNumberCount) && ((romanNumberCount == length) || (romanNumberCount == length - hyphenCount)))
                    return PosTaggerInputTypeResult.Num;
            }


            if ((lowerCount == 0) && (upperCount == 0))
            {
                /// цифры в любой комбинации со знаками препинаний без букв - NUM
                if (digitsCount != 0)
                    return PosTaggerInputTypeResult.Num;

                var _first_ch = *_base;
                switch (_first_ch)
                {
                    // запятая - Com
                    case ',': return PosTaggerInputTypeResult.Com;
                    // двоеточие - Col
                    case ':': return PosTaggerInputTypeResult.Col;
                }

                var _first_ct = *(_CTM + _first_ch);
                // дефис - Dush
                if ((_first_ct & CharType.IsHyphen) == CharType.IsHyphen)
                    return PosTaggerInputTypeResult.Dash;
            }
            else
            if ((digitsCount == 0) && (firstHyphenIndex == -1))
            {
                switch (pointCount)
                {
                    case 0:
                        if ((hyphenCount == 0) && _numbers.Contains(word.valueUpper))
                        {
                            return PosTaggerInputTypeResult.CreateNum();
                        }
                        break;

                    case 1:
                        if ((hyphenCount == 0) && (Xlat.IsDot(*(_base + length - 1))) && _numbers.Contains(word.valueUpper))
                        {
                            return PosTaggerInputTypeResult.CreateNum();
                        }
                        break;

                    default:
                        if ((hyphenCount == 0) && _abbreviations.Contains(word.valueOriginal))
                        {
                            return PosTaggerInputTypeResult.IsAbbreviation;
                        }
                        break;
                }
            }


            var first_ch = *_base;
            var first_ct = *(_CTM + first_ch);

            var isFirstUpper = (1 < length) && ((first_ct & CharType.IsUpper) == CharType.IsUpper);
            if (isFirstUpper)
            {
                if ((lowerCount != 0) && (0 < upperCount) && (pointCount == 0))
                    return PosTaggerInputTypeResult.FstC;

                if (pointCount != 0)
                {
                    var ch = *(_base + 1);
                    if (Xlat.IsDot(ch))
                        return PosTaggerInputTypeResult.OneCP;
                }
            }


            if ((first_ct & CharType.IsDigit) == CharType.IsDigit)
                return PosTaggerInputTypeResult.Num;

            if (firstHyphenIndex != -1)
            {
                var firstNumberWord = word.valueUpper.Substring(0, firstHyphenIndex);
                if (_numbers.Contains(firstNumberWord))
                {
                    var p = LastPositionOfHyphen(_base, length);
                    var v = word.valueUpper.Substring(p);
                    return PosTaggerInputTypeResult.CreateNum(v);
                }
            }

            if ((digitsCount == 0) && (lowerCount == 0) && (upperCount == 0))
            {
                return PosTaggerInputTypeResult.IsPunctuation;
            }

            return PosTaggerInputTypeResult.O;
        }
    }
}
