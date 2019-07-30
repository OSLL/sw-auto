using System.Text;

namespace LangAnalyzerStd.Tokenizing
{
    using LangAnalyzerStd.Ner;
    using LangAnalyzerStd.Postagger;
    using LangAnalyzerStd.Syntax;
    using LangAnalyzerStd.Morphology;

    public sealed class Word
    {
        public string valueOriginal;
        public string valueUpper;
        public int startIndex;
        public int length;

        #region pos-tagger
        public PosTaggerInputType posTaggerInputType;
        public PosTaggerOutputType posTaggerOutputType;

        public PosTaggerExtraWordType posTaggerExtraWordType;

        //последнее слово (без цифр, только слово буквами) в цепочке 'Numeral'. по нему будет делаться морфоанализ.
        public string posTaggerLastValueUpperInNumeralChain;

        //первая буква в оригинальном слове (valueOriginal) - большая.
        public bool posTaggerFirstCharIsUpper;
        #endregion

        #region ner
        public NerInputType nerInputType;
        public NerOutputType nerOutputType;

        //next ner-word in chain
        public Word NerNext
        {
            get;
            private set;
        }
        //previous ner-word in chain
        public Word NerPrev
        {
            get;
            private set;
        }

        public void SetNextPrev(Word next, NerOutputType nerOutputType)
        {
            NerNext = next;
            next.NerPrev = this;

            this.nerOutputType = next.nerOutputType = nerOutputType;
        }
        public bool IsFirstWordInNerChain
        {
            get { return NerNext != null && NerPrev == null; }
        }
        public bool IsWordInNerChain
        {
            get { return NerNext != null || NerPrev != null; }
        }
        public bool HasNerPrevWord
        {
            get { return NerPrev != null; }
        }
        public string GetNerValue()
        {
            return (GetNerValue(new StringBuilder()));
        }
        public string GetNerValue(StringBuilder sb)
        {
            if (NerNext != null)
            {
                sb.Clear();
                for (var w = this; w != null; w = w.NerNext)
                {
                    sb.Append(w.valueOriginal).Append(' ');
                }
                return sb.Remove(sb.Length - 1, 1).ToString();
            }
            return valueOriginal;
        }
        public int GetNerLength()
        {
            if (NerNext != null)
            {
                for (var w = this; ; w = w.NerNext)
                {
                    if (w.NerNext == null)
                    {
                        var len = ((w.startIndex - this.startIndex) + w.length);
                        return len;
                    }
                }
            }
            return length;
        }
        public int GetNerChainLength()
        {
            if (NerNext != null)
            {
                var len = 1;
                for (var w = this; ; w = w.NerNext)
                {
                    if (w.NerNext == null)
                    {
                        return len;
                    }
                    len++;
                }
            }
            return 1;
        }

        #endregion

        public WordFormMorphology morphology;

        public SyntaxRoleType syntaxRoleType;

        #region toStrings
        public override string ToString()
        {
            return ('\'' + valueUpper + " (" + valueOriginal + ")', [" + startIndex + ":" + length + "], " +
                "N:['" + nerInputType.ToString() + "'  " +
                '\'' + ((nerOutputType == NerOutputType.O) ? "-" : nerOutputType.ToString()) + '\'' +
                ((NerNext != null) ? " {-chain-}" : string.Empty) + "], " +
                "P:['" + posTaggerInputType.ToString() + "'  " +
                '\'' + ((posTaggerOutputType == PosTaggerOutputType.Other) ? "-" : posTaggerOutputType.ToString()) + '\'' +
                ((posTaggerExtraWordType != PosTaggerExtraWordType.__DEFAULT__) ? (" (is-" + posTaggerExtraWordType.ToString().ToLowerInvariant() + ')') : string.Empty) + "], " +
                "M:" + morphology.ToString() +
                ", " +
                "S:['" + ((syntaxRoleType == SyntaxRoleType.Other) ? "-" : syntaxRoleType.ToString()) + "']"
            );
        }
        public string ToStringPosTagger(bool getShorty = false)
        {
            var str = string.Format("{0}, {1}",
                (posTaggerInputType == PosTaggerInputType.O) ? "-" : posTaggerInputType.ToText(),
                (posTaggerOutputType == PosTaggerOutputType.Other) ? "-" : posTaggerOutputType.ToText() +
                ((!getShorty && posTaggerExtraWordType != PosTaggerExtraWordType.__DEFAULT__)
                    ? (" [is-" + posTaggerExtraWordType.ToString().ToLowerInvariant() + ']')
                    : string.Empty)
            );
            return str;
        }
        public string ToStringNer(bool whenBothOtherReturnNull)
        {
            if (!whenBothOtherReturnNull || (nerInputType != NerInputType.O) || (nerOutputType != NerOutputType.O))
            {
                var str = string.Format("{0}, {1}",
                    (nerInputType == NerInputType.O) ? "-" : nerInputType.ToText(),
                    (nerOutputType == NerOutputType.O) ? "-" : nerOutputType.ToText()
                );
                return str;
            }

            return null;
        }
        public string ToStringMorphology(bool notEmptyMorphoAttribute)
        {
            if (notEmptyMorphoAttribute && morphology.IsEmptyMorphoAttribute())
            {
                return null;
            }
            return morphology.ToString();
        }
        #endregion
    }
}