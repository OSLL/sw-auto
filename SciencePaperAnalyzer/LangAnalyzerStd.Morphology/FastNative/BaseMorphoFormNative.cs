using System;
using System.Diagnostics;
using System.Text;
using LangAnalyzerStd.Core;

namespace LangAnalyzerStd.Morphology
{
    /// <summary>
    /// Базовая форма слова
    /// </summary>
    unsafe internal sealed class BaseMorphoFormNative : IBaseMorphoFormNative
    {
        #region tempBuffers

        private static IntPtrSet tempBufferHS;

        internal static void BeginLoad()
        {
            const int DEFAULT_CAPACITY = 107;

            tempBufferHS = new IntPtrSet(DEFAULT_CAPACITY);
        }
        internal static void EndLoad()
        {
            tempBufferHS = null;
        }
        #endregion

        /// основа
        private readonly char* _base;
        /// окончания морфо-форм
        private char*[] _morphoFormEndings;
        /// часть речи
        private readonly PartOfSpeechEnum _partOfSpeech;

        public BaseMorphoFormNative(char* _base, MorphoTypeNative morphoType)
        {
            this._base = _base;
            _partOfSpeech = morphoType.PartOfSpeech;

            Debug.Assert(morphoType.HasMorphoForms, "morphoType.MorphoForms.Length <= 0");

            _morphoFormEndings = morphoType.MorphoFormEndings;
        }

        public void AppendMorphoFormEndings(BaseMorphoFormNative other)
        {
            Debug.Assert(_morphoFormEndings[0] == other._morphoFormEndings[0],
                "_MorphoFormEndings[ 0 ] != baseMorphoForms._MorphoFormEndings[ 0 ]");

            //select longest array of morpho-form-endings
            char*[] first, second;
            if (_morphoFormEndings.Length < other._morphoFormEndings.Length)
            {
                first = other._morphoFormEndings;
                second = _morphoFormEndings;
            }
            else
            {
                first = _morphoFormEndings;
                second = other._morphoFormEndings;
            }

            fixed (char** morphoFormEndingsBase = first)
            {
                for (int i = 0, len = first.Length; i < len; i++)
                {
                    tempBufferHS.Add(new IntPtr(*(morphoFormEndingsBase + i)));
                }
            }
            //store curreent count in 'tempBufferHS'
            var count = tempBufferHS.Count;
            fixed (char** morphoFormEndingsBase = second)
            {
                for (int i = 0, len = second.Length; i < len; i++)
                {
                    tempBufferHS.Add(new IntPtr(*(morphoFormEndingsBase + i)));
                }
            }
            //if count of 'tempBufferHS' not changed, then [_MorphoFormEndings] & [other._MorphoFormEndings] are equals
            if (count != tempBufferHS.Count)
            {
                _morphoFormEndings = new char*[tempBufferHS.Count];
                fixed (char** morphoFormEndingsBase = _morphoFormEndings)
                {
                    var it = tempBufferHS.GetEnumerator();
                    for (var i = 0; it.MoveNext(); i++)
                    {
                        *(morphoFormEndingsBase + i) = (char*)it.Current;
                    }
                }
            }
            tempBufferHS.Clear();
        }

        /// получение основы
        public char* Base
        {
            get { return _base; }
        }

        /// окончания морфо-форм
        public char*[] MorphoFormEndings
        {
            get { return _morphoFormEndings; }
        }

        /// часть речи
        public PartOfSpeechEnum PartOfSpeech
        {
            get { return _partOfSpeech; }
        }

        /// получение нормальной формы
        public string GetNormalForm()
        {
            return StringsHelper.CreateWordForm(_base, _morphoFormEndings[0]);
        }

        public override string ToString()
        {
            const string format = "[base: '{0}', normal-form: '{1}', pos: '{2}', {{morpho-form-endings: '{3}'}}]";

            var sb = new StringBuilder();
            foreach (var morphoFormEnding in MorphoFormEndings)
            {
                if (sb.Length != 0)
                    sb.Append(", ");
                sb.Append(StringsHelper.ToString(morphoFormEnding));
            }

            var _base = StringsHelper.ToString(Base);

            var normalForm = GetNormalForm();

            return string.Format(format, _base, normalForm, PartOfSpeech, sb.ToString());
        }
    }
}