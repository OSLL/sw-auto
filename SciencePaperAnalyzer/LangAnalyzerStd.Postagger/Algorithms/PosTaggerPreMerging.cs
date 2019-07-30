using System.Collections.Generic;

using LangAnalyzerStd.Core;
using LangAnalyzerStd.Tokenizing;

namespace LangAnalyzerStd.Postagger
{
    public sealed class PosTaggerPreMerging
    {
        private const char SPACE = ' ';
        private readonly PosTaggerResourcesModel _model;

        public PosTaggerPreMerging(PosTaggerResourcesModel model)
        {
            _model = model;
        }

        public void Run(List<Word> words)
        {
            MergePhrases(words);

            MergeAbbreviations(words);

            MergeNumbers(words);
        }

        private void MergePhrases(List<Word> words)
        {
            var phrases = _model.PhrasesSearcher.FindAllIngnoreCase(words);

            if (phrases == null)
                return;

            foreach (var ss in phrases)
            {
                var w1 = words[ss.StartIndex];
                if (w1.posTaggerInputType == PosTaggerInputType.CompPh)
                {
                    continue;
                }

                switch (ss.Length)
                {
                    case 2:
                        {
                            var w2 = words[ss.StartIndex + 1];

                            w1.posTaggerInputType = PosTaggerInputType.CompPh;
                            w1.valueUpper = string.Concat(w1.valueUpper, SPACE, w2.valueUpper);
                            w1.valueOriginal = string.Concat(w1.valueOriginal, SPACE, w2.valueOriginal);
                            w1.length = w2.startIndex - w1.startIndex + w2.length;
                            w2.valueUpper = w2.valueOriginal = null;
                        }
                        break;

                    case 3:
                        {
                            var w2 = words[ss.StartIndex + 1];
                            var w3 = words[ss.StartIndex + 2];

                            w1.posTaggerInputType = PosTaggerInputType.CompPh;
                            w1.valueUpper = string.Concat(w1.valueUpper, SPACE, w2.valueUpper, SPACE, w3.valueUpper);
                            w1.valueOriginal = string.Concat(w1.valueOriginal, SPACE, w2.valueOriginal, SPACE, w3.valueOriginal);
                            w1.length = w3.startIndex - w1.startIndex + w3.length;
                            w2.valueUpper = w2.valueOriginal = null;
                            w3.valueUpper = w3.valueOriginal = null;
                        }
                        break;

                    case 4:
                        {
                            var w2 = words[ss.StartIndex + 1];
                            var w3 = words[ss.StartIndex + 2];
                            var w4 = words[ss.StartIndex + 3];

                            w1.posTaggerInputType = PosTaggerInputType.CompPh;
                            w1.valueUpper = string.Concat(w1.valueUpper, SPACE, w2.valueUpper, SPACE, w3.valueUpper, SPACE, w4.valueUpper);
                            w1.valueOriginal = string.Concat(w1.valueOriginal, SPACE, w2.valueOriginal, SPACE, w3.valueOriginal, SPACE, w4.valueOriginal);
                            w1.length = w4.startIndex - w1.startIndex + w4.length;
                            w2.valueUpper = w2.valueOriginal = null;
                            w3.valueUpper = w3.valueOriginal = null;
                            w4.valueUpper = w4.valueOriginal = null;
                        }
                        break;

                    default:
                        {
                            var w = default(Word);
                            for (int i = ss.StartIndex + 1, j = ss.Length; 1 < j; j--)
                            {
                                w = words[i];
                                w1.valueUpper = string.Concat(w1.valueUpper, SPACE, w.valueUpper);
                                w1.valueOriginal = string.Concat(w1.valueOriginal, SPACE, w.valueOriginal);
                                w.valueUpper = w.valueOriginal = null;
                            }
                            w1.posTaggerInputType = PosTaggerInputType.CompPh;
                            w1.length = w.startIndex - w1.startIndex + w.length;
                        }
                        break;
                }
            }

            for (int i = words.Count - 1; 0 <= i; i--)
            {
                var w = words[i];
                if (w.valueOriginal == null)
                {
                    words.RemoveAt(i);
                }
            }
        }
        private void MergeAbbreviations(List<Word> words)
        {
            var abbreviations = _model.AbbreviationsSearcher.FindAllSensitiveCase(words);

            if (abbreviations == null)
                return;

            foreach (var ss in abbreviations)
            {
                var w1 = words[ss.StartIndex];
                if ((w1.posTaggerInputType == PosTaggerInputType.CompPh) ||
                     (w1.posTaggerExtraWordType == PosTaggerExtraWordType.Abbreviation))
                {
                    continue;
                }

                switch (ss.Length)
                {
                    case 2:
                        {
                            var w2 = words[ss.StartIndex + 1];

                            w1.posTaggerInputType = PosTaggerInputType.O;
                            w1.posTaggerExtraWordType = PosTaggerExtraWordType.Abbreviation;
                            w1.valueUpper = string.Concat(w1.valueUpper, w2.valueUpper);
                            w1.valueOriginal = string.Concat(w1.valueOriginal, w2.valueOriginal);
                            w1.length = w2.startIndex - w1.startIndex + w2.length;
                            w2.valueUpper = w2.valueOriginal = null;
                        }
                        break;

                    case 3:
                        {
                            var w2 = words[ss.StartIndex + 1];
                            var w3 = words[ss.StartIndex + 2];

                            w1.posTaggerInputType = PosTaggerInputType.O;
                            w1.posTaggerExtraWordType = PosTaggerExtraWordType.Abbreviation;
                            w1.valueUpper = string.Concat(w1.valueUpper, w2.valueUpper, w3.valueUpper);
                            w1.valueOriginal = string.Concat(w1.valueOriginal, w2.valueOriginal, w3.valueOriginal);
                            w1.length = w3.startIndex - w1.startIndex + w3.length;
                            w2.valueUpper = w2.valueOriginal = null;
                            w3.valueUpper = w3.valueOriginal = null;
                        }
                        break;

                    case 4:
                        {
                            var w2 = words[ss.StartIndex + 1];
                            var w3 = words[ss.StartIndex + 2];
                            var w4 = words[ss.StartIndex + 3];

                            w1.posTaggerInputType = PosTaggerInputType.O;
                            w1.posTaggerExtraWordType = PosTaggerExtraWordType.Abbreviation;
                            w1.valueUpper = string.Concat(w1.valueUpper, w2.valueUpper, w3.valueUpper, w4.valueUpper);
                            w1.valueOriginal = string.Concat(w1.valueOriginal, w2.valueOriginal, w3.valueOriginal, w4.valueOriginal);
                            w1.length = w4.startIndex - w1.startIndex + w4.length;
                            w2.valueUpper = w2.valueOriginal = null;
                            w3.valueUpper = w3.valueOriginal = null;
                            w4.valueUpper = w4.valueOriginal = null;
                        }
                        break;

                    default:
                        {
                            var w = default(Word);
                            for (int i = ss.StartIndex + 1, j = ss.Length; 1 < j; j--)
                            {
                                w = words[i];
                                w1.valueUpper = string.Concat(w1.valueUpper, w.valueUpper);
                                w1.valueOriginal = string.Concat(w1.valueOriginal, w.valueOriginal);
                                w.valueUpper = w.valueOriginal = null;
                            }
                            w1.posTaggerInputType = PosTaggerInputType.O;
                            w1.posTaggerExtraWordType = PosTaggerExtraWordType.Abbreviation;
                            w1.length = w.startIndex - w1.startIndex + w.length;
                        }
                        break;
                }
            }

            for (int i = words.Count - 1; 0 <= i; i--)
            {
                var w = words[i];
                if (w.valueOriginal == null)
                {
                    words.RemoveAt(i);
                }
            }
        }
        private void MergeNumbers(List<Word> words)
        {
            #region description
            /*
            2.	числительных, в том числе написанных прописью, а также слов, обозначающих счетные множества  (список numbers.txt); 
              числительные могут включать в себя:
            i.	дефис, точку, запятую, двоеточие;  -  (в середине)
            ii.	любое сокращение из списка (список abr.txt) – (в середине и в конце)
            iii.	числительные с дефисом и окончанием (строго после цифры – дефис – слово, состоящие только из букв, все без пробелов)
            //---commented---iv.	предлоги перед числительным или сокращением из списка abr.txt: от, до, на, с, по, из - (в начале и в середине)
            iv.	предлоги или и сокращением из списка abr.txt в середине между числительным
            v.	союз «и» - (в середине)     
            */
            #endregion

            for (int i = 0, len_minus_1 = words.Count - 1; i <= len_minus_1; i++)
            {
                var w1 = words[i];
                if (w1.posTaggerInputType == PosTaggerInputType.Num)
                {
                    for (var j = i + 1; j <= len_minus_1; j++)
                    {
                        //i. дефис, точку, запятую, двоеточие
                        var w = words[j];
                        switch (w.posTaggerInputType)
                        {
                            case PosTaggerInputType.Num:
                                #region
                                {
                                    w1.valueOriginal = string.Concat(w1.valueOriginal, SPACE, w.valueOriginal);
                                    w1.valueUpper = string.Concat(w1.valueUpper, SPACE, w.valueUpper); //null;
                                    w1.length = (w.startIndex - w1.startIndex) + w.length;
                                    w1.posTaggerLastValueUpperInNumeralChain = w.posTaggerLastValueUpperInNumeralChain;
                                    words.RemoveAt(j);
                                    len_minus_1--;
                                    j--;
                                    #region [.commented. attach previous pretext.]
                                    /*hasNumChain = true;*/
                                    #endregion
                                }
                                #endregion
                                break;

                            case PosTaggerInputType.Col:
                            case PosTaggerInputType.Com:
                            case PosTaggerInputType.Dash:
                                #region
                                {
                                    if ((j != len_minus_1) &&
                                         (words[j + 1].posTaggerInputType == PosTaggerInputType.Num))
                                    {
                                        w1.valueOriginal = string.Concat(w1.valueOriginal, SPACE, w.valueOriginal);
                                        w1.valueUpper = string.Concat(w1.valueUpper, SPACE, w.valueUpper); //null;
                                        w1.length = (w.startIndex - w1.startIndex) + w.length;
                                        words.RemoveAt(j);
                                        len_minus_1--;
                                        j--;
                                    }
                                    else
                                    {
                                        goto NEXT_NUM;
                                    }
                                }
                                #endregion
                                break;

                            default:
                                #region
                                {
                                    if (w.posTaggerExtraWordType == PosTaggerExtraWordType.Abbreviation)
                                    {
                                        w1.valueOriginal = string.Concat(w1.valueOriginal, SPACE, w.valueOriginal);
                                        w1.valueUpper = string.Concat(w1.valueUpper, SPACE, w.valueUpper); //null;
                                        w1.length = (w.startIndex - w1.startIndex) + w.length;
                                        w1.posTaggerLastValueUpperInNumeralChain = null;
                                        words.RemoveAt(j);
                                        len_minus_1--;
                                        j--;
                                    }
                                    else
                                    if ( //(w.valueUpper != null) &&
                                         (Xlat.IsDot(w.valueUpper[0]) ||
                                          (w.valueUpper == PosTaggerResourcesModel.UNION_AND) ||
                                          PosTaggerResourcesModel.Pretexts.Contains(w.valueUpper)
                                         )
                                       )
                                    {
                                        if ((j != len_minus_1) &&
                                             (words[j + 1].posTaggerInputType == PosTaggerInputType.Num))
                                        {
                                            w1.valueOriginal = string.Concat(w1.valueOriginal, SPACE, w.valueOriginal);
                                            w1.valueUpper = string.Concat(w1.valueUpper, SPACE, w.valueUpper); //null;
                                            w1.length = (w.startIndex - w1.startIndex) + w.length;
                                            words.RemoveAt(j);
                                            len_minus_1--;
                                            j--;
                                        }
                                        else
                                        {
                                            goto NEXT_NUM;
                                        }
                                    }
                                    else
                                    {
                                        goto NEXT_NUM;
                                    }
                                }
                                #endregion
                                break;
                        }
                    }

                NEXT_NUM:;
                }
            }
        }
    }
}
