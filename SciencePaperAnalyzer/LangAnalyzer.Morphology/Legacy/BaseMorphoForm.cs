namespace LangAnalyzer.Morphology
{
    /// <summary>
    /// Базовая форма слова
    /// </summary>
    internal sealed class BaseMorphoForm : IBaseMorphoForm
    {
        public BaseMorphoForm(string _base, MorphoType morphoType, MorphoAttributePair? nounType)
        {
            NounType = nounType;
            this.Base = _base;
            NormalForm = this.Base;
            if (morphoType.MorphoForms.Length != 0)
            {
                NormalForm += morphoType.MorphoForms[0].Ending;
            }
            MorphoType = morphoType;
        }

        /// получение основы
        public string Base { get; }
        /// полученеи нормальной формы
        public string NormalForm { get; }
        /// получение типа существительного
        public MorphoAttributePair? NounType { get; }

        /// получение морфотипа
        public MorphoType MorphoType { get; }

        public MorphoForm[] MorphoForms
        {
            get { return MorphoType.MorphoForms; }
        }
        public PartOfSpeechEnum PartOfSpeech
        {
            get { return MorphoType.PartOfSpeech; }
        }
        public MorphoAttributeGroupEnum MorphoAttributeGroup
        {
            get { return MorphoType.MorphoAttributeGroup; }
        }

        public override string ToString()
        {
            return $"{'['}{Base}, {NormalForm}, {NounType}, {MorphoType}{']'}";
        }
    }
}

