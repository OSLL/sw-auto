using System.Collections.Generic;
using System.Linq;

namespace LangAnalyzer.Morphology
{
    /// <summary>
    /// Морфотип
    /// </summary>
    internal sealed class MorphoType
    {
        private static readonly MorphoForm[] EMPTY = new MorphoForm[0];

        internal MorphoType(PartOfSpeechBase partOfSpeechBase)
        {
            MorphoForms = EMPTY;
            MorphoAttributeGroup = partOfSpeechBase.MorphoAttributeGroup;
            PartOfSpeech1 = partOfSpeechBase.PartOfSpeech;
        }

        internal void SetMorphoForms(List<MorphoForm> morphoForms)
        {
            MorphoForms = morphoForms.ToArray();
            if (morphoForms.Count != 0)
            {
                MaxEndingLength = MorphoForms.Max(morphoForm => morphoForm.Ending.Length);
            }
            else
            {
                MaxEndingLength = 0;
            }
        }

        /// получение типов атрибутов
        public MorphoAttributeGroupEnum MorphoAttributeGroup { get; }
        public PartOfSpeechEnum PartOfSpeech
        {
            get { return PartOfSpeech1; }
        }
        /// получение форм
        public MorphoForm[] MorphoForms { get; private set; }
        /// получение длины самого длинного окончания
        public int MaxEndingLength { get; private set; }

        public PartOfSpeechEnum PartOfSpeech1 { get; }

        public override string ToString()
        {
            return $"[{PartOfSpeech}, {MorphoAttributeGroup}, {{{string.Join(",", (IEnumerable<MorphoForm>)MorphoForms)}}}]";
        }
    }
}
