using System.Collections.Generic;

using LangAnalyzerStd.Core;

namespace LangAnalyzerStd.Morphology
{
    /// <summary>
    /// Морфологическая форма слова
    /// </summary>
    internal sealed class MorphoForm
    {

        private static readonly MorphoAttributePair[] EMPTY = new MorphoAttributePair[0];

        internal MorphoForm(string ending, List<MorphoAttributePair> morphoAttributePair)
        {
            Ending = string.Intern(ending);
            EndingUpper = string.Intern(StringsHelper.ToUpperInvariant(Ending));
            if (morphoAttributePair.Count != 0)
                MorphoAttributePairs = morphoAttributePair.ToArray();
            else
                MorphoAttributePairs = EMPTY;
        }
        /// получение окончания
        public string Ending { get; }
        public string EndingUpper { get; }
        /// получение атрибутов
        public MorphoAttributePair[] MorphoAttributePairs { get; }

        public override string ToString()
        {
            return $"{'['}{Ending}, {{{string.Join(",", MorphoAttributePairs)}}}]";
        }
    }
}

