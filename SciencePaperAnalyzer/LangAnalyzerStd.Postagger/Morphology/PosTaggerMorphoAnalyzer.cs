using System;
using System.Collections.Generic;

using LangAnalyzerStd.Core;
using LangAnalyzerStd.Morphology;
using LangAnalyzerStd.Ner;
using LangAnalyzerStd.Tokenizing;
using MorphoAmbiguityResolver = LangAnalyzerStd.Postagger.MorphoAmbiguityResolver5g;

namespace LangAnalyzerStd.Postagger
{
    internal sealed class MorphoAmbiguityTuple
    {
        public enum PunctuationTypeEnum : byte
        {
            __NonPunctuation__,
            PunctuationQuote,
            PunctuationBracket,
            Punctuation,
        }

        unsafe public MorphoAmbiguityTuple(
            Word word,
            WordFormMorphology wordFormMorphology,
            PunctuationTypeEnum punctuationType)
        {
            this.word = word;
            this.wordFormMorphology = wordFormMorphology;
            this.punctuationType = punctuationType;
        }

        unsafe public static PunctuationTypeEnum GetPunctuationType(Word word)
        {
            if (word.posTaggerOutputType == PosTaggerOutputType.Punctuation)
            {
                if (word.nerInputType == NerInputType.Q)
                {
                    return PunctuationTypeEnum.PunctuationQuote;
                }
                else
                {
                    fixed (char* _base = word.valueOriginal)
                    {
                        var ct = *(XlatUnsafe.Inst._CHARTYPE_MAP + *_base);
                        if ((ct & CharType.IsQuote) == CharType.IsQuote)
                        {
                            word.nerInputType = NerInputType.Q;
                            return PunctuationTypeEnum.PunctuationQuote;
                        }
                        else
                        if ((ct & CharType.IsBracket) == CharType.IsBracket)
                        {
                            return PunctuationTypeEnum.PunctuationBracket;
                        }
                        else
                        {
                            return PunctuationTypeEnum.Punctuation;
                        }
                    }
                }
            }
            else
            {
                return PunctuationTypeEnum.__NonPunctuation__;
            }
        }

        public Word word;
        public WordFormMorphology wordFormMorphology;
        public PunctuationTypeEnum punctuationType;

        public override string ToString()
        {
            return $"{((punctuationType != PunctuationTypeEnum.__NonPunctuation__) ? ("{" + punctuationType + "}, ") : string.Empty)}Word: {word}, WordFormMorphology: {wordFormMorphology}";
        }
    }

    internal sealed class WordMorphoAmbiguity
    {
        public WordMorphoAmbiguity(
            Word word,
            MorphoAmbiguityTuple.PunctuationTypeEnum punctuationType,
            List<MorphoAmbiguityTuple> morphoAmbiguityTuples)
        {
            this.word = word;
            this.punctuationType = punctuationType;
            this.morphoAmbiguityTuples = morphoAmbiguityTuples;
        }

        public readonly Word word;
        public readonly MorphoAmbiguityTuple.PunctuationTypeEnum punctuationType;
        public readonly List<MorphoAmbiguityTuple> morphoAmbiguityTuples;

        public bool IsPunctuation()
        {
            return punctuationType != MorphoAmbiguityTuple.PunctuationTypeEnum.__NonPunctuation__;
        }
        public void SetWordMorphologyAsUndefined()
        {
            var wma = morphoAmbiguityTuples[0];

            if (!wma.wordFormMorphology.IsEmpty())
            {
                word.morphology = wma.wordFormMorphology;
            }
            else
            {
                var partOfSpeech = PosTaggerMorphoAnalyzer.ToPartOfSpeech(word.posTaggerOutputType).GetValueOrDefault();
                word.morphology = new WordFormMorphology(partOfSpeech);
            }

            morphoAmbiguityTuples.Clear();
            morphoAmbiguityTuples.Add(new MorphoAmbiguityTuple(word, word.morphology, wma.punctuationType));
        }

        public void SetWordMorphologyAsDefined()
        {
            var punctuationType = morphoAmbiguityTuples[0].punctuationType;
            morphoAmbiguityTuples.Clear();
            morphoAmbiguityTuples.Add(new MorphoAmbiguityTuple(word, word.morphology, punctuationType));
        }

        public override string ToString()
        {
            return $"{((punctuationType != MorphoAmbiguityTuple.PunctuationTypeEnum.__NonPunctuation__) ? ("{" + punctuationType + "}, ") : string.Empty)}MorphoAmbiguityTuples: {morphoAmbiguityTuples.Count}, Word: {word}";
        }
    }

    unsafe internal struct WordMorphoAmbiguityFactory
    {
        private const int DEFAULT_WORD_COUNT = 100;
        private const int DEFAULT_WORDFORMMORPHOLOGY_COUNT = 5;

        private readonly List<List<MorphoAmbiguityTuple>> _MorphoAmbiguityTuples_Buffer;

        public WordMorphoAmbiguityFactory(object _dummy_)
        {
            _MorphoAmbiguityTuples_Buffer = new List<List<MorphoAmbiguityTuple>>(DEFAULT_WORD_COUNT);
            for (var i = 0; i < _MorphoAmbiguityTuples_Buffer.Capacity; i++)
            {
                _MorphoAmbiguityTuples_Buffer.Add(new List<MorphoAmbiguityTuple>(DEFAULT_WORDFORMMORPHOLOGY_COUNT));
            }
        }

        unsafe public WordMorphoAmbiguity Create(Word word, int wordIdex)
        {
            while (_MorphoAmbiguityTuples_Buffer.Count <= wordIdex)
            {
                _MorphoAmbiguityTuples_Buffer.Add(new List<MorphoAmbiguityTuple>(DEFAULT_WORDFORMMORPHOLOGY_COUNT));
            }

            var punctuationType = MorphoAmbiguityTuple.GetPunctuationType(word);
            var buffer = _MorphoAmbiguityTuples_Buffer[wordIdex];
            buffer.Clear();
            buffer.Add(new MorphoAmbiguityTuple(word, new WordFormMorphology(), punctuationType));

            return new WordMorphoAmbiguity(word, punctuationType, buffer);
        }

        unsafe public WordMorphoAmbiguity Create(Word word, int wordIdex, WordFormMorphology[] wordFormMorphologies)
        {
            while (_MorphoAmbiguityTuples_Buffer.Count <= wordIdex)
            {
                _MorphoAmbiguityTuples_Buffer.Add(new List<MorphoAmbiguityTuple>(DEFAULT_WORDFORMMORPHOLOGY_COUNT));
            }

            var punctuationType = MorphoAmbiguityTuple.GetPunctuationType(word);
            var buffer = _MorphoAmbiguityTuples_Buffer[wordIdex];
            buffer.Clear();
            for (int i = 0, len = wordFormMorphologies.Length; i < len; i++)
            {
                buffer.Add(new MorphoAmbiguityTuple(word, wordFormMorphologies[i], punctuationType));
            }
            return new WordMorphoAmbiguity(word, punctuationType, buffer);
        }
    }

    unsafe public sealed class PosTaggerMorphoAnalyzer : IDisposable
    {
        private readonly MorphoAnalyzer _morphoAnalyzer;
        private readonly IMorphoModel _morphoModel;
        private readonly MorphoAmbiguityPreProcessor _morphoAmbiguityPreProcessor;
        private readonly MorphoAmbiguityResolver _morphoAmbiguityResolver;
        private readonly List<WordFormMorphology> _wordFormMorphologies_Buffer;
        private readonly WordMorphoAmbiguityFactory _wordMorphoAmbiguityFactory;
        private readonly List<WordMorphoAmbiguity> _wordMorphoAmbiguities;

        public unsafe CharType* CTM { get; }

        public PosTaggerMorphoAnalyzer(IMorphoModel morphoModel, MorphoAmbiguityResolverModel morphoAmbiguityModel)
        {
            _morphoModel = morphoModel;
            _morphoAnalyzer = new MorphoAnalyzer(_morphoModel);
            _morphoAmbiguityPreProcessor = new MorphoAmbiguityPreProcessor();
            _morphoAmbiguityResolver = new MorphoAmbiguityResolver(morphoAmbiguityModel);
            _wordFormMorphologies_Buffer = new List<WordFormMorphology>();
            _wordMorphoAmbiguityFactory = new WordMorphoAmbiguityFactory(null);
            _wordMorphoAmbiguities = new List<WordMorphoAmbiguity>();
            CTM = XlatUnsafe.Inst._CHARTYPE_MAP;
        }

        public void Dispose()
        {
            _morphoAmbiguityResolver.Dispose();
        }

        unsafe public void Run(List<Word> words
#if DEBUG
            , bool applyMorphoAmbiguityPreProcess
#endif
            )
        {
            var wordMorphology = default(WordMorphology);
            for (int i = 0, wordsLength = words.Count; i < wordsLength; i++)
            {
                var word = words[i];

                if (word.posTaggerExtraWordType != PosTaggerExtraWordType.__DEFAULT__)
                {
                    _wordMorphoAmbiguities.Add(_wordMorphoAmbiguityFactory.Create(word, i));
                    continue;
                }

                switch (word.posTaggerInputType)
                {
                    case PosTaggerInputType.O:      // = "O"; // Другой
                    case PosTaggerInputType.AllLat: // - только латиница: нет строчных и точек;
                    case PosTaggerInputType.FstC:   // - первая заглавная, не содержит пробелов;
                        {
                            if (word.valueUpper == null)
                            {
                                _wordMorphoAmbiguities.Add(_wordMorphoAmbiguityFactory.Create(word, i));
                                continue;
                            }

                            var mode = GetWordFormMorphologyMode(word, i);
                            wordMorphology = _morphoAnalyzer.GetWordMorphology_NoToUpper(word.valueUpper, mode);
                        }
                        break;

                    case PosTaggerInputType.Num:    // – содержит хотя бы одну цифру и не содержит букв;
                        {
                            if (word.posTaggerLastValueUpperInNumeralChain == null)
                            {
                                _wordMorphoAmbiguities.Add(_wordMorphoAmbiguityFactory.Create(word, i));
                                continue;
                            }

                            var mode = GetWordFormMorphologyMode(word, i);
                            wordMorphology = _morphoAnalyzer.GetWordMorphology_4LastValueUpperInNumeralChain(
                                                        word.posTaggerLastValueUpperInNumeralChain, mode);
                        }
                        break;

                    default:
                        _wordMorphoAmbiguities.Add(_wordMorphoAmbiguityFactory.Create(word, i));
                        continue;
                }

                #region  post process MorphoAnalyze result
                if (wordMorphology.HasWordFormMorphologies)
                {
                    var wfms = default(WordFormMorphology[]);
                    //Если данное слово имеет только одну часть речи, прописанную в морфословаре, то использовать ее вместо определённой с помощью PoS-tagger.
                    if (wordMorphology.IsSinglePartOfSpeech)
                    {
                        CorrectPosTaggerOutputType(word, wordMorphology.PartOfSpeech);
                        wfms = wordMorphology.WordFormMorphologies.ToArray();
                        _wordMorphoAmbiguities.Add(_wordMorphoAmbiguityFactory.Create(word, i, wfms));
                    }
                    else
                    {
                        #region clause #1
                        //для данного слова в морфословаре определено несколько частей речи. 
                        //ищем среди морфоинформации по слову морфоинформацию по части речи от pos-tagger'а, если она есть - берем её

                        //вот эта хуйня из-за двойной трансляции 
                        //{PosTaggerOutputType::AdjectivePronoun => PartOfSpeechEnum::Adjective, PartOfSpeechEnum::Pronoun} 
                        // & 
                        //{PosTaggerOutputType::AdverbialPronoun => PartOfSpeechEnum::Adverb, PartOfSpeechEnum::Pronoun}
                        var partOfSpeech = default(PartOfSpeechEnum?);
                        switch (word.posTaggerOutputType)
                        {
                            case PosTaggerOutputType.AdjectivePronoun:
                                {
                                    wfms = TryGetByPosTaggerOutputType(ref wordMorphology, PartOfSpeechEnum.Adjective, PartOfSpeechEnum.Pronoun);
                                    if (wfms != null)
                                    {
                                        _wordMorphoAmbiguities.Add(_wordMorphoAmbiguityFactory.Create(word, i, wfms));
                                        continue;
                                    }
                                }
                                break;

                            case PosTaggerOutputType.AdverbialPronoun:
                                {
                                    wfms = TryGetByPosTaggerOutputType(ref wordMorphology, PartOfSpeechEnum.Adverb, PartOfSpeechEnum.Pronoun);
                                    if (wfms != null)
                                    {
                                        _wordMorphoAmbiguities.Add(_wordMorphoAmbiguityFactory.Create(word, i, wfms));
                                        continue;
                                    }
                                }
                                break;

                            default:
                                {
                                    partOfSpeech = ToPartOfSpeech(word.posTaggerOutputType);
                                    if (partOfSpeech.HasValue)
                                    {
                                        wfms = TryGetByPosTaggerOutputType(ref wordMorphology, partOfSpeech.Value);
                                        if (wfms != null)
                                        {
                                            _wordMorphoAmbiguities.Add(_wordMorphoAmbiguityFactory.Create(word, i, wfms));
                                            continue;
                                        }
                                    }
                                }
                                break;
                        }
                        #endregion

                        #region clause #2
                        //При этом  для данного слова в морфословаре определено несколько частей речи.  
                        //В данном случае в первую очередь м.б. соответствия (слева выход PoS-tagger, справа морфословарь, последовательность пунктов неважна):
                        switch (word.posTaggerOutputType)
                        {
                            /*2.3. Participle = Adjective */
                            case PosTaggerOutputType.Participle:
                                wfms = TryGetByPosTaggerOutputType(ref wordMorphology, PartOfSpeechEnum.Adjective);
                                break;

                            default:
                                #region clause #3
                                {
                                    if (partOfSpeech.HasValue)
                                    {
                                        switch (partOfSpeech.Value)
                                        {
                                            /*3.1. Pronoun = Noun*/
                                            case PartOfSpeechEnum.Pronoun:
                                                wfms = TryGetByPosTaggerOutputType(ref wordMorphology, PartOfSpeechEnum.Noun);
                                                break;

                                            /*3.2. Noun = Pronoun */
                                            case PartOfSpeechEnum.Noun:
                                                wfms = TryGetByPosTaggerOutputType(ref wordMorphology, PartOfSpeechEnum.Pronoun);
                                                break;

                                            /*3.3. Conjunction = Particle */
                                            case PartOfSpeechEnum.Conjunction:
                                                wfms = TryGetByPosTaggerOutputType(ref wordMorphology, PartOfSpeechEnum.Particle);
                                                break;

                                            /*3.4. Particle = Conjunction*/
                                            case PartOfSpeechEnum.Particle:
                                                wfms = TryGetByPosTaggerOutputType(ref wordMorphology, PartOfSpeechEnum.Conjunction);
                                                break;

                                            /*3.5. Numeral = Noun, Adjective */
                                            case PartOfSpeechEnum.Numeral:
                                                wfms = TryGetByPosTaggerOutputType(ref wordMorphology, PartOfSpeechEnum.Noun, PartOfSpeechEnum.Adjective);
                                                break;

                                            /*3.6. Adjective = Verb, Adverb*/
                                            case PartOfSpeechEnum.Adjective:
                                                wfms = TryGetByPosTaggerOutputType(ref wordMorphology, PartOfSpeechEnum.Verb, PartOfSpeechEnum.Adverb);
                                                break;

                                            /*3.7. Adverb = Adjective */
                                            case PartOfSpeechEnum.Adverb:
                                                wfms = TryGetByPosTaggerOutputType(ref wordMorphology, PartOfSpeechEnum.Adjective);
                                                break;
                                        }
                                    }
                                }
                                #endregion
                                break;
                        }

                        /*Если таковых соответствий не нашлось, то берется первая из выдачи морфословаря часть речи.*/
                        if (wfms == null)
                        {
                            var _partOfSpeech = wordMorphology.WordFormMorphologies[0].PartOfSpeech;
                            word.posTaggerOutputType = ToPosTaggerOutputType(_partOfSpeech);
                            wfms = TryGetByPosTaggerOutputType(ref wordMorphology, _partOfSpeech);
                        }

                        _wordMorphoAmbiguities.Add(_wordMorphoAmbiguityFactory.Create(word, i, wfms));

                        #endregion
                    }
                }
                else
                {
                    _wordMorphoAmbiguities.Add(_wordMorphoAmbiguityFactory.Create(word, i));
                }
                #endregion
            }

#if DEBUG
            if ( applyMorphoAmbiguityPreProcess )
            {
#endif
            _morphoAmbiguityPreProcessor.Run(_wordMorphoAmbiguities);
#if DEBUG
            }
#endif
            _morphoAmbiguityResolver.Resolve(_wordMorphoAmbiguities);

            _wordMorphoAmbiguities.Clear();
        }

        private WordFormMorphology[] TryGetByPosTaggerOutputType(ref WordMorphology wordMorphology, PartOfSpeechEnum filterPartOfSpeech)
        {
            if ((wordMorphology.PartOfSpeech & filterPartOfSpeech) == filterPartOfSpeech)
            {
                var morphologies = TryGetByPosTaggerOutputType(wordMorphology.WordFormMorphologies, filterPartOfSpeech);
                return morphologies;
            }

            return null;
        }
        private WordFormMorphology[] TryGetByPosTaggerOutputType(ref WordMorphology wordMorphology,
            PartOfSpeechEnum filterPartOfSpeech_1, PartOfSpeechEnum filterPartOfSpeech_2)
        {
            if ((wordMorphology.PartOfSpeech & filterPartOfSpeech_1) == filterPartOfSpeech_1)
            {
                var morphologies = TryGetByPosTaggerOutputType(wordMorphology.WordFormMorphologies, filterPartOfSpeech_1);
                if (morphologies != null)
                {
                    return morphologies;
                }
            }

            if ((wordMorphology.PartOfSpeech & filterPartOfSpeech_2) == filterPartOfSpeech_2)
            {
                var morphologies = TryGetByPosTaggerOutputType(wordMorphology.WordFormMorphologies, filterPartOfSpeech_2);

                return morphologies;
            }

            return null;
        }
        private WordFormMorphology[] TryGetByPosTaggerOutputType(List<WordFormMorphology> wordFormMorphologies, PartOfSpeechEnum filterPartOfSpeech)
        {
            _wordFormMorphologies_Buffer.Clear();

            for (int i = 0, len = wordFormMorphologies.Count; i < len; i++)
            {
                var wordFormMorphology = wordFormMorphologies[i];
                if ((wordFormMorphology.PartOfSpeech & filterPartOfSpeech) == filterPartOfSpeech)
                {
                    _wordFormMorphologies_Buffer.Add(wordFormMorphology);
                }
            }

            if (_wordFormMorphologies_Buffer.Count != 0)
            {
                var result = _wordFormMorphologies_Buffer.ToArray();
                return result;
            }
            return null;
        }

        #region table {PartOfSpeechEnum} <=> {PosTaggerOutputType}
        /*
        морфоанализатор::{PartOfSpeechEnum}  PoS-tagger::{PosTaggerOutputType}
        PartOfSpeechEnum.Adjective	    PosTaggerOutputType.Adjective
                                        PosTaggerOutputType.AdjectivePronoun
        PartOfSpeechEnum.Adverb	        PosTaggerOutputType.Adverb
                                        PosTaggerOutputType.AdverbialPronoun
        PartOfSpeechEnum.Article	    PosTaggerOutputType.Article
        PartOfSpeechEnum.Conjunction	PosTaggerOutputType.Conjunction
        PartOfSpeechEnum.Interjection	PosTaggerOutputType.Interjection
        PartOfSpeechEnum.Noun	        PosTaggerOutputType.Noun
        PartOfSpeechEnum.Numeral	    PosTaggerOutputType.Numeral
        PartOfSpeechEnum.Other	        PosTaggerOutputType.Other
        PartOfSpeechEnum.Particle	    PosTaggerOutputType.Particle
        PartOfSpeechEnum.Predicate	    PosTaggerOutputType.Predicate
        PartOfSpeechEnum.Preposition	PosTaggerOutputType.Preposition
        PartOfSpeechEnum.Pronoun	    PosTaggerOutputType.Pronoun 
	                                    PosTaggerOutputType.PossessivePronoun
	                                    PosTaggerOutputType.AdjectivePronoun  
	                                    PosTaggerOutputType.AdverbialPronoun
        PartOfSpeechEnum.Verb	        PosTaggerOutputType.Verb
	                                    PosTaggerOutputType.Infinitive
	                                    PosTaggerOutputType.AdverbialParticiple
	                                    PosTaggerOutputType.AuxiliaryVerb
	                                    PosTaggerOutputType.Participle
        -	                            PosTaggerOutputType.Punctuation
        */
        #endregion

        private static void CorrectPosTaggerOutputType(Word word, PartOfSpeechEnum singlePartOfSpeech)
        {
            switch (singlePartOfSpeech)
            {
                case PartOfSpeechEnum.Adjective:
                    switch (word.posTaggerOutputType)
                    {
                        case PosTaggerOutputType.Adjective:
                        case PosTaggerOutputType.AdjectivePronoun:
                            break;
                        default:
                            word.posTaggerOutputType = PosTaggerOutputType.Adjective;
                            break;
                    }
                    break;
                case PartOfSpeechEnum.Adverb:
                    switch (word.posTaggerOutputType)
                    {
                        case PosTaggerOutputType.Adverb:
                        case PosTaggerOutputType.AdverbialPronoun:
                            break;
                        default:
                            word.posTaggerOutputType = PosTaggerOutputType.Adverb;
                            break;
                    }
                    break;
                case PartOfSpeechEnum.Article: word.posTaggerOutputType = PosTaggerOutputType.Article; break;
                case PartOfSpeechEnum.Conjunction: word.posTaggerOutputType = PosTaggerOutputType.Conjunction; break;
                case PartOfSpeechEnum.Interjection: word.posTaggerOutputType = PosTaggerOutputType.Interjection; break;
                case PartOfSpeechEnum.Noun: word.posTaggerOutputType = PosTaggerOutputType.Noun; break;
                case PartOfSpeechEnum.Numeral: word.posTaggerOutputType = PosTaggerOutputType.Numeral; break;
                case PartOfSpeechEnum.Other: word.posTaggerOutputType = PosTaggerOutputType.Other; break;
                case PartOfSpeechEnum.Particle: word.posTaggerOutputType = PosTaggerOutputType.Particle; break;
                case PartOfSpeechEnum.Predicate: word.posTaggerOutputType = PosTaggerOutputType.Predicate; break;
                case PartOfSpeechEnum.Preposition: word.posTaggerOutputType = PosTaggerOutputType.Preposition; break;
                case PartOfSpeechEnum.Pronoun:
                    switch (word.posTaggerOutputType)
                    {
                        case PosTaggerOutputType.Pronoun:
                        case PosTaggerOutputType.PossessivePronoun:
                        case PosTaggerOutputType.AdjectivePronoun:
                        case PosTaggerOutputType.AdverbialPronoun:
                            break;
                        default:
                            word.posTaggerOutputType = PosTaggerOutputType.Pronoun;
                            break;
                    }
                    break;
                case PartOfSpeechEnum.Verb:
                    switch (word.posTaggerOutputType)
                    {
                        case PosTaggerOutputType.Verb:
                        case PosTaggerOutputType.Infinitive:
                        case PosTaggerOutputType.AdverbialParticiple:
                        case PosTaggerOutputType.AuxiliaryVerb:
                        case PosTaggerOutputType.Participle:
                            break;
                        default:
                            word.posTaggerOutputType = PosTaggerOutputType.Verb;
                            break;
                    }
                    break;

                default:
                    throw new ArgumentException(singlePartOfSpeech.ToString());
            }
        }
        private static PosTaggerOutputType ToPosTaggerOutputType(PartOfSpeechEnum singlePartOfSpeech)
        {
            switch (singlePartOfSpeech)
            {
                case PartOfSpeechEnum.Adjective: return (PosTaggerOutputType.Adjective);
                case PartOfSpeechEnum.Adverb: return (PosTaggerOutputType.Adverb);
                case PartOfSpeechEnum.Article: return (PosTaggerOutputType.Article);
                case PartOfSpeechEnum.Conjunction: return (PosTaggerOutputType.Conjunction);
                case PartOfSpeechEnum.Interjection: return (PosTaggerOutputType.Interjection);
                case PartOfSpeechEnum.Noun: return (PosTaggerOutputType.Noun);
                case PartOfSpeechEnum.Numeral: return (PosTaggerOutputType.Numeral);
                case PartOfSpeechEnum.Other: return (PosTaggerOutputType.Other);
                case PartOfSpeechEnum.Particle: return (PosTaggerOutputType.Particle);
                case PartOfSpeechEnum.Predicate: return (PosTaggerOutputType.Predicate);
                case PartOfSpeechEnum.Preposition: return (PosTaggerOutputType.Preposition);
                case PartOfSpeechEnum.Pronoun: return (PosTaggerOutputType.Pronoun);
                case PartOfSpeechEnum.Verb: return (PosTaggerOutputType.Verb);

                default:
                    throw new ArgumentException(singlePartOfSpeech.ToString());
            }
        }
        internal static PartOfSpeechEnum? ToPartOfSpeech(PosTaggerOutputType posTaggerOutputType)
        {
            switch (posTaggerOutputType)
            {
                case PosTaggerOutputType.Adjective:
                    return PartOfSpeechEnum.Adjective;
                case PosTaggerOutputType.Adverb:
                    return PartOfSpeechEnum.Adverb;
                case PosTaggerOutputType.Article: return PartOfSpeechEnum.Article;
                case PosTaggerOutputType.Conjunction: return PartOfSpeechEnum.Conjunction;
                case PosTaggerOutputType.Interjection: return PartOfSpeechEnum.Interjection;
                case PosTaggerOutputType.Noun: return PartOfSpeechEnum.Noun;
                case PosTaggerOutputType.Numeral: return PartOfSpeechEnum.Numeral;
                case PosTaggerOutputType.Other: return PartOfSpeechEnum.Other;
                case PosTaggerOutputType.Particle: return PartOfSpeechEnum.Particle;
                case PosTaggerOutputType.Predicate: return PartOfSpeechEnum.Predicate;
                case PosTaggerOutputType.Preposition: return PartOfSpeechEnum.Preposition;

                case PosTaggerOutputType.Pronoun:
                case PosTaggerOutputType.PossessivePronoun:
                case PosTaggerOutputType.AdjectivePronoun:
                case PosTaggerOutputType.AdverbialPronoun: return PartOfSpeechEnum.Pronoun;

                case PosTaggerOutputType.Verb:
                case PosTaggerOutputType.Infinitive:
                case PosTaggerOutputType.AdverbialParticiple:
                case PosTaggerOutputType.AuxiliaryVerb:
                case PosTaggerOutputType.Participle: return PartOfSpeechEnum.Verb;
            }

            return null;
        }

        #region description
        /*
        это на этапе морфо+теггер до снятия неоднозначности

        В случае наличия нескольких вариантов нормализации с разным положением регистра отбор кандидата  производить следующим образом:
        - если слово написано с [_не_заглавной_] буквы и это часть речи      NOUN; ADJECTIVE; ADVERB , то отбирать [_все_] варианты;
        - если слово написано с [_не_заглавной_] буквы и это часть речи _не_ NOUN; ADJECTIVE; ADVERB , то отбирать варианты с [_не_заглавной_] буквы;
        - если слово написано с [_Заглавной_]    буквы и это {_первое_} слово в предложении, то отбирать [_все_] варианты;
        - если слово написано с [_Заглавной_]    буквы и это часть речи      NOUN; ADJECTIVE; ADVERB и {_не_первое_} слово в предложении, то отбирать с [_заглавной_] буквы;
        - если слово написано с [_Заглавной_]    буквы и это часть речи _не_ NOUN; ADJECTIVE; ADVERB и {_не_первое_} слово в предложении, то отбирать [_все_] варианты;
        */
        #endregion

        private static WordFormMorphologyModeEnum GetWordFormMorphologyMode(Word word, int wordindex)
        {
            if (wordindex == 0)
            {
                return WordFormMorphologyModeEnum.Default;
            }

            if (word.posTaggerFirstCharIsUpper)
            {
                switch (word.posTaggerOutputType)
                {
                    case PosTaggerOutputType.Noun:
                    case PosTaggerOutputType.Adjective:
                    case PosTaggerOutputType.Adverb:
                        return WordFormMorphologyModeEnum.FirstStartsWithUpperAfterLowerLetter;

                    default:
                        return WordFormMorphologyModeEnum.Default;
                }
            }
            else
            {
                return WordFormMorphologyModeEnum.FirstStartsWithLowerAfterUpperLetter;
            }
        }
    }
}
