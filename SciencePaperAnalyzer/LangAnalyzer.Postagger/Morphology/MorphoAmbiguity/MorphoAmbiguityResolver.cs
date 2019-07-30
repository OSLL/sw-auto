using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using LangAnalyzer.Core;
using LangAnalyzer.Сrfsuite;
using LangAnalyzer.Morphology;

namespace LangAnalyzer.Postagger
{
    #region description
    /*
    'Собака на сене'
    
    'Собака'
O	w[0]|a[0]|w[1]|a[1]=N|U|E|U     w[0]|b[0]|w[1]|b[1]=N|A:2|E|A:2	    w[0]|c[0]|w[1]|c[1]=N|S|E|U	    w[0]|d[0]|w[1]|d[1]=N|F|E|U	    __BOS__
O	w[-1]|a[-1]|w[0]|a[0]=N|U|E|U	w[-1]|b[-1]|w[0]|b[0]=N|A:2|E|A:2	w[-1]|c[-1]|w[0]|c[0]=N|S|E|U	w[-1]|d[-1]|w[0]|d[0]=N|F|E|U	w[0]|a[0]|w[1]|a[1]=E|U|N|U	w[0]|b[0]|w[1]|b[1]=E|A:2|N|I:2	w[0]|c[0]|w[1]|c[1]=E|U|N|S	w[0]|d[0]|w[1]|d[1]=E|U|N|N	w[-1]|a[-1]|w[1]|a[1]=N|U|N|U	w[-1]|b[-1]|w[1]|b[1]=N|A:2|N|I:2	w[-1]|c[-1]|w[1]|c[1]=N|S|N|S	w[-1]|d[-1]|w[1]|d[1]=N|F|N|N
O	w[-1]|a[-1]|w[0]|a[0]=E|U|N|U	w[-1]|b[-1]|w[0]|b[0]=E|A:2|N|I:2	w[-1]|c[-1]|w[0]|c[0]=E|U|N|S	w[-1]|d[-1]|w[0]|d[0]=E|U|N|N	__EOS__

O	w[0]|a[0]|w[1]|a[1]=N|U|E|U	    w[0]|b[0]|w[1]|b[1]=N|A:2|E|I:2	    w[0]|c[0]|w[1]|c[1]=N|S|E|U	    w[0]|d[0]|w[1]|d[1]=N|F|E|U	    __BOS__
O	w[-1]|a[-1]|w[0]|a[0]=N|U|E|U	w[-1]|b[-1]|w[0]|b[0]=N|A:2|E|I:2	w[-1]|c[-1]|w[0]|c[0]=N|S|E|U	w[-1]|d[-1]|w[0]|d[0]=N|F|E|U	w[0]|a[0]|w[1]|a[1]=E|U|N|U	w[0]|b[0]|w[1]|b[1]=E|I:2|N|I:2	w[0]|c[0]|w[1]|c[1]=E|U|N|S	w[0]|d[0]|w[1]|d[1]=E|U|N|N	w[-1]|a[-1]|w[1]|a[1]=N|U|N|U	w[-1]|b[-1]|w[1]|b[1]=N|A:2|N|I:2	w[-1]|c[-1]|w[1]|c[1]=N|S|N|S	w[-1]|d[-1]|w[1]|d[1]=N|F|N|N
O	w[-1]|a[-1]|w[0]|a[0]=E|U|N|U	w[-1]|b[-1]|w[0]|b[0]=E|I:2|N|I:2	w[-1]|c[-1]|w[0]|c[0]=E|U|N|S	w[-1]|d[-1]|w[0]|d[0]=E|U|N|N	__EOS__

    
    'на'
O	w[0]|a[0]|w[1]|a[1]=N|U|E|U     w[0]|b[0]|w[1]|b[1]=N|A:2|E|A:2	    w[0]|c[0]|w[1]|c[1]=N|S|E|U	    w[0]|d[0]|w[1]|d[1]=N|F|E|U	    __BOS__
O	w[-1]|a[-1]|w[0]|a[0]=N|U|E|U	w[-1]|b[-1]|w[0]|b[0]=N|A:2|E|A:2	w[-1]|c[-1]|w[0]|c[0]=N|S|E|U	w[-1]|d[-1]|w[0]|d[0]=N|F|E|U	w[0]|a[0]|w[1]|a[1]=E|U|N|U	w[0]|b[0]|w[1]|b[1]=E|A:2|N|I:2	w[0]|c[0]|w[1]|c[1]=E|U|N|S	w[0]|d[0]|w[1]|d[1]=E|U|N|N	w[-1]|a[-1]|w[1]|a[1]=N|U|N|U	w[-1]|b[-1]|w[1]|b[1]=N|A:2|N|I:2	w[-1]|c[-1]|w[1]|c[1]=N|S|N|S	w[-1]|d[-1]|w[1]|d[1]=N|F|N|N
O	w[-1]|a[-1]|w[0]|a[0]=E|U|N|U	w[-1]|b[-1]|w[0]|b[0]=E|A:2|N|I:2	w[-1]|c[-1]|w[0]|c[0]=E|U|N|S	w[-1]|d[-1]|w[0]|d[0]=E|U|N|N	__EOS__

O	w[0]|a[0]|w[1]|a[1]=N|U|E|U	    w[0]|b[0]|w[1]|b[1]=N|A:2|E|I:2	    w[0]|c[0]|w[1]|c[1]=N|S|E|U	    w[0]|d[0]|w[1]|d[1]=N|F|E|U	    __BOS__
O	w[-1]|a[-1]|w[0]|a[0]=N|U|E|U	w[-1]|b[-1]|w[0]|b[0]=N|A:2|E|I:2	w[-1]|c[-1]|w[0]|c[0]=N|S|E|U	w[-1]|d[-1]|w[0]|d[0]=N|F|E|U	w[0]|a[0]|w[1]|a[1]=E|U|N|U	w[0]|b[0]|w[1]|b[1]=E|I:2|N|I:2	w[0]|c[0]|w[1]|c[1]=E|U|N|S	w[0]|d[0]|w[1]|d[1]=E|U|N|N	w[-1]|a[-1]|w[1]|a[1]=N|U|N|U	w[-1]|b[-1]|w[1]|b[1]=N|A:2|N|I:2	w[-1]|c[-1]|w[1]|c[1]=N|S|N|S	w[-1]|d[-1]|w[1]|d[1]=N|F|N|N
O	w[-1]|a[-1]|w[0]|a[0]=E|U|N|U	w[-1]|b[-1]|w[0]|b[0]=E|I:2|N|I:2	w[-1]|c[-1]|w[0]|c[0]=E|U|N|S	w[-1]|d[-1]|w[0]|d[0]=E|U|N|N	__EOS__
     
    */
    #endregion

    internal static class MorphoAmbiguity
    {
        private const byte ZERO = (byte)'\0';
        private const byte O = (byte)'O';
        private const double PROBABILITY_EQUAL_THRESHOLD = 0.000001d;

        /// <summary>
        /// Part-Of-Speech => (p/w) crf-field
        /// </summary>
        public static byte GetCRFWfieldValue(MorphoAmbiguityTuple mat)
        {
            switch (mat.word.posTaggerOutputType)
            {
                case PosTaggerOutputType.Adjective: return (byte)'J';
                case PosTaggerOutputType.AdjectivePronoun: return (byte)'R';
                case PosTaggerOutputType.Adverb: return (byte)'D';
                case PosTaggerOutputType.AdverbialParticiple: return (byte)'X';
                case PosTaggerOutputType.AdverbialPronoun: return (byte)'H';
                case PosTaggerOutputType.Article: return (byte)'A';
                case PosTaggerOutputType.AuxiliaryVerb: return (byte)'G';
                case PosTaggerOutputType.Conjunction: return (byte)'C';
                case PosTaggerOutputType.Infinitive: return (byte)'F';
                case PosTaggerOutputType.Interjection: return (byte)'I';
                case PosTaggerOutputType.Noun: return (byte)'N';
                case PosTaggerOutputType.Numeral: return (byte)'M';
                case PosTaggerOutputType.Participle: return (byte)'Z';
                case PosTaggerOutputType.Particle: return (byte)'W';
                case PosTaggerOutputType.PossessivePronoun: return (byte)'S';
                case PosTaggerOutputType.Preposition: return (byte)'E';
                case PosTaggerOutputType.Pronoun: return (byte)'Y';
                case PosTaggerOutputType.Verb: return (byte)'V';
                case PosTaggerOutputType.Punctuation:
                    switch (mat.punctuationType)
                    {
                        case MorphoAmbiguityTuple.PunctuationTypeEnum.PunctuationQuote:
                            return (byte)'Q'; //Quote
                        case MorphoAmbiguityTuple.PunctuationTypeEnum.PunctuationBracket:
                            return (byte)'B'; //Bracket
                    }
                    return (byte)'T'; //otherpunctuation
            }

            // Other
            return O;
        }

        /// <summary>
        /// Person => (a) crf-field
        /// </summary>
        public static byte GetCRFAfieldValue(MorphoAmbiguityTuple mat)
        {
            switch (mat.wordFormMorphology.PartOfSpeech)
            {
                case PartOfSpeechEnum.Noun:
                case PartOfSpeechEnum.Verb:
                case PartOfSpeechEnum.Pronoun:
                case PartOfSpeechEnum.Adjective:
                    var ma = mat.wordFormMorphology.MorphoAttribute;
                    if ((ma & MorphoAttributeEnum.First) == MorphoAttributeEnum.First)
                    {
                        return (byte)'F';
                    }
                    if ((ma & MorphoAttributeEnum.Second) == MorphoAttributeEnum.Second)
                    {
                        return (byte)'S';
                    }
                    if ((ma & MorphoAttributeEnum.Third) == MorphoAttributeEnum.Third)
                    {
                        return (byte)'T';
                    }
                    break;
            }

            return (byte)'U';
        }

        #region description
        /*
        *VerbTransitivity – этот атрибут есть только у класса глаголов, при этом у этого класса отсутствует падеж; 
          поэтому, дабы не увеличивать кол-во параметров CRF, 
          есть предложение использовать транзитивность вместо падежа (на месте падежа, т.е. в столбце b) 
          у всех элементов класса (Verb, Predicative, Infinitive, AdverbialParticiple, AuxiliaryVerb, Participle) 
          за исключением Participle и AuxiliaryVerb – там просто проставить то что есть. 
        ** Intransitive – стандартное сокращение I, но тогда будет путаться с I падежным (Instrumental). 
           Поэтому I заменяем на R. (Transitive) => T, (Intransitive) => R
        */
        #endregion

        /// <summary>
        /// Case => (b) crf-field, (e) for Verb-class (Verb, Predicative, Infinitive, AdverbialParticiple, AuxiliaryVerb, Participle)
        /// </summary>
        public static byte GetCRFBfieldValue(MorphoAmbiguityTuple mat)
        {
            switch (mat.wordFormMorphology.PartOfSpeech)
            {
                case PartOfSpeechEnum.Noun:
                case PartOfSpeechEnum.Pronoun:
                case PartOfSpeechEnum.Numeral:
                case PartOfSpeechEnum.Adjective:
                case PartOfSpeechEnum.Preposition:
                    {
                        var ma = mat.wordFormMorphology.MorphoAttribute;
                        if ((ma & MorphoAttributeEnum.Nominative) == MorphoAttributeEnum.Nominative)
                        {
                            return (byte)'N';
                        }
                        if ((ma & MorphoAttributeEnum.Genitive) == MorphoAttributeEnum.Genitive)
                        {
                            return (byte)'G';
                        }
                        if ((ma & MorphoAttributeEnum.Dative) == MorphoAttributeEnum.Dative)
                        {
                            return (byte)'D';
                        }
                        if ((ma & MorphoAttributeEnum.Accusative) == MorphoAttributeEnum.Accusative)
                        {
                            return (byte)'A';
                        }
                        if ((ma & MorphoAttributeEnum.Instrumental) == MorphoAttributeEnum.Instrumental)
                        {
                            return (byte)'I';
                        }
                        if ((ma & MorphoAttributeEnum.Prepositional) == MorphoAttributeEnum.Prepositional)
                        {
                            return (byte)'P';
                        }
                        if ((ma & MorphoAttributeEnum.Locative) == MorphoAttributeEnum.Locative)
                        {
                            return (byte)'L';
                        }
                    }
                    break;

                case PartOfSpeechEnum.Verb:
                    {
                        switch (mat.word.posTaggerOutputType)
                        {
                            case PosTaggerOutputType.AuxiliaryVerb:
                                break;

                            case PosTaggerOutputType.Participle:
                                {
                                    var ma = mat.wordFormMorphology.MorphoAttribute;
                                    if ((ma & MorphoAttributeEnum.Nominative) == MorphoAttributeEnum.Nominative)
                                    {
                                        return (byte)'N';
                                    }
                                    if ((ma & MorphoAttributeEnum.Genitive) == MorphoAttributeEnum.Genitive)
                                    {
                                        return (byte)'G';
                                    }
                                    if ((ma & MorphoAttributeEnum.Dative) == MorphoAttributeEnum.Dative)
                                    {
                                        return (byte)'D';
                                    }
                                    if ((ma & MorphoAttributeEnum.Accusative) == MorphoAttributeEnum.Accusative)
                                    {
                                        return (byte)'A';
                                    }
                                    if ((ma & MorphoAttributeEnum.Instrumental) == MorphoAttributeEnum.Instrumental)
                                    {
                                        return (byte)'I';
                                    }
                                    if ((ma & MorphoAttributeEnum.Prepositional) == MorphoAttributeEnum.Prepositional)
                                    {
                                        return (byte)'P';
                                    }
                                    if ((ma & MorphoAttributeEnum.Locative) == MorphoAttributeEnum.Locative)
                                    {
                                        return (byte)'L';
                                    }
                                }
                                break;

                            default:
                                {
                                    var ma = mat.wordFormMorphology.MorphoAttribute;
                                    if ((ma & MorphoAttributeEnum.Transitive) == MorphoAttributeEnum.Transitive)
                                    {
                                        return (byte)'T';
                                    }
                                    if ((ma & MorphoAttributeEnum.Intransitive) == MorphoAttributeEnum.Intransitive)
                                    {
                                        return (byte)'R';
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }

            return (byte)'U';
        }

        /// <summary>
        /// Number => (c) crf-field
        /// </summary>
        public static byte GetCRFCfieldValue(MorphoAmbiguityTuple mat)
        {
            switch (mat.wordFormMorphology.PartOfSpeech)
            {
                case PartOfSpeechEnum.Noun:
                case PartOfSpeechEnum.Verb:
                case PartOfSpeechEnum.Pronoun:
                case PartOfSpeechEnum.Numeral:
                case PartOfSpeechEnum.Adjective:
                    var ma = mat.wordFormMorphology.MorphoAttribute;
                    if ((ma & MorphoAttributeEnum.Singular) == MorphoAttributeEnum.Singular)
                    {
                        return (byte)'S';
                    }
                    if ((ma & MorphoAttributeEnum.Plural) == MorphoAttributeEnum.Plural)
                    {
                        return (byte)'P';
                    }
                    break;
            }

            return (byte)'U';
        }

        /// <summary>
        /// Gender => (d) crf-field
        /// </summary>
        public static byte GetCRFDfieldValue(MorphoAmbiguityTuple mat)
        {
            switch (mat.wordFormMorphology.PartOfSpeech)
            {
                case PartOfSpeechEnum.Noun:
                case PartOfSpeechEnum.Verb:
                case PartOfSpeechEnum.Pronoun:
                case PartOfSpeechEnum.Numeral:
                case PartOfSpeechEnum.Adjective:
                    var ma = mat.wordFormMorphology.MorphoAttribute;
                    if ((ma & MorphoAttributeEnum.Feminine) == MorphoAttributeEnum.Feminine)
                    {
                        return (byte)'F';
                    }
                    if ((ma & MorphoAttributeEnum.Masculine) == MorphoAttributeEnum.Masculine)
                    {
                        return (byte)'M';
                    }
                    if ((ma & MorphoAttributeEnum.Neuter) == MorphoAttributeEnum.Neuter)
                    {
                        return (byte)'N';
                    }
                    break;
            }

            return (byte)'U';
        }

        /// <summary>
        /// O => (y) crf-field
        /// </summary>
        public static byte GetCRFYfieldValue()
        {
            return O;
        }

        unsafe public static MorphoAttributeEnum ToMorphoAttributes(byte* value)
        {
            var morphoAttributes = default(MorphoAttributeEnum);

            //first letter - part-of-speech
            if ((*value++) == ZERO)
            {
                return morphoAttributes;
            }

            //Person
            switch ((*value++))
            {
                //Person - First
                case (byte)'F': morphoAttributes |= MorphoAttributeEnum.First; break;
                //Person - Second
                case (byte)'S': morphoAttributes |= MorphoAttributeEnum.Second; break;
                //Person - Third
                case (byte)'T': morphoAttributes |= MorphoAttributeEnum.Third; break;
                case (byte)'U': break;

                default:
                    return morphoAttributes;
            }

            //Case
            switch (*value++)
            {
                //Case - Nominative
                case (byte)'N': morphoAttributes |= MorphoAttributeEnum.Nominative; break;
                //Case - Genitive
                case (byte)'G': morphoAttributes |= MorphoAttributeEnum.Genitive; break;
                //Case - Dative
                case (byte)'D': morphoAttributes |= MorphoAttributeEnum.Dative; break;
                //Case - Accusative
                case (byte)'A': morphoAttributes |= MorphoAttributeEnum.Accusative; break;
                //Case - Instrumental
                case (byte)'I': morphoAttributes |= MorphoAttributeEnum.Instrumental; break;
                //Case - Prepositional
                case (byte)'P': morphoAttributes |= MorphoAttributeEnum.Prepositional; break;
                //Case - Locative
                case (byte)'L': morphoAttributes |= MorphoAttributeEnum.Locative; break;
                case (byte)'U': break;

                default:
                    return morphoAttributes;
            }

            //Number
            switch (*value++)
            {
                //Number - Singular
                case (byte)'S': morphoAttributes |= MorphoAttributeEnum.Singular; break;
                //Number - Plural
                case (byte)'P': morphoAttributes |= MorphoAttributeEnum.Plural; break;
                case (byte)'U': break;

                default:
                    return morphoAttributes;
            }

            //Gender
            switch (*value++)
            {
                //Gender - Feminine
                case (byte)'F': morphoAttributes |= MorphoAttributeEnum.Feminine; break;
                //Gender - Masculine
                case (byte)'M': morphoAttributes |= MorphoAttributeEnum.Masculine; break;
                //Gender - Neuter
                case (byte)'N': morphoAttributes |= MorphoAttributeEnum.Neuter; break;
                case (byte)'U': break;

                default:
                    return morphoAttributes;
            }

            Debug.Assert(*value == ZERO, "*value != '\0'");

            return morphoAttributes;
        }

        public static bool IsSingleItemAndEmptyMorphoAttribute(List<MorphoAmbiguityTuple> mats)
        {
            return ((mats.Count == 1) && mats[0].wordFormMorphology.IsEmptyMorphoAttribute());
        }

        /*
        Так же возможен вариант, когда два токена объединились по четырем морфохарактеристикам, но при это имеют разный регистр первой буквы:
            Путин		Noun	Genitive, Singular, Masculine, Animate, Proper
            Путин		Noun	Accusative, Singular, Masculine, Animate, Proper
            путина		Noun	Nominative, Singular, Feminine, Inanimate, Common
        В этом случае отбирать тот вариант, с каким регистром написано в тексте.                     
        */
        public static bool IsFirstPreference(MorphoAmbiguityTuple first, MorphoAmbiguityTuple second)
        {
            if (second == default(MorphoAmbiguityTuple))
                return true;

            if (first.word.posTaggerInputType != second.word.posTaggerInputType)
            {
                if (XlatUnsafe.Inst.IsUpper(first.wordFormMorphology.NormalForm[0]))
                {
                    return first.word.posTaggerInputType == PosTaggerInputType.FstC;
                }
            }
            return false;
        }
        public static bool IsProbabilityEqual(double probability1, double probability2)
        {
            return Math.Abs(probability2 - probability1) <= PROBABILITY_EQUAL_THRESHOLD;
        }
    }

    internal struct Probability
    {
        public double maxProbability;
        public MorphoAmbiguityTuple max_morphoAmbiguityTuple_0;
        public MorphoAmbiguityTuple max_morphoAmbiguityTuple_1;
        public MorphoAmbiguityTuple max_morphoAmbiguityTuple_2;
        public MorphoAmbiguityTuple max_morphoAmbiguityTuple_3;
        public MorphoAmbiguityTuple max_morphoAmbiguityTuple_4;

        public static Probability Create()
        {
            var prob = new Probability()
            {
                maxProbability = double.MinValue,
            };
            return prob;
        }
    }


    //========================= 3-Grams =========================//

    /// <summary>
    /// Конвертор в формат CRF
    /// </summary>
    unsafe internal sealed class MorphoAmbiguityResolver3g : IDisposable
    {
        #region private fields
        private const byte ZERO = (byte)'\0';
        private const byte VERTICAL_SLASH = (byte)'|';
        private const byte SEMICOLON = (byte)';';
        private const int UTF8_BUFFER_SIZE = 1024 * 4;             //4KB
        private const int ATTRIBUTE_MAX_LENGTH = UTF8_BUFFER_SIZE / 4; //1KB
        private const int MAT_THREEGRAMS_SIZE = 3;
        private const int MAT_THREEGRAMS_FIRST_INDEX = 0;
        private const int MAT_THREEGRAMS_MIDDLE_INDEX = 1;
        private const int MAT_THREEGRAMS_LAST_INDEX = 2;
        private readonly Dictionary<IntPtr, float> _modelDictionaryBytes;
        private readonly CRFNgram[] _NGramsMiddle;
        private readonly int _NGramsMiddleLength;
        private readonly CRFNgram[] _NGramsFirst;
        private readonly int _NGramsFirstLength;
        private readonly CRFNgram[] _NGramsLast;
        private readonly int _NGramsLastLength;
        private readonly GCHandle _attributeBufferGCHandle;
        private readonly byte[] _attributeBuffer;
        private byte* _attributeBufferPtrBase;
        private byte* _attributeBufferPtr;
        private List<WordMorphoAmbiguity> _wordMorphoAmbiguities;
        private int _wordMorphoAmbiguitiesCount;
        private WordMorphoAmbiguity _Wma_0, _Wma_1, _Wma_2;
        private MorphoAmbiguityTuple _MatThreegram_0, _MatThreegram_1, _MatThreegram_2;

        public MorphoAmbiguityResolverModel Model { get; }

#if DEBUG
            private readonly System.Text.StringBuilder _sb_attr_debug = new System.Text.StringBuilder();
            private readonly char[] _chars_attr_debug = new char[ ATTRIBUTE_MAX_LENGTH * 10 ];
#endif
        #endregion

        #region ctor dtor dispose
        public MorphoAmbiguityResolver3g(MorphoAmbiguityResolverModel model)
        {
            Model = model;
            _modelDictionaryBytes = model.DictionaryBytes;

            var crfTemplateFile_3g = LoadTemplate(model.Config.TemplateFilename3g);
            _NGramsFirst = crfTemplateFile_3g.GetCRFNgramsWhichCanTemplateBeApplied(MAT_THREEGRAMS_FIRST_INDEX, MAT_THREEGRAMS_SIZE);
            _NGramsFirstLength = _NGramsFirst.Length;
            _NGramsMiddle = crfTemplateFile_3g.GetCRFNgramsWhichCanTemplateBeApplied(MAT_THREEGRAMS_MIDDLE_INDEX, MAT_THREEGRAMS_SIZE);
            _NGramsMiddleLength = _NGramsMiddle.Length;
            _NGramsLast = crfTemplateFile_3g.GetCRFNgramsWhichCanTemplateBeApplied(MAT_THREEGRAMS_LAST_INDEX, MAT_THREEGRAMS_SIZE);
            _NGramsLastLength = _NGramsLast.Length;

            _attributeBuffer = new byte[ATTRIBUTE_MAX_LENGTH + 1];
            var attributeBuffer = _attributeBuffer;
            _attributeBufferGCHandle = GCHandle.Alloc(attributeBuffer, GCHandleType.Pinned);
            _attributeBufferPtrBase = (byte*)_attributeBufferGCHandle.AddrOfPinnedObject().ToPointer();
        }

        ~MorphoAmbiguityResolver3g()
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
            if (_attributeBufferPtrBase != null)
            {
                _attributeBufferGCHandle.Free();
                _attributeBufferPtrBase = null;
            }
        }

        /// <summary>
        /// Получить шаблон 
		/// </summary>
		/// <param name="templatePath">путь к файлу шаблона</param>
		/// <returns>Шаблон</returns>
        private static CRFTemplateFile LoadTemplate(string templatePath)
        {
            var result = CRFTemplateFileLoader.Load(templatePath);
            CheckTemplate(result);
            return result;
        }
        /// <summary>
        /// Проверить правильность шаблона
        /// </summary>
        /// <param name="crfTemplateFile">Шаблон</param>
        private static void CheckTemplate(CRFTemplateFile crfTemplateFile)
        {
            foreach (CRFNgram ngram in crfTemplateFile.CRFNgrams)
            {
                foreach (CRFAttribute crfAttribute in ngram.CRFAttributes)
                {
                    switch (crfAttribute.Position)
                    {
                        case -1:
                        case 0:
                        case 1:
                            break;
                        default:
                            throw new Exception($"Аттрибут '{crfAttribute.AttributeName}' содержащит недопустимое значение индекса морфо-атрибута: '{crfAttribute.Position}{'\''}");
                    }
                }
            }
        }
        #endregion


        public void Resolve(List<WordMorphoAmbiguity> wordMorphoAmbiguities)
        {
            #region init
            _wordMorphoAmbiguities = wordMorphoAmbiguities;
            _wordMorphoAmbiguitiesCount = _wordMorphoAmbiguities.Count;
#if DEBUG
                _sb_attr_debug.Clear();
#endif
            #endregion

            #region special-case-with-0,1,2-WMA
            switch (_wordMorphoAmbiguitiesCount)
            {
                case 0: goto UNINIT;
                case 1:
                    _wordMorphoAmbiguities[0].SetWordMorphologyAsUndefined();
                    goto UNINIT;

                case 2:
                    _Wma_0 = _wordMorphoAmbiguities[0];
                    _Wma_1 = _wordMorphoAmbiguities[1];

                    var prob_42 = Probability.Create();

                    foreach (var _ in IteratedOverThreeGramsMAT_42WMA())
                    {
                        //crf-tagging
                        var probability = TaggingBiGramsMAT_42WMA();
                        if (!probability.HasValue)
                        {
                            continue;
                        }

                        var __probability = probability.Value;
                        if (prob_42.maxProbability < __probability)
                        {
                            prob_42.max_morphoAmbiguityTuple_0 = _MatThreegram_0;
                            prob_42.max_morphoAmbiguityTuple_1 = _MatThreegram_1;
                            prob_42.maxProbability = __probability;
                        }
                    }

                    //set best word-morphology
                    SetBestWordMorphology(prob_42.max_morphoAmbiguityTuple_0, _Wma_0);
                    SetBestWordMorphology(prob_42.max_morphoAmbiguityTuple_1, _Wma_1);
                    goto UNINIT;
            }
            #endregion

            _Wma_0 = _wordMorphoAmbiguities[0];
            _Wma_1 = _wordMorphoAmbiguities[1];
            _Wma_2 = _wordMorphoAmbiguities[2];

            var prob = Probability.Create();

            foreach (var _ in IteratedOverThreeGramsMAT_4FirstWMA())
            {
                //crf-tagging
                var probability = TaggingThreeGramsMAT();
                if (!probability.HasValue)
                {
                    continue;
                }

                var __probability = probability.Value;
                if (prob.maxProbability < __probability)
                {
                    prob.max_morphoAmbiguityTuple_0 = _MatThreegram_0;
                    prob.max_morphoAmbiguityTuple_1 = _MatThreegram_1;
                    prob.max_morphoAmbiguityTuple_2 = _MatThreegram_2;
                    prob.maxProbability = __probability;
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_0, _Wma_0);
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_1, _Wma_1);

            foreach (var wmaIndex in IteratedOverThreeGramsWMA())
            {
                prob = Probability.Create();

                foreach (var _ in IteratedOverThreeGramsMAT())
                {
                    //crf-tagging
                    var probability = TaggingThreeGramsMAT();
                    if (!probability.HasValue)
                    {
                        continue;
                    }

                    var __probability = probability.Value;
                    if (prob.maxProbability < __probability)
                    {
                        prob.max_morphoAmbiguityTuple_1 = _MatThreegram_1;
                        prob.max_morphoAmbiguityTuple_2 = _MatThreegram_2;
                        prob.maxProbability = __probability;
                    }
                }

                //set best word-morphology
                SetBestWordMorphology(prob.max_morphoAmbiguityTuple_1, _Wma_1);
            }

            prob = Probability.Create();

            foreach (var _ in IteratedOverThreeGramsMAT_4LastWMA())
            {
                //crf-tagging
                var probability = TaggingThreeGramsMAT();
                if (!probability.HasValue)
                {
                    continue;
                }

                var __probability = probability.Value;
                if (prob.maxProbability < __probability)
                {
                    prob.max_morphoAmbiguityTuple_2 = _MatThreegram_2;
                    prob.maxProbability = __probability;
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_2, _Wma_2);

        #region uninit
        UNINIT:
            _wordMorphoAmbiguities = null;
            _MatThreegram_0 = _MatThreegram_1 = _MatThreegram_2 = default(MorphoAmbiguityTuple);
            _Wma_0 = _Wma_1 = _Wma_2 = default(WordMorphoAmbiguity);
#if DEBUG
                var _attr_debug = _sb_attr_debug.ToString();
#endif
            #endregion
        }

        private IEnumerable<int> IteratedOverThreeGramsWMA()
        {
            for (int i = 3, len = _wordMorphoAmbiguitiesCount; i < len; i++)
            {
                _Wma_0 = _Wma_1;
                _Wma_1 = _Wma_2;
                _Wma_2 = _wordMorphoAmbiguities[i];

                yield return i - 2;
            }
        }

        private IEnumerable<int> IteratedOverThreeGramsMAT()
        {
            if (_Wma_1.IsPunctuation())
            {
                yield break;
            }
            var mats_1 = _Wma_1.morphoAmbiguityTuples;
            var len_1 = mats_1.Count;

            if (1 < len_1)
            {
                var mats_0 = _Wma_0.morphoAmbiguityTuples;
                var mats_2 = _Wma_2.morphoAmbiguityTuples;

                var len_0 = mats_0.Count;
                var len_2 = mats_2.Count;

                for (var i = 0; i < len_0; i++)
                {
                    _MatThreegram_0 = mats_0[i];

                    for (var j = 0; j < len_1; j++)
                    {
                        _MatThreegram_1 = mats_1[j];

                        for (var k = 0; k < len_2; k++)
                        {
                            _MatThreegram_2 = mats_2[k];

                            yield return 0;
                        }
                    }
                }
            }
        }
        private IEnumerable<int> IteratedOverThreeGramsMAT_4FirstWMA()
        {
            var mats_0 = _Wma_0.morphoAmbiguityTuples;
            var len_0 = mats_0.Count;

            var mats_1 = _Wma_1.morphoAmbiguityTuples;
            var len_1 = mats_1.Count;

            var mats_2 = _Wma_2.morphoAmbiguityTuples;
            var len_2 = mats_2.Count;

            if ((1 < len_0) || (1 < len_1) || ((3 < _wordMorphoAmbiguitiesCount) && (1 < len_2)))
            {
                for (var i = 0; i < len_0; i++)
                {
                    _MatThreegram_0 = mats_0[i];

                    for (var j = 0; j < len_1; j++)
                    {
                        _MatThreegram_1 = mats_1[j];

                        for (var k = 0; k < len_2; k++)
                        {
                            _MatThreegram_2 = mats_2[k];

                            yield return 0;
                        }
                    }
                }
            }
        }
        private IEnumerable<int> IteratedOverThreeGramsMAT_4LastWMA()
        {
            if (_Wma_2.IsPunctuation())
            {
                yield break;
            }

            var mats_2 = _Wma_2.morphoAmbiguityTuples;
            var len_2 = mats_2.Count;

            if (1 < len_2)
            {
                var mats_0 = _Wma_0.morphoAmbiguityTuples;
                var len_0 = mats_0.Count;

                var mats_1 = _Wma_1.morphoAmbiguityTuples;
                var len_1 = mats_1.Count;

                for (var i = 0; i < len_0; i++)
                {
                    _MatThreegram_0 = mats_0[i];

                    for (var j = 0; j < len_1; j++)
                    {
                        _MatThreegram_1 = mats_1[j];

                        for (var k = 0; k < len_2; k++)
                        {
                            _MatThreegram_2 = mats_2[k];

                            yield return 0;
                        }
                    }
                }
            }
        }
        private IEnumerable<int> IteratedOverThreeGramsMAT_42WMA()
        {
            var mats_0 = _Wma_0.morphoAmbiguityTuples;
            var len_0 = mats_0.Count;

            var mats_1 = _Wma_1.morphoAmbiguityTuples;
            var len_1 = mats_1.Count;

            if ((1 < len_0) || (1 < len_1))
            {
                for (var i = 0; i < len_0; i++)
                {
                    _MatThreegram_0 = mats_0[i];

                    for (var j = 0; j < len_1; j++)
                    {
                        _MatThreegram_1 = mats_1[j];

                        yield return 0;
                    }
                }
            }
        }

        private double? TaggingThreeGramsMAT()
        {
            var marginal = default(double?);

            #region first
            marginal = BrimFirstMAT();

            #region BOS
            _attributeBufferPtr = CopyToZero(XlatUnsafe.Inst.BeginOfSentencePtrBase, _attributeBufferPtrBase);
            FillMat5CharsBufferWithZero(_attributeBufferPtr, _MatThreegram_0);
            if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f1))
            {
                marginal = marginal.GetValueOrDefault() + f1;
            }
            #endregion

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif
            #endregion

            #region middle
            var f = BrimMiddleMAT();
            if (f.HasValue)
            {
                marginal = marginal.GetValueOrDefault() + f.Value;
            }

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif
            #endregion

            #region last
            f = BrimLastMAT();
            if (f.HasValue)
            {
                marginal = marginal.GetValueOrDefault() + f.Value;
            }

            #region EOS
            _attributeBufferPtr = CopyToZero(XlatUnsafe.Inst.EndOfSentencePtrBase, _attributeBufferPtrBase);
            FillMat5CharsBufferWithZero(_attributeBufferPtr, _MatThreegram_2);
            if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f2))
            {
                marginal = marginal.GetValueOrDefault() + f2;
            }
            #endregion

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif
            #endregion

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }
        private double? TaggingBiGramsMAT_42WMA()
        {
            var marginal = default(double?);

            #region first
            marginal = BrimFirstMAT();

            #region BOS
            _attributeBufferPtr = CopyToZero(XlatUnsafe.Inst.BeginOfSentencePtrBase, _attributeBufferPtrBase);
            FillMat5CharsBufferWithZero(_attributeBufferPtr, _MatThreegram_0);
            if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f1))
            {
                marginal = marginal.GetValueOrDefault() + f1;
            }
            #endregion

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif
            #endregion

            _MatThreegram_2 = _MatThreegram_1;
            _MatThreegram_1 = _MatThreegram_0;

            #region last
            var f = BrimLastMAT();
            if (f.HasValue)
            {
                marginal = marginal.GetValueOrDefault() + f.Value;
            }

            #region EOS
            _attributeBufferPtr = CopyToZero(XlatUnsafe.Inst.EndOfSentencePtrBase, _attributeBufferPtrBase);
            FillMat5CharsBufferWithZero(_attributeBufferPtr, _MatThreegram_2);
            if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f2))
            {
                marginal = marginal.GetValueOrDefault() + f2;
            }
            #endregion

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif
            #endregion

            _MatThreegram_0 = _MatThreegram_1;
            _MatThreegram_1 = _MatThreegram_2;

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }

        private double? BrimFirstMAT()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsFirstLength; i++)
            {
                var ngram = _NGramsFirst[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 4:
                        AppendAttrValueFirstMAT(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT(ngram.CRFAttribute_3);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueFirstMAT(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _MatThreegram_0);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f0))
                {
                    marginal = marginal.GetValueOrDefault() + f0;
                }

                #endregion
            }

            return marginal;
        }
        private double? BrimMiddleMAT()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsMiddleLength; i++)
            {
                var ngram = _NGramsMiddle[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 4:
                        AppendAttrValueMiddleMAT(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueMiddleMAT(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueMiddleMAT(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueMiddleMAT(ngram.CRFAttribute_3);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueMiddleMAT(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _MatThreegram_1);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f1))
                {
                    marginal = marginal.GetValueOrDefault() + f1;
                }

                #endregion
            }

            return marginal;
        }
        private double? BrimLastMAT()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsLastLength; i++)
            {
                var ngram = _NGramsLast[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 4:
                        AppendAttrValueLastMAT(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT(ngram.CRFAttribute_3);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueLastMAT(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _MatThreegram_2);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f2))
                {
                    marginal = marginal.GetValueOrDefault() + f2;
                }

                #endregion
            }

            return marginal;
        }

        private void AppendAttrValueFirstMAT(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _MatThreegram_0);
                    break;

                case 1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _MatThreegram_1);
                    break;

                default: throw new ArgumentException("position: " + crfAttribute.Position);
            }
        }
        private void AppendAttrValueMiddleMAT(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _MatThreegram_0);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _MatThreegram_1);
                    break;

                case 1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _MatThreegram_2);
                    break;

                default: throw new ArgumentException("position: " + crfAttribute.Position);
            }
        }
        private void AppendAttrValueLastMAT(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _MatThreegram_1);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _MatThreegram_2);
                    break;

                default: throw new ArgumentException("position: " + crfAttribute.Position);
            }
        }

        unsafe private static byte* CopyToZero(byte* src, byte* dist)
        {
            for (; ; dist++, src++)
            {
                var ch = *src;
                if (ch == ZERO)
                    return (dist);

                *dist = *src;
            }
        }
        unsafe private static void FillMat5CharsBufferWithZero(byte* ptr, MorphoAmbiguityTuple mat)
        {
            *(ptr++) = SEMICOLON;
            *(ptr++) = MorphoAmbiguity.GetCRFWfieldValue(mat);
            *(ptr++) = MorphoAmbiguity.GetCRFAfieldValue(mat);
            *(ptr++) = MorphoAmbiguity.GetCRFBfieldValue(mat);
            *(ptr++) = MorphoAmbiguity.GetCRFCfieldValue(mat);
            *(ptr++) = MorphoAmbiguity.GetCRFDfieldValue(mat);
            *(ptr) = ZERO;
        }

        private static byte GetAttrValue(int columnIndex, MorphoAmbiguityTuple mat)
        {
            switch (columnIndex)
            {
                //w – part-of-speech
                case 0:
                    return MorphoAmbiguity.GetCRFWfieldValue(mat);

                //a - Person
                case 1:
                    return MorphoAmbiguity.GetCRFAfieldValue(mat);

                //b - Case
                case 2:
                    return MorphoAmbiguity.GetCRFBfieldValue(mat);

                //c - Number
                case 3:
                    return MorphoAmbiguity.GetCRFCfieldValue(mat);

                //d - Gender
                case 4:
                    return MorphoAmbiguity.GetCRFDfieldValue(mat);

                //y – искомое значение
                case 5:
                    return MorphoAmbiguity.GetCRFYfieldValue();

                default: throw new ArgumentException("columnIndex: " + columnIndex);
            }
        }

        private static void SetBestWordMorphology(MorphoAmbiguityTuple max_morphoAmbiguityTuple, WordMorphoAmbiguity wma)
        {
            if (max_morphoAmbiguityTuple != default(MorphoAmbiguityTuple))
            {
                max_morphoAmbiguityTuple.word.morphology = max_morphoAmbiguityTuple.wordFormMorphology;
                wma.SetWordMorphologyAsDefined();
            }
            else
            {
                wma.SetWordMorphologyAsUndefined();
            }
        }
    }


    //========================= 5-Grams =========================//

    /// <summary>
    /// Конвертор в формат CRF
    /// </summary>
    unsafe internal sealed class MorphoAmbiguityResolver5g : IDisposable
    {
        #region private fields
        private const byte ZERO = (byte)'\0';
        private const byte VERTICAL_SLASH = (byte)'|';
        private const byte SEMICOLON = (byte)';';
        private const int ATTRIBUTE_MAX_LENGTH = 1024; //1KB
        private const int MAT_FIVEGRAMS_SIZE = 5;
        private const int MAT_FIVEGRAMS_FIRST_INDEX = 0;
        private const int MAT_FIVEGRAMS_SECOND_INDEX = 1;
        private const int MAT_FIVEGRAMS_MIDDLE_INDEX = 2;
        private const int MAT_FIVEGRAMS_PRELAST_INDEX = 3;
        private const int MAT_FIVEGRAMS_LAST_INDEX = 4;
        private readonly Dictionary<IntPtr, float> _modelDictionaryBytes;
        private readonly CRFNgram[] _NGramsFirst;
        private readonly int _NGramsFirstLength;
        private readonly CRFNgram[] _NGramsSecond;
        private readonly int _NGramsSecondLength;
        private readonly CRFNgram[] _NGramsMiddle;
        private readonly int _NGramsMiddleLength;
        private readonly CRFNgram[] _NGramsPreLast;
        private readonly int _NGramsPreLastLength;
        private readonly CRFNgram[] _NGramsLast;
        private readonly int _NGramsLastLength;

        private readonly CRFNgram[] _NGramsFirst42;
        private readonly int _NGramsFirst42Length;
        private readonly CRFNgram[] _NGramsLast42;
        private readonly int _NGramsLast42Length;

        private readonly CRFNgram[] _NGramsSecond43;
        private readonly int _NGramsSecond43Length;

        private readonly GCHandle _attributeBufferGCHandle;
        private readonly byte[] _attributeBuffer;
        private byte* _attributeBufferPtrBase;
        private byte* _attributeBufferPtr;
        private List<WordMorphoAmbiguity> _wordMorphoAmbiguities;
        private int _wordMorphoAmbiguitiesCount;
        private WordMorphoAmbiguity _Wma_0, _Wma_1, _Wma_2, _Wma_3, _Wma_4;
        private MorphoAmbiguityTuple _Mat_0, _Mat_1, _Mat_2, _Mat_3, _Mat_4;

        public MorphoAmbiguityResolverModel Model { get; }

#if DEBUG
            private readonly System.Text.StringBuilder _sb_attr_debug = new System.Text.StringBuilder();
            private readonly char[] _chars_attr_debug = new char[ ATTRIBUTE_MAX_LENGTH * 10 ];
#endif
        #endregion

        #region ctor dtor dispose
        public MorphoAmbiguityResolver5g(MorphoAmbiguityResolverModel model)
        {
            Model = model;
            _modelDictionaryBytes = model.DictionaryBytes;

            var crfTemplateFile_5g = LoadTemplate(model.Config.TemplateFilename5g);
            _NGramsFirst = crfTemplateFile_5g.GetCRFNgramsWhichCanTemplateBeApplied(MAT_FIVEGRAMS_FIRST_INDEX, MAT_FIVEGRAMS_SIZE);
            _NGramsFirstLength = _NGramsFirst.Length;
            _NGramsSecond = crfTemplateFile_5g.GetCRFNgramsWhichCanTemplateBeApplied(MAT_FIVEGRAMS_SECOND_INDEX, MAT_FIVEGRAMS_SIZE);
            _NGramsSecondLength = _NGramsSecond.Length;
            _NGramsMiddle = crfTemplateFile_5g.GetCRFNgramsWhichCanTemplateBeApplied(MAT_FIVEGRAMS_MIDDLE_INDEX, MAT_FIVEGRAMS_SIZE);
            _NGramsMiddleLength = _NGramsMiddle.Length;
            _NGramsPreLast = crfTemplateFile_5g.GetCRFNgramsWhichCanTemplateBeApplied(MAT_FIVEGRAMS_PRELAST_INDEX, MAT_FIVEGRAMS_SIZE);
            _NGramsPreLastLength = _NGramsPreLast.Length;
            _NGramsLast = crfTemplateFile_5g.GetCRFNgramsWhichCanTemplateBeApplied(MAT_FIVEGRAMS_LAST_INDEX, MAT_FIVEGRAMS_SIZE);
            _NGramsLastLength = _NGramsLast.Length;

            //-2.1- //sent from 2-word's => [0,1];[-1,0]
            var crfTemplateFile_3g = LoadTemplate(model.Config.TemplateFilename3g);
            _NGramsFirst42 = crfTemplateFile_3g.GetCRFNgramsWhichCanTemplateBeApplied(0, 2);
            _NGramsFirst42Length = _NGramsFirst42.Length;
            _NGramsLast42 = crfTemplateFile_3g.GetCRFNgramsWhichCanTemplateBeApplied(1, 2);
            _NGramsLast42Length = _NGramsLast42.Length;

            //-2.2- //sent from 3-word's => [(0,1,2)];[(-1,0,1)];[(-2,-1,0)]
            _NGramsSecond43 = crfTemplateFile_5g.GetCRFNgramsWhichCanTemplateBeApplied(1, 3);
            _NGramsSecond43Length = _NGramsSecond43.Length;

            //-3-
            _attributeBuffer = new byte[ATTRIBUTE_MAX_LENGTH + 1];
            _attributeBufferGCHandle = GCHandle.Alloc(_attributeBuffer, GCHandleType.Pinned);
            _attributeBufferPtrBase = (byte*)_attributeBufferGCHandle.AddrOfPinnedObject().ToPointer();
        }

        ~MorphoAmbiguityResolver5g()
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
            if (_attributeBufferPtrBase != null)
            {
                _attributeBufferGCHandle.Free();
                _attributeBufferPtrBase = null;
            }
        }

        /// <summary>
        /// Получить шаблон 
		/// </summary>
		/// <param name="templatePath">путь к файлу шаблона</param>
		/// <returns>Шаблон</returns>
        private static CRFTemplateFile LoadTemplate(string templatePath)
        {
            var result = CRFTemplateFileLoader.Load(templatePath);
            CheckTemplate(result);
            return result;
        }

        /// <summary>
        /// Проверить правильность шаблона
        /// </summary>
        /// <param name="crfTemplateFile">Шаблон</param>
        private static void CheckTemplate(CRFTemplateFile crfTemplateFile)
        {
            foreach (CRFNgram ngram in crfTemplateFile.CRFNgrams)
            {
                foreach (CRFAttribute crfAttribute in ngram.CRFAttributes)
                {
                    switch (crfAttribute.Position)
                    {
                        case -2:
                        case -1:
                        case 0:
                        case 1:
                        case 2:
                            break;
                        default:
                            throw new Exception($"Аттрибут '{crfAttribute.AttributeName}' содержащит недопустимое значение индекса морфо-атрибута: '{crfAttribute.Position}{'\''}");
                    }
                }
            }
        }
        #endregion


        public void Resolve(List<WordMorphoAmbiguity> wordMorphoAmbiguities)
        {
            #region init
            _wordMorphoAmbiguities = wordMorphoAmbiguities;
            _wordMorphoAmbiguitiesCount = _wordMorphoAmbiguities.Count;
#if DEBUG
                _sb_attr_debug.Clear();
#endif
            #endregion

            #region special case with 0,1,2-WMA
            switch (_wordMorphoAmbiguitiesCount)
            {
                case 0: goto UNINIT;
                case 1:
                    _wordMorphoAmbiguities[0].SetWordMorphologyAsUndefined();
                    goto UNINIT;

                case 2:
                    Resolve42();
                    goto UNINIT;
            }
            #endregion

            #region first wma
            _Wma_0 = _wordMorphoAmbiguities[0];
            _Wma_1 = _wordMorphoAmbiguities[1];
            _Wma_2 = _wordMorphoAmbiguities[2];

            var prob = Probability.Create();

            foreach (var _ in IteratedOverFirstMAT())
            {
                //crf-tagging
                var probability = TaggingFirstMAT();
                if (!probability.HasValue)
                {
                    continue;
                }

                if (prob.maxProbability < probability.Value)
                {
                    prob.max_morphoAmbiguityTuple_0 = _Mat_0;
                    prob.maxProbability = probability.Value;
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_0, _Wma_0);
            #endregion

            #region special case with 3,4,5-&-more-WMA
            switch (_wordMorphoAmbiguitiesCount)
            {
                case 3:
                    Resolve43();
                    break;

                case 4:
                    _Wma_3 = _wordMorphoAmbiguities[3];
                    Resolve44();
                    break;

                default:
                    _Wma_3 = _wordMorphoAmbiguities[3];
                    _Wma_4 = _wordMorphoAmbiguities[4];
                    Resolve45();
                    break;
            }
        #endregion

        #region uninit
        UNINIT:
            _wordMorphoAmbiguities = null;
            _Mat_0 = _Mat_1 = _Mat_2 = _Mat_3 = _Mat_4 = default;
            _Wma_0 = _Wma_1 = _Wma_2 = _Wma_3 = _Wma_4 = default;
#if DEBUG
                var _attr_debug = _sb_attr_debug.ToString();
#endif
            #endregion
        }

        private IEnumerable<int> IteratedOverFirstMAT()
        {
            if (_Wma_0.IsPunctuation())
            {
                yield break;
            }
            var mats_0 = _Wma_0.morphoAmbiguityTuples;
            var len_0 = mats_0.Count;

            if (1 < len_0)
            {
                var mats_1 = _Wma_1.morphoAmbiguityTuples;
                var mats_2 = _Wma_2.morphoAmbiguityTuples;

                var len_1 = mats_1.Count;
                var len_2 = mats_2.Count;

                for (var i = 0; i < len_0; i++)
                {
                    _Mat_0 = mats_0[i];

                    for (var j = 0; j < len_1; j++)
                    {
                        _Mat_1 = mats_1[j];

                        for (var x = 0; x < len_2; x++)
                        {
                            _Mat_2 = mats_2[x];

                            yield return 0;
                        }
                    }
                }
            }
        }

        private double? TaggingFirstMAT()
        {
            var marginal = default(double?);

            #region first
            marginal = BrimFirstMAT();

            #region BOS
            _attributeBufferPtr = CopyToZero(XlatUnsafe.Inst.BeginOfSentencePtrBase, _attributeBufferPtrBase);
            FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_0);
            if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f1))
            {
                marginal = marginal.GetValueOrDefault() + f1;
            }
            #endregion

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif
            #endregion

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }

        private double? BrimFirstMAT()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsFirstLength; i++)
            {
                var ngram = _NGramsFirst[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 6:
                        AppendAttrValueFirstMAT(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT(ngram.CRFAttribute_3); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT(ngram.CRFAttribute_4); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT(ngram.CRFAttribute_5);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueFirstMAT(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_0);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f0))
                {
                    marginal = marginal.GetValueOrDefault() + f0;
                }

                #endregion
            }

            return marginal;
        }

        private void AppendAttrValueFirstMAT(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_0);
                    break;

                case 1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                case 2:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_2);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }


        #region sent from 2-words
        /// <summary>
        /// sent from 2-words => [(0,1)];[(-1,0)]
        /// </summary>
        private void Resolve42()
        {
            _Wma_0 = _wordMorphoAmbiguities[0];
            _Wma_1 = _wordMorphoAmbiguities[1];

            var prob = Probability.Create();

            foreach (var _ in IteratedOverMAT42())
            {
                //crf-tagging
                var probability = TaggingMAT42();
                if (probability.HasValue)
                {
                    if (prob.maxProbability < probability.Value)
                    {
                        prob.max_morphoAmbiguityTuple_0 = _Mat_0;
                        prob.max_morphoAmbiguityTuple_1 = _Mat_1;
                        prob.maxProbability = probability.Value;
                    }
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_0, _Wma_0);
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_1, _Wma_1);
        }

        private IEnumerable<int> IteratedOverMAT42()
        {
            var mats_0 = _Wma_0.morphoAmbiguityTuples;
            var len_0 = mats_0.Count;

            var mats_1 = _Wma_1.morphoAmbiguityTuples;
            var len_1 = mats_1.Count;

            if ((1 < len_0) || (1 < len_1))
            {
                for (var i = 0; i < len_0; i++)
                {
                    _Mat_0 = mats_0[i];

                    for (var j = 0; j < len_1; j++)
                    {
                        _Mat_1 = mats_1[j];

                        yield return 0;
                    }
                }
            }
        }

        private double? TaggingMAT42()
        {
            var marginal = default(double?);

            #region first
            marginal = BrimFirstMAT42();

            #region BOS
            _attributeBufferPtr = CopyToZero(XlatUnsafe.Inst.BeginOfSentencePtrBase, _attributeBufferPtrBase);
            FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_0);
            if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f1))
            {
                marginal = marginal.GetValueOrDefault() + f1;
            }
            #endregion

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif
            #endregion

            #region last
            var f = BrimLastMAT42();
            if (f.HasValue)
            {
                marginal = marginal.GetValueOrDefault() + f.Value;
            }

            #region EOS
            _attributeBufferPtr = CopyToZero(XlatUnsafe.Inst.EndOfSentencePtrBase, _attributeBufferPtrBase);
            FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_1);
            if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f2))
            {
                marginal = marginal.GetValueOrDefault() + f2;
            }
            #endregion

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif
            #endregion

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }

        private double? BrimFirstMAT42()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsFirst42Length; i++)
            {
                var ngram = _NGramsFirst42[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 4:
                        AppendAttrValueFirstMAT42(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT42(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT42(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueFirstMAT42(ngram.CRFAttribute_3);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueFirstMAT42(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_0);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f))
                {
                    marginal = marginal.GetValueOrDefault() + f;
                }

                #endregion
            }

            return marginal;
        }
        private double? BrimLastMAT42()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsLast42Length; i++)
            {
                var ngram = _NGramsLast42[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 4:
                        AppendAttrValueLastMAT42(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT42(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT42(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT42(ngram.CRFAttribute_3);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueLastMAT42(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_1);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f))
                {
                    marginal = marginal.GetValueOrDefault() + f;
                }

                #endregion
            }

            return marginal;
        }

        private void AppendAttrValueFirstMAT42(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_0);
                    break;

                case 1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }
        private void AppendAttrValueLastMAT42(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_0);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }
        #endregion


        #region sent from 3-words
        /// <summary>
        /// sent from 3-words => [(0,1,2)];[(-1,0,1)];[(-2,-1,0)]
        /// </summary>
        private void Resolve43()
        {
            #region second/middle wma
            var prob = Probability.Create();

            foreach (var _ in IteratedOverSecondMAT43())
            {
                //crf-tagging
                var probability = TaggingSecondMAT43();
                if (probability.HasValue)
                {
                    if (prob.maxProbability < probability.Value)
                    {
                        prob.max_morphoAmbiguityTuple_1 = _Mat_1;
                        prob.maxProbability = probability.Value;
                    }
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_1, _Wma_1);
            #endregion

            #region last wma
            prob = Probability.Create();

            foreach (var _ in IteratedOverLastMAT43())
            {
                //crf-tagging
                var probability = TaggingLastMAT43();
                if (probability.HasValue)
                {
                    if (prob.maxProbability < probability.Value)
                    {
                        prob.max_morphoAmbiguityTuple_2 = _Mat_2;
                        prob.maxProbability = probability.Value;
                    }
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_2, _Wma_2);
            #endregion
        }

        private IEnumerable<int> IteratedOverSecondMAT43()
        {
            if (_Wma_1.IsPunctuation())
            {
                yield break;
            }
            var mats_1 = _Wma_1.morphoAmbiguityTuples;
            var len_1 = mats_1.Count;

            if (1 < len_1)
            {
                Debug.Assert(_Wma_0.morphoAmbiguityTuples.Count == 1, "(_Wma_0.MorphoAmbiguityTuples.Count != 1)");

                var mats_2 = _Wma_2.morphoAmbiguityTuples;

                var len_2 = mats_2.Count;


                _Mat_0 = _Wma_0.morphoAmbiguityTuples[0];

                for (var j = 0; j < len_1; j++)
                {
                    _Mat_1 = mats_1[j];

                    for (var x = 0; x < len_2; x++)
                    {
                        _Mat_2 = mats_2[x];

                        yield return 0;
                    }
                }
            }
        }

        private IEnumerable<int> IteratedOverLastMAT43()
        {
            if (_Wma_2.IsPunctuation())
            {
                yield break;
            }

            var mats_2 = _Wma_2.morphoAmbiguityTuples;
            var len_2 = mats_2.Count;

            if (1 < len_2)
            {
                Debug.Assert(_Wma_0.morphoAmbiguityTuples.Count == 1, "(_Wma_0.MorphoAmbiguityTuples.Count != 1)");
                Debug.Assert(_Wma_1.morphoAmbiguityTuples.Count == 1, "(_Wma_1.MorphoAmbiguityTuples.Count != 1)");

                _Mat_0 = _Wma_0.morphoAmbiguityTuples[0];

                _Mat_1 = _Wma_1.morphoAmbiguityTuples[0];

                for (var x = 0; x < len_2; x++)
                {
                    _Mat_2 = mats_2[x];

                    yield return 0;
                }
            }
        }

        private double? TaggingSecondMAT43()
        {
            var marginal = default(double?);

            marginal = BrimSecondMAT43();

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }
        private double? TaggingLastMAT43()
        {
            var marginal = default(double?);

            marginal = BrimLastMAT43();

            #region EOS
            _attributeBufferPtr = CopyToZero(XlatUnsafe.Inst.EndOfSentencePtrBase, _attributeBufferPtrBase);
            FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_2);
            if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f2))
            {
                marginal = marginal.GetValueOrDefault() + f2;
            }
            #endregion

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }

        private double? BrimSecondMAT43()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsSecond43Length; i++)
            {
                var ngram = _NGramsSecond43[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 6:
                        AppendAttrValueSecondMAT43(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT43(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT43(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT43(ngram.CRFAttribute_3); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT43(ngram.CRFAttribute_4); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT43(ngram.CRFAttribute_5);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueSecondMAT43(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_1);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f1))
                {
                    marginal = marginal.GetValueOrDefault() + f1;
                }

                #endregion
            }

            return marginal;
        }
        private double? BrimLastMAT43()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsLastLength; i++)
            {
                var ngram = _NGramsLast[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 6:
                        AppendAttrValueLastMAT43(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT43(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT43(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT43(ngram.CRFAttribute_3); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT43(ngram.CRFAttribute_4); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT43(ngram.CRFAttribute_5);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueLastMAT43(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_2);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f2))
                {
                    marginal = marginal.GetValueOrDefault() + f2;
                }

                #endregion
            }

            return marginal;
        }

        private void AppendAttrValueSecondMAT43(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_0);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                case 1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_2);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }
        private void AppendAttrValueLastMAT43(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -2:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_0);
                    break;

                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_2);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }
        #endregion


        #region sent from 4-words
        /// <summary>
        /// sent from 4-words => [(0,1,2)];[(0,1,2);(-1,0,1)];[(-2,-1,0);(-1,0,1)];[(-2,-1,0)]
        /// </summary>
        private void Resolve44()
        {
            #region second wma
            var prob = Probability.Create();

            foreach (var _ in IteratedOverSecondMAT44())
            {
                //crf-tagging
                var probability = TaggingSecondMAT44();
                if (probability.HasValue)
                {
                    if (prob.maxProbability < probability.Value)
                    {
                        prob.max_morphoAmbiguityTuple_1 = _Mat_1;
                        prob.maxProbability = probability.Value;
                    }
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_1, _Wma_1);
            #endregion

            #region prelast/third wma
            prob = Probability.Create();

            foreach (var _ in IteratedOverPreLastMAT44())
            {
                //crf-tagging
                var probability = TaggingPreLastMAT44();
                if (probability.HasValue)
                {
                    if (prob.maxProbability < probability.Value)
                    {
                        prob.max_morphoAmbiguityTuple_2 = _Mat_2;
                        prob.maxProbability = probability.Value;
                    }
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_2, _Wma_2);
            #endregion

            #region last/fourth wma
            prob = Probability.Create();

            foreach (var _ in IteratedOverLastMAT44())
            {
                //crf-tagging
                var probability = TaggingLastMAT44();
                if (probability.HasValue)
                {
                    if (prob.maxProbability < probability.Value)
                    {
                        prob.max_morphoAmbiguityTuple_3 = _Mat_3;
                        prob.maxProbability = probability.Value;
                    }
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_3, _Wma_3);
            #endregion
        }

        private IEnumerable<int> IteratedOverSecondMAT44()
        {
            if (_Wma_1.IsPunctuation())
            {
                yield break;
            }
            var mats_1 = _Wma_1.morphoAmbiguityTuples;
            var len_1 = mats_1.Count;

            if (1 < len_1)
            {
                Debug.Assert(_Wma_0.morphoAmbiguityTuples.Count == 1, "(_Wma_0.MorphoAmbiguityTuples.Count != 1)");

                var mats_2 = _Wma_2.morphoAmbiguityTuples;
                var mats_3 = _Wma_3.morphoAmbiguityTuples;

                var len_2 = mats_2.Count;
                var len_3 = mats_3.Count;

                _Mat_0 = _Wma_0.morphoAmbiguityTuples[0];

                for (var j = 0; j < len_1; j++)
                {
                    _Mat_1 = mats_1[j];

                    for (var x = 0; x < len_2; x++)
                    {
                        _Mat_2 = mats_2[x];

                        for (var z = 0; z < len_3; z++)
                        {
                            _Mat_3 = mats_3[z];

                            yield return 0;
                        }
                    }
                }
            }
        }
        private IEnumerable<int> IteratedOverPreLastMAT44()
        {
            if (_Wma_2.IsPunctuation())
            {
                yield break;
            }

            var mats_2 = _Wma_2.morphoAmbiguityTuples;
            var len_2 = mats_2.Count;

            if (1 < len_2)
            {
                Debug.Assert(_Wma_0.morphoAmbiguityTuples.Count == 1, "(_Wma_0.MorphoAmbiguityTuples.Count != 1)");
                Debug.Assert(_Wma_1.morphoAmbiguityTuples.Count == 1, "(_Wma_1.MorphoAmbiguityTuples.Count != 1)");

                var mats_3 = _Wma_3.morphoAmbiguityTuples;

                var len_3 = mats_3.Count;

                _Mat_0 = _Wma_0.morphoAmbiguityTuples[0];

                _Mat_1 = _Wma_1.morphoAmbiguityTuples[0];

                for (var x = 0; x < len_2; x++)
                {
                    _Mat_2 = mats_2[x];

                    for (var z = 0; z < len_3; z++)
                    {
                        _Mat_3 = mats_3[z];

                        yield return 0;
                    }
                }
            }
        }
        private IEnumerable<int> IteratedOverLastMAT44()
        {
            if (_Wma_3.IsPunctuation())
            {
                yield break;
            }

            var mats_3 = _Wma_3.morphoAmbiguityTuples;
            var len_3 = mats_3.Count;

            if (1 < len_3)
            {
                Debug.Assert(_Wma_2.morphoAmbiguityTuples.Count == 1, "(_Wma_2.MorphoAmbiguityTuples.Count != 1)");
                Debug.Assert(_Wma_1.morphoAmbiguityTuples.Count == 1, "(_Wma_1.MorphoAmbiguityTuples.Count != 1)");

                _Mat_1 = _Wma_1.morphoAmbiguityTuples[0];

                _Mat_2 = _Wma_2.morphoAmbiguityTuples[0];

                for (var z = 0; z < len_3; z++)
                {
                    _Mat_3 = mats_3[z];

                    yield return 0;
                }
            }
        }

        private double? TaggingSecondMAT44()
        {
            var marginal = default(double?);

            marginal = BrimSecondMAT44();

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }
        private double? TaggingPreLastMAT44()
        {
            var marginal = default(double?);

            marginal = BrimPreLastMAT44();

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }
        private double? TaggingLastMAT44()
        {
            var marginal = default(double?);

            marginal = BrimLastMAT44();

            #region EOS
            _attributeBufferPtr = CopyToZero(XlatUnsafe.Inst.EndOfSentencePtrBase, _attributeBufferPtrBase);
            FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_3);
            if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f3))
            {
                marginal = marginal.GetValueOrDefault() + f3;
            }
            #endregion

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }

        private double? BrimSecondMAT44()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsSecondLength; i++)
            {
                var ngram = _NGramsSecond[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 6:
                        AppendAttrValueSecondMAT44(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT44(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT44(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT44(ngram.CRFAttribute_3); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT44(ngram.CRFAttribute_4); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT44(ngram.CRFAttribute_5);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueSecondMAT44(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_1);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f1))
                {
                    marginal = marginal.GetValueOrDefault() + f1;
                }

                #endregion
            }

            return marginal;
        }
        private double? BrimPreLastMAT44()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsPreLastLength; i++)
            {
                var ngram = _NGramsPreLast[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 6:
                        AppendAttrValuePreLastMAT44(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValuePreLastMAT44(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValuePreLastMAT44(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValuePreLastMAT44(ngram.CRFAttribute_3); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValuePreLastMAT44(ngram.CRFAttribute_4); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValuePreLastMAT44(ngram.CRFAttribute_5);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValuePreLastMAT44(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_2);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f2))
                {
                    marginal = marginal.GetValueOrDefault() + f2;
                }

                #endregion
            }

            return marginal;
        }
        private double? BrimLastMAT44()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsLastLength; i++)
            {
                var ngram = _NGramsLast[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 6:
                        AppendAttrValueLastMAT44(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT44(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT44(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT44(ngram.CRFAttribute_3); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT44(ngram.CRFAttribute_4); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT44(ngram.CRFAttribute_5);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueLastMAT44(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_3);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f3))
                {
                    marginal = marginal.GetValueOrDefault() + f3;
                }

                #endregion
            }

            return marginal;
        }

        private void AppendAttrValueSecondMAT44(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_0);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                case 1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_2);
                    break;

                case 2:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_3);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }

        private void AppendAttrValuePreLastMAT44(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -2:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_0);
                    break;

                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_2);
                    break;

                case 1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_3);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }

        private void AppendAttrValueLastMAT44(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -2:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_2);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_3);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }
        #endregion


        #region sent from 5-&-more-words
        /// <summary>
        /// sent from 5-&-more-words => [(0,1,2)];[(0,1,2);(-1,0,1)];[(-2,-1,0);(-1,0,1);(0,1,2)];[(-2,-1,0);(-1,0,1)];[(-2,-1,0)]
        /// </summary>
        private void Resolve45()
        {
            #region second wma
            var prob = Probability.Create();

            foreach (var _ in IteratedOverSecondMAT45())
            {
                //crf-tagging
                var probability = TaggingSecondMAT45();
                if (probability.HasValue)
                {
                    if (prob.maxProbability < probability.Value)
                    {
                        prob.max_morphoAmbiguityTuple_1 = _Mat_1;
                        prob.maxProbability = probability.Value;
                    }
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_1, _Wma_1);
            #endregion

            #region middle wma
            foreach (var wmaIndex in IteratedOverWMA45())
            {
                prob = Probability.Create();

                foreach (var _ in IteratedOverMiddleMAT45())
                {
                    //crf-tagging
                    var probability = TaggingMiddleMAT45();
                    if (probability.HasValue)
                    {
                        if (prob.maxProbability < probability.Value)
                        {
                            prob.max_morphoAmbiguityTuple_2 = _Mat_2;
                            prob.maxProbability = probability.Value;
                        }
                    }
                }

                //set best word-morphology
                SetBestWordMorphology(prob.max_morphoAmbiguityTuple_2, _Wma_2);
            }
            #endregion

            #region prelast/third-wma
            prob = Probability.Create();

            foreach (var _ in IteratedOverPreLastMAT45())
            {
                //crf-tagging
                var probability = TaggingPreLastMAT45();
                if (probability.HasValue)
                {
                    if (prob.maxProbability < probability.Value)
                    {
                        prob.max_morphoAmbiguityTuple_3 = _Mat_3;
                        prob.maxProbability = probability.Value;
                    }
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_3, _Wma_3);
            #endregion

            #region last/fourth wma
            prob = Probability.Create();

            foreach (var _ in IteratedOverLastMAT45())
            {
                //crf-tagging
                var probability = TaggingLastMAT45();
                if (probability.HasValue)
                {
                    if (prob.maxProbability < probability.Value)
                    {
                        prob.max_morphoAmbiguityTuple_4 = _Mat_4;
                        prob.maxProbability = probability.Value;
                    }
                }
            }

            //set best word-morphology
            SetBestWordMorphology(prob.max_morphoAmbiguityTuple_4, _Wma_4);
            #endregion
        }

        private IEnumerable<int> IteratedOverWMA45()
        {
            yield return (0);

            for (int i = 5, len = _wordMorphoAmbiguitiesCount; i < len; i++)
            {
                _Wma_0 = _Wma_1;
                _Wma_1 = _Wma_2;
                _Wma_2 = _Wma_3;
                _Wma_3 = _Wma_4;
                _Wma_4 = _wordMorphoAmbiguities[i];

                yield return i - 4;
            }
        }

        private IEnumerable<int> IteratedOverSecondMAT45()
        {
            if (_Wma_1.IsPunctuation())
            {
                yield break;
            }
            var mats_1 = _Wma_1.morphoAmbiguityTuples;
            var len_1 = mats_1.Count;

            if (1 < len_1)
            {
                Debug.Assert(_Wma_0.morphoAmbiguityTuples.Count == 1, "(_Wma_0.MorphoAmbiguityTuples.Count != 1)");

                var mats_2 = _Wma_2.morphoAmbiguityTuples;
                var mats_3 = _Wma_3.morphoAmbiguityTuples;

                var len_2 = mats_2.Count;
                var len_3 = mats_3.Count;

                _Mat_0 = _Wma_0.morphoAmbiguityTuples[0];

                for (var j = 0; j < len_1; j++)
                {
                    _Mat_1 = mats_1[j];

                    for (var x = 0; x < len_2; x++)
                    {
                        _Mat_2 = mats_2[x];

                        for (var z = 0; z < len_3; z++)
                        {
                            _Mat_3 = mats_3[z];

                            yield return 0;
                        }
                    }
                }
            }
        }
        private IEnumerable<int> IteratedOverMiddleMAT45()
        {
            if (_Wma_2.IsPunctuation())
            {
                yield break;
            }
            var mats_2 = _Wma_2.morphoAmbiguityTuples;
            var len_2 = mats_2.Count;

            if (1 < len_2)
            {
                Debug.Assert(_Wma_0.morphoAmbiguityTuples.Count == 1, "(_Wma_0.MorphoAmbiguityTuples.Count != 1)");
                Debug.Assert(_Wma_1.morphoAmbiguityTuples.Count == 1, "(_Wma_1.MorphoAmbiguityTuples.Count != 1)");

                var mats_3 = _Wma_3.morphoAmbiguityTuples;
                var mats_4 = _Wma_4.morphoAmbiguityTuples;

                var len_3 = mats_3.Count;
                var len_4 = mats_4.Count;

                _Mat_0 = _Wma_0.morphoAmbiguityTuples[0];

                _Mat_1 = _Wma_1.morphoAmbiguityTuples[0];

                for (var x = 0; x < len_2; x++)
                {
                    _Mat_2 = mats_2[x];

                    for (var z = 0; z < len_3; z++)
                    {
                        _Mat_3 = mats_3[z];

                        for (var w = 0; w < len_4; w++)
                        {
                            _Mat_4 = mats_4[w];

                            yield return 0;
                        }
                    }
                }
            }
        }
        private IEnumerable<int> IteratedOverPreLastMAT45()
        {
            if (_Wma_3.IsPunctuation())
            {
                yield break;
            }

            var mats_3 = _Wma_3.morphoAmbiguityTuples;
            var len_3 = mats_3.Count;

            if (1 < len_3)
            {
                Debug.Assert(_Wma_0.morphoAmbiguityTuples.Count == 1, "(_Wma_0.MorphoAmbiguityTuples.Count != 1)");
                Debug.Assert(_Wma_1.morphoAmbiguityTuples.Count == 1, "(_Wma_1.MorphoAmbiguityTuples.Count != 1)");
                Debug.Assert(_Wma_2.morphoAmbiguityTuples.Count == 1, "(_Wma_2.MorphoAmbiguityTuples.Count != 1)");

                var mats_4 = _Wma_4.morphoAmbiguityTuples;

                var len_4 = mats_4.Count;

                _Mat_0 = _Wma_0.morphoAmbiguityTuples[0];

                _Mat_1 = _Wma_1.morphoAmbiguityTuples[0];

                _Mat_2 = _Wma_2.morphoAmbiguityTuples[0];

                for (var z = 0; z < len_3; z++)
                {
                    _Mat_3 = mats_3[z];

                    for (var w = 0; w < len_4; w++)
                    {
                        _Mat_4 = mats_4[w];

                        yield return 0;
                    }
                }
            }
        }
        private IEnumerable<int> IteratedOverLastMAT45()
        {
            if (_Wma_4.IsPunctuation())
            {
                yield break;
            }

            var mats_4 = _Wma_4.morphoAmbiguityTuples;
            var len_4 = mats_4.Count;

            if (1 < len_4)
            {
                Debug.Assert(_Wma_3.morphoAmbiguityTuples.Count == 1, "(_Wma_3.MorphoAmbiguityTuples.Count != 1)");
                Debug.Assert(_Wma_2.morphoAmbiguityTuples.Count == 1, "(_Wma_2.MorphoAmbiguityTuples.Count != 1)");

                _Mat_2 = _Wma_2.morphoAmbiguityTuples[0];

                _Mat_3 = _Wma_3.morphoAmbiguityTuples[0];

                for (var z = 0; z < len_4; z++)
                {
                    _Mat_4 = mats_4[z];

                    yield return 0;
                }
            }
        }

        private double? TaggingSecondMAT45()
        {
            var marginal = default(double?);

            marginal = BrimSecondMAT45();

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }
        private double? TaggingMiddleMAT45()
        {
            var marginal = default(double?);

            marginal = BrimMiddleMAT45();

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }
        private double? TaggingPreLastMAT45()
        {
            var marginal = default(double?);

            marginal = BrimPreLastMAT45();

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }
        private double? TaggingLastMAT45()
        {
            var marginal = default(double?);

            marginal = BrimLastMAT45();

            #region EOS
            _attributeBufferPtr = CopyToZero(XlatUnsafe.Inst.EndOfSentencePtrBase, _attributeBufferPtrBase);
            FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_4);
            if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f4))
            {
                marginal = marginal.GetValueOrDefault() + f4;
            }
            #endregion

#if DEBUG
                    _sb_attr_debug.Append( '\n' );
#endif

#if DEBUG
                _sb_attr_debug.Append( '\n' );
#endif

            return marginal;
        }

        private double? BrimSecondMAT45()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsSecondLength; i++)
            {
                var ngram = _NGramsSecond[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 6:
                        AppendAttrValueSecondMAT45(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT45(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT45(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT45(ngram.CRFAttribute_3); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT45(ngram.CRFAttribute_4); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueSecondMAT45(ngram.CRFAttribute_5);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueSecondMAT45(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_1);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f1))
                {
                    marginal = marginal.GetValueOrDefault() + f1;
                }

                #endregion
            }

            return marginal;
        }
        private double? BrimMiddleMAT45()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsMiddleLength; i++)
            {
                var ngram = _NGramsMiddle[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 6:
                        AppendAttrValueMiddleMAT45(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueMiddleMAT45(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueMiddleMAT45(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueMiddleMAT45(ngram.CRFAttribute_3); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueMiddleMAT45(ngram.CRFAttribute_4); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueMiddleMAT45(ngram.CRFAttribute_5);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueMiddleMAT45(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_2);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f2))
                {
                    marginal = marginal.GetValueOrDefault() + f2;
                }

                #endregion
            }

            return marginal;
        }
        private double? BrimPreLastMAT45()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsPreLastLength; i++)
            {
                var ngram = _NGramsPreLast[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 6:
                        AppendAttrValuePreLastMAT45(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValuePreLastMAT45(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValuePreLastMAT45(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValuePreLastMAT45(ngram.CRFAttribute_3); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValuePreLastMAT45(ngram.CRFAttribute_4); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValuePreLastMAT45(ngram.CRFAttribute_5);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValuePreLastMAT45(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_3);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f3))
                {
                    marginal = marginal.GetValueOrDefault() + f3;
                }

                #endregion
            }

            return marginal;
        }
        private double? BrimLastMAT45()
        {
            var marginal = default(double?);

            for (var i = 0; i < _NGramsLastLength; i++)
            {
                var ngram = _NGramsLast[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                switch (ngram.CRFAttributesLength)
                {
                    case 6:
                        AppendAttrValueLastMAT45(ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT45(ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT45(ngram.CRFAttribute_2); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT45(ngram.CRFAttribute_3); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT45(ngram.CRFAttribute_4); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                        AppendAttrValueLastMAT45(ngram.CRFAttribute_5);
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                var crfAttr = ngram.CRFAttributes[j];
                                AppendAttrValueLastMAT45(crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }

                #region retrieve attr values
                FillMat5CharsBufferWithZero(_attributeBufferPtr, _Mat_4);
                if (_modelDictionaryBytes.TryGetValue(new IntPtr(_attributeBufferPtrBase), out float f4))
                {
                    marginal = marginal.GetValueOrDefault() + f4;
                }

                #endregion
            }

            return marginal;
        }

        private void AppendAttrValueSecondMAT45(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_0);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                case 1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_2);
                    break;

                case 2:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_3);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }
        private void AppendAttrValueMiddleMAT45(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -2:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_0);
                    break;

                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_2);
                    break;

                case 1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_3);
                    break;

                case 2:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_4);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }
        private void AppendAttrValuePreLastMAT45(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -2:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_1);
                    break;

                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_2);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_3);
                    break;

                case 1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_4);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }
        private void AppendAttrValueLastMAT45(CRFAttribute crfAttribute)
        {
            switch (crfAttribute.Position)
            {
                case -2:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_2);
                    break;

                case -1:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_3);
                    break;

                case 0:
                    *(_attributeBufferPtr++) = GetAttrValue(crfAttribute.ColumnIndex, _Mat_4);
                    break;

                default: throw new ArgumentException($"position: {crfAttribute.Position}");
            }
        }
        #endregion


        unsafe private static byte* CopyToZero(byte* src, byte* dist)
        {
            for (; ; dist++, src++)
            {
                var ch = *src;
                if (ch == ZERO)
                    return (dist);

                *dist = *src;
            }
        }
        unsafe private static void FillMat5CharsBufferWithZero(byte* ptr, MorphoAmbiguityTuple mat)
        {
            *(ptr++) = SEMICOLON;
            *(ptr++) = MorphoAmbiguity.GetCRFWfieldValue(mat);
            *(ptr++) = MorphoAmbiguity.GetCRFAfieldValue(mat);
            *(ptr++) = MorphoAmbiguity.GetCRFBfieldValue(mat);
            *(ptr++) = MorphoAmbiguity.GetCRFCfieldValue(mat);
            *(ptr++) = MorphoAmbiguity.GetCRFDfieldValue(mat);
            *(ptr) = ZERO;
        }

        private static byte GetAttrValue(int columnIndex, MorphoAmbiguityTuple mat)
        {
            switch (columnIndex)
            {
                //w – part-of-speech
                case 0:
                    return MorphoAmbiguity.GetCRFWfieldValue(mat);

                //a - Person
                case 1:
                    return MorphoAmbiguity.GetCRFAfieldValue(mat);

                //b - Case
                case 2:
                    return MorphoAmbiguity.GetCRFBfieldValue(mat);

                //c - Number
                case 3:
                    return MorphoAmbiguity.GetCRFCfieldValue(mat);

                //d - Gender
                case 4:
                    return MorphoAmbiguity.GetCRFDfieldValue(mat);

                //y – искомое значение
                case 5:
                    return MorphoAmbiguity.GetCRFYfieldValue();

                default: throw new ArgumentException($"columnIndex: {columnIndex}");
            }
        }

        private static void SetBestWordMorphology(MorphoAmbiguityTuple max_morphoAmbiguityTuple, WordMorphoAmbiguity wma)
        {
            if (max_morphoAmbiguityTuple != default(MorphoAmbiguityTuple))
            {
                max_morphoAmbiguityTuple.word.morphology = max_morphoAmbiguityTuple.wordFormMorphology;
                wma.SetWordMorphologyAsDefined();
            }
            else
            {
                wma.SetWordMorphologyAsUndefined();
            }
        }
    }

}
