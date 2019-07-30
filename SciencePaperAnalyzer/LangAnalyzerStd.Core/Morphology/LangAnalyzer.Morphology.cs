using System;
using System.Collections.Generic;
using System.Runtime;
using LangAnalyzerStd.Core;

namespace LangAnalyzerStd.Morphology
{
    unsafe public interface IBaseMorphoFormNative
    {
        char* Base { get; }

        char*[] MorphoFormEndings { get; }

        PartOfSpeechEnum PartOfSpeech { get; }
    }

    public interface IBaseMorphoForm
    {
        string NormalForm { get; }

        PartOfSpeechEnum PartOfSpeech { get; }
    }

    /// <summary>
    /// форма слова
    /// </summary>
    public struct WordForm
    {
        public WordForm(string form, PartOfSpeechEnum partOfSpeech)
        {
            Form = form;
            PartOfSpeech = partOfSpeech;
        }

        /// форма
        public string Form;

        /// часть речи
        public PartOfSpeechEnum PartOfSpeech;

        public override string ToString()
        {
            return '[' + Form + ", " + PartOfSpeech + ']';
        }
    }

    /// <summary>
    /// формы слова
    /// </summary>
    public struct WordForms
    {
        private static readonly List<WordForm> EMPTY = new List<WordForm>(0);

        public WordForms(string word)
        {
            Word = word;
            Forms = EMPTY;
        }

        /// исходное слово
        public string Word;
        /// формы слова
        public List<WordForm> Forms;

        public bool HasForms
        {
            get { return Forms != null && Forms.Count != 0; }
        }

        public override string ToString()
        {
            return '[' + Word + ", {" + string.Join(",", Forms) + "}]";
        }
    }

    /// <summary>
    /// морфохарактеристики формы слова
    /// </summary>
    unsafe public struct WordFormMorphology
    {
        private readonly char* _base;
        private readonly char* _ending;

        public WordFormMorphology(IBaseMorphoFormNative baseMorphoForm, MorphoAttributeEnum morphoAttribute)
            : this()
        {
            _base = baseMorphoForm.Base;
            _ending = baseMorphoForm.MorphoFormEndings[0];
            PartOfSpeech = baseMorphoForm.PartOfSpeech;
            MorphoAttribute = morphoAttribute;
        }
        public WordFormMorphology(IBaseMorphoForm baseMorphoForm, MorphoAttributeEnum morphoAttribute)
            : this()
        {
            _NormalForm = baseMorphoForm.NormalForm;
            PartOfSpeech = baseMorphoForm.PartOfSpeech;
            MorphoAttribute = morphoAttribute;
        }
        public WordFormMorphology(PartOfSpeechEnum partOfSpeech)
            : this()
        {
            PartOfSpeech = partOfSpeech;
        }
        public WordFormMorphology(PartOfSpeechEnum partOfSpeech, MorphoAttributeEnum morphoAttribute)
            : this()
        {
            PartOfSpeech = partOfSpeech;
            MorphoAttribute = morphoAttribute;
        }

        private string _NormalForm;
        /// нормальная форма
        public string NormalForm
        {
            get
            {
                if (_NormalForm == null)
                {
                    if ((IntPtr)_base != IntPtr.Zero)
                    {
                        _NormalForm = StringsHelper.CreateWordForm(_base, _ending);
                    }
                }
                return (_NormalForm);
            }
        }
        /// часть речи
        public readonly PartOfSpeechEnum PartOfSpeech;
        /// морфохарактеристики
        public readonly MorphoAttributeEnum MorphoAttribute;

        public bool IsEmpty()
        {
            return MorphoAttribute == MorphoAttributeEnum.__UNDEFINED__ &&
                PartOfSpeech == PartOfSpeechEnum.Other &&
                _NormalForm == null &&
                (IntPtr)_base == IntPtr.Zero;
        }
        public bool IsEmptyMorphoAttribute()
        {
            return MorphoAttribute == MorphoAttributeEnum.__UNDEFINED__;
        }
        public bool IsEmptyNormalForm()
        {
            return (_NormalForm == null) && ((IntPtr)_base == IntPtr.Zero);
        }

        public override string ToString()
        {
            return '[' + NormalForm + ", " + PartOfSpeech + ", " + MorphoAttribute + "]";
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool Equals(WordFormMorphology x, WordFormMorphology y)
        {
            return Equals(ref x, ref y);
        }
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool Equals(ref WordFormMorphology x, ref WordFormMorphology y)
        {
            return x.MorphoAttribute == y.MorphoAttribute &&
                x.PartOfSpeech == y.PartOfSpeech &&
                x._base == y._base &&
                x._ending == y._ending &&
                string.CompareOrdinal(x._NormalForm, y._NormalForm) == 0;
        }
    }

    /// <summary>
    /// информация о морфологических свойствах слова
    /// </summary>
    public struct WordMorphology
    {
        /// массив морфохарактеристик
        public List<WordFormMorphology> WordFormMorphologies;
        /// часть речи
        public PartOfSpeechEnum PartOfSpeech;
        public bool IsSinglePartOfSpeech;

        public bool HasWordFormMorphologies
        {
            get { return WordFormMorphologies != null && WordFormMorphologies.Count != 0; }
        }

        public override string ToString()
        {
            return $"[{PartOfSpeech}, {{{(HasWordFormMorphologies ? string.Join(",", WordFormMorphologies) : "NULL")}}}]";
        }
    }
}
