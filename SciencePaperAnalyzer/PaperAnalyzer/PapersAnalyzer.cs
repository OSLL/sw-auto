using LangAnalyzerStd.Morphology;
using LangAnalyzerStd.Postagger;
using System;
using System.Collections.Generic;
using System.Linq;
using AnalyzeResults.Presentation;
using SentenceHtml = AnalyzeResults.Presentation.Sentence;
using WordHtml = AnalyzeResults.Presentation.Word;
using Word = LangAnalyzerStd.Tokenizing.Word;
using System.Text.RegularExpressions;
using AnalyzeResults.Errors;
using System.Text;
using AnalyzeResults.Settings;
using DocumentFormat.OpenXml.Drawing;
using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;

namespace PaperAnalyzer
{
    /// <summary>
    /// Analyses paper's text
    /// </summary>
    public sealed class PapersAnalyzer : IPaperAnalyzer
    {
        private readonly IPaperAnalyzerEnvironment _environment;

        private static readonly List<string> personalPronouns = new List<string>
        {
            "я", "мой", "мое", "мои", "моё", "моя", "мы", "наш", "наше", "наши", "нашу", "ты", "твой", "твое", "твоё", "твоя"
        };
        private readonly IConfiguration _appConfig;
        public PapersAnalyzer(IPaperAnalyzerEnvironment environment, IConfiguration appConfig = null)
        {
            _environment = environment;
            _appConfig = appConfig;
        }

        private Criterion CreateKeywordMentioningCriterion(Dictionary<string, List<int>> keywordMarks, Dictionary<string, Word[]> keywordDict, ResultScoreSettings settings)
        {
            if (keywordDict.Count > 0)
            {
                var missingKeywords = keywordMarks.Where((kv) => kv.Value.Count == 0).ToList();
                var isSatisfied = missingKeywords.Count == 0;
                var keyWordsReport = new StringBuilder();
                foreach (var kv in missingKeywords)
                {
                    keyWordsReport.Append($"{kv.Key}: отсутствует\n");
                }
                return new BooleanCriterion("Упоминание ключевых слов", isSatisfied, settings.KeywordsMentioning.Weight,
                            "Каждое ключевое слово из списка должно быть упомянуто хотя бы один раз в тексте работы.",
                            $"{keyWordsReport}");
            }
            else
            {
                return new BooleanCriterion("Упоминание ключевых слов", false, settings.KeywordsMentioning.Weight,
                            "Каждое ключевое слово из списка должно быть упомянуто хотя бы один раз в тексте работы.",
                            $"Список ключевых слов должен быть определен в тексте работы.");
            }
        }
        private Dictionary<string, Word[]> PrepareKeywordsDict(string keywords, char delimiter = ',')
        {
            Dictionary<string, Word[]> result = new Dictionary<string, Word[]>();
            foreach (string keyword in keywords.Split(new Char[] { delimiter, '\n' }))
            {
                string key = keyword.Trim().ToLower();
                if (!string.IsNullOrEmpty(key) && !result.ContainsKey(key) && (key.Length > 2 || (key.Length > 1 && keyword.ToUpper() == keyword)))
                {
                    List<Word[]> analysisResult = _environment.Processor.RunFullAnalysis(key, true, true, true, true);
                    List<Word> keywordWords = new List<Word>();
                    foreach (var r in analysisResult)
                    {
                        keywordWords.AddRange(r);
                    }
                    if (keyword.ToUpper() != keyword)
                    {
                        result.Add(key, keywordWords.ToArray());
                    }
                    else
                    {
                        result.Add(keyword, keywordWords.ToArray());
                    }
                }
            }
            return result;
        }

        private Dictionary<string, List<int>> MarkKeywordInSentence(Dictionary<string, Word[]> keywords, Word[] sentence)
        {
            Dictionary<string, List<int>> result = new Dictionary<string, List<int>>();
            foreach (KeyValuePair<string, Word[]> keyword in keywords)
            {
                result[keyword.Key] = new List<int>();
            }
            for (var i = 0; i < sentence.Length; i++)
            {
                foreach (KeyValuePair<string, Word[]> keyword in keywords)
                {
                    // temporary list of startindex
                    List<int> siTemp = new List<int>();
                    int offset = 0;
                    foreach (Word w in keyword.Value)
                    {
                        if (i + offset < sentence.Length)
                        {
                            siTemp.Add(sentence[i + offset].startIndex);
                            if (keyword.Key.ToUpper() == keyword.Key)
                            {
                                if (keyword.Key == sentence[i + offset].valueOriginal)
                                {
                                    offset += 1;
                                    continue;
                                }
                                else break;
                            }
                            else
                            if (sentence[i + offset].morphology.IsEmptyNormalForm() || w.morphology.IsEmptyNormalForm())
                            {
                                var common_length = Math.Min(sentence[i + offset].valueOriginal.Length, w.valueOriginal.Length);
                                // compare original value if normal form not available
                                if (common_length > 4 && sentence[i + offset].valueOriginal.ToLower().Substring(0, common_length - 2) != w.valueOriginal.ToLower().Substring(0, common_length - 2))
                                {
                                    break;
                                }
                                else
                                if (common_length <= 4 && sentence[i + offset].valueOriginal != w.valueOriginal)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                // if both normal form are available then try compare them
                                if (sentence[i + offset].morphology.NormalForm.ToLower() != w.morphology.NormalForm.ToLower())
                                {
                                    break;
                                }
                            }
                            offset += 1;
                        }
                        else break;
                    }
                    // if the keyword fully match in string
                    if (offset == keyword.Value.Length)
                    {
                        if (result.ContainsKey(keyword.Key))
                        {
                            result[keyword.Key].AddRange(siTemp);
                        }
                        else
                        {
                            result[keyword.Key] = siTemp;
                        }
                    }
                }
            }
            return result;
        }
        private string TryGetKeywordFromText(ref string text, string section_titles = null)
        {
            string lookBehind = @"Ключевые слова: ";
            string lookForward = String.Empty;
            if (string.IsNullOrEmpty(section_titles))
            {
                // if section titles is not provided, take from keyword until end of line
                lookForward = @"\n";
            }
            else
            {
                // if provided, take until first section's title
                lookForward = $@"#|{string.Join('|', section_titles.Split(new Char[] { ',', '\n' }).Select(x => x.Trim()))}";

            }
            Match m = Regex.Match(text, $@"(?<={lookBehind})[\S\s]*?(?=({lookForward}))");
            if (m.Success)
            {
                text = text.Replace(Regex.Match(text, $@"({lookBehind})[\S\s]*?(?=({lookForward}))").Value, " \n");
                return Regex.Replace(m.Value.Replace('\n', ' ').Replace('\r', ' '), @"\s+", " ");
            }
            return "";
        }
        public PaperAnalysisResult ProcessTextWithResult(
            string text,
            string titlesString,
            string paperName,
            string refsName,
            string keywords,
            ResultScoreSettings settings)
        {
            try
            {
                if (string.IsNullOrEmpty(refsName))
                    refsName = "Список литературы";
                if (string.IsNullOrEmpty(paperName))
                    paperName = "";
                // paper name not provided by user, assume the first line is the title, if a condition is met (paperName=='#auto#') 
                // the condition is added due to the fact that only task 3 (the full paper) should contain the first line as article's name
                // so when sending work for task 3, we either pass article's name manually, or pass the flag for auto detection
                // TODO: move the flag to an outer constant or sth
                bool autoExtractPaperName = paperName.Equals(_appConfig==null?"":_appConfig.GetValue("AutoTitleExtractionToken","#auto#"));
                if (autoExtractPaperName)
                {
                    var assumedPaperName = Regex.Match(text, @"^[\S\s]+?[\s]\n");
                    if (assumedPaperName.Success)
                    {
                        paperName = assumedPaperName.Value;
                        paperName = Regex.Replace(paperName, @"[#\n]", "");
                    }
                }
                Console.OutputEncoding = Encoding.UTF8;
                Console.WriteLine(paperName);
                if (string.IsNullOrEmpty(titlesString))
                    titlesString = "";

                var kwInText = TryGetKeywordFromText(ref text, titlesString);
                // prepare the keyword dictionary and result dictionary for keyword marks
                var keywordDict = PrepareKeywordsDict(kwInText + "," + keywords, ',');
                // same for paper name
                paperName = Regex.Replace(paperName.Replace(",", " , "), @"\s+", " ");
                var paperNameDict = PrepareKeywordsDict(paperName, ' ');
                Console.WriteLine(paperName);


                Dictionary<string, List<int>> keywordMarks = new Dictionary<string, List<int>>();
                Dictionary<string, List<int>> papernameMarks = new Dictionary<string, List<int>>();

                var forbiddenDicts = new List<ForbiddenWordHashSet>();
                // IEnumerable из словарей надо привести к HashSet
                foreach (var dict in settings.ForbiddenWords)
                {
                    var items = new ForbiddenWordHashSet()
                    {
                        Name = dict.Name,
                        Words = dict.Words.ToHashSet()
                    };
                    forbiddenDicts.Add(items);
                }



                var referencesIndex = text.IndexOf(refsName, StringComparison.InvariantCultureIgnoreCase);
                string referencesSection;
                var refSection = new Section
                {
                    Type = SectionType.ReferencesList
                };
                var refNameSection = new Section()
                {
                    Type = SectionType.SectionTitle
                };
                var refNameSentence = new SentenceHtml(SentenceType.Reference)
                {
                    Original = refsName
                };
                refNameSection.Sentences.Add(refNameSentence);

                var referencesList = new List<Reference>();

                if (referencesIndex != -1)
                {
                    referencesSection = text.Substring(referencesIndex);
                    text = text.Remove(referencesIndex);

                    referencesSection = referencesSection.Replace(refsName, "").Trim();
                    var tstRefs = referencesSection.Split("\n");

                    var references = new List<string>();

                    var refStartRegex = new Regex(@"^(([1-9]|[1-9][0-9])\. )");

                    for (int i = 0; i < tstRefs.Length; i++)
                    {
                        var regexResult = refStartRegex.Match(tstRefs[i]);

                        if (regexResult.Success)
                        {
                            if (regexResult.Value == $"{references.Count + 1}. ")
                            {
                                references.Add(tstRefs[i].Trim());
                            }
                            else
                            {
                                var last = references.Last();
                                if (last.Contains($"{references.Count + 1}. "))
                                {
                                    var refs = last.Split($"{references.Count + 1}. ");
                                    references.RemoveAt(references.Count - 1);
                                    references.Add(refs[0].Trim());
                                    references.Add($"{references.Count + 1}. {refs[1].Trim()}");
                                    references.Add(tstRefs[i].Trim());
                                }
                                else
                                {
                                    references.RemoveAt(references.Count - 1);
                                    references.Add(last + tstRefs[i].Trim());
                                }
                            }
                        }
                        else
                        {
                            if (references.Count != 0)
                            {
                                var last = references.Last();
                                references.RemoveAt(references.Count - 1);
                                references.Add(last + tstRefs[i].Trim());
                            }
                        }
                    }

                    var referenceRegex = new Regex(@"(\[([1-9]|[1-9][0-9]|\-|\,)*\])");
                    var refYearRegex = new Regex(@"((19|20)\d{2}\.)");
                    var firstMatches = referenceRegex.Matches(text).Select(x => x.Value.Replace("[", "").Replace("]", "")).Select(x => x.Split(",")).ToList();
                    var matches = new List<string>();
                    foreach (var match in firstMatches)
                    {
                        matches.AddRange(match);
                    }
                    matches = matches.Distinct().ToList();

                    var referenceIndexes = new List<int>();
                    foreach (var match in matches)
                    {
                        if (match.Contains("-"))
                        {
                            var interval = match.Split("-").Select(x => int.Parse(x)).ToList();

                            if (interval.Count != 2)
                                continue;

                            var minNum = interval.Min();
                            var maxNum = interval.Max();

                            for (int i = minNum; i <= maxNum; i++)
                                referenceIndexes.Add(i);
                        }
                        else if(match.Length>0)
                        {
                            var num = int.Parse(match);
                            referenceIndexes.Add(num);
                        }
                    }

                    referenceIndexes = referenceIndexes.Distinct().ToList();

                    foreach (var reference in references)
                    {
                        try
                        {
                            var number = int.Parse(reference.Split(".")[0]);
                            var refSentence = new SentenceHtml(SentenceType.Reference)
                            {
                                Original = reference
                            };
                            refSection.Sentences.Add(refSentence);
                            var year = refYearRegex.Match(refSentence.Original);

                            var referenceToAdd = new Reference(refSentence, number)
                            {
                                ReferedTo = referenceIndexes.Contains(number),
                                Year = year.Success ? int.Parse(year.Value.Replace(".", "")) : 0
                            };
                            referencesList.Add(referenceToAdd);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                text = text.Replace("\n", "");

                var titles = titlesString.Split("\n").Select(x => x.Trim()).ToList();

                //titles.Add(paperName);

                var titleIndex = text.IndexOf(paperName, StringComparison.InvariantCultureIgnoreCase);
                if (titleIndex != -1)
                {
                    text = text.Substring(titleIndex);
                }
                var result = _environment.Processor.RunFullAnalysis(text, true, true, true, true);

                var dictionary = new Dictionary<string, int>();
                var stopDictionary = new Dictionary<string, int>();
                var stopPartsOfSpeech = new List<PartOfSpeechEnum>
                {
                    PartOfSpeechEnum.Article,
                    PartOfSpeechEnum.Conjunction,
                    PartOfSpeechEnum.Interjection,
                    PartOfSpeechEnum.Numeral,
                    PartOfSpeechEnum.Other,
                    PartOfSpeechEnum.Particle,
                    PartOfSpeechEnum.Predicate,
                    PartOfSpeechEnum.Preposition,
                    PartOfSpeechEnum.Pronoun
                };

                var errors = new List<Error>();

                var newResult = new List<Word[]>();

                for (int i = 0; i < result.Count; i++)
                {
                    var sentence = result[i];
                    var tmpSentence = new List<Word>();

                    bool upperCaseStreak = false;
                    foreach (var word in sentence)
                    {
                        if (word.posTaggerExtraWordType == PosTaggerExtraWordType.Punctuation || word.morphology.PartOfSpeech == PartOfSpeechEnum.Numeral)
                        {
                            tmpSentence.Add(word);
                            continue;
                        }

                        if (tmpSentence.Count == 0)
                        {
                            tmpSentence.Add(word);
                            if (word.valueOriginal == word.valueUpper)
                                upperCaseStreak = true;
                            continue;
                        }

                        if (word.valueOriginal == word.valueUpper)
                        {
                            if (upperCaseStreak == true)
                            {
                                tmpSentence.Add(word);
                                continue;
                            }
                            else
                            {
                                upperCaseStreak = true;
                                var newSentence = tmpSentence.ConvertAll(x => x);
                                tmpSentence.Clear();
                                tmpSentence.Add(word);
                                newResult.Add(newSentence.ToArray());
                                continue;
                            }
                        }
                        else
                        {
                            if (upperCaseStreak == true)
                                upperCaseStreak = false;
                            if (word.valueOriginal[0] == word.valueUpper[0]
                                && (word.morphology.PartOfSpeech == PartOfSpeechEnum.Noun && word.morphology.MorphoAttribute == MorphoAttributeEnum.Common
                                    || word.morphology.PartOfSpeech == PartOfSpeechEnum.Adjective
                                    || word.morphology.PartOfSpeech == PartOfSpeechEnum.Verb
                                    || word.morphology.PartOfSpeech == PartOfSpeechEnum.Preposition))
                            {
                                var newSentence = tmpSentence.ConvertAll(x => x);
                                tmpSentence.Clear();
                                tmpSentence.Add(word);
                                newResult.Add(newSentence.ToArray());
                                continue;
                            }
                            else
                            {
                                if (tmpSentence.Count > 0 && tmpSentence.Last().morphology.PartOfSpeech == PartOfSpeechEnum.Preposition)
                                {
                                    var lastWord = tmpSentence.Last();
                                    tmpSentence.RemoveAt(tmpSentence.Count - 1);
                                    var newSentence = tmpSentence.ConvertAll(x => x);
                                    tmpSentence.Clear();
                                    tmpSentence.Add(lastWord);
                                    tmpSentence.Add(word);
                                    newResult.Add(newSentence.ToArray());
                                }
                                else
                                {
                                    tmpSentence.Add(word);
                                    continue;
                                }
                            }
                        }
                    }
                    newResult.Add(tmpSentence.ToArray());
                }

                newResult = newResult.Where(x => x.Length > 0).ToList();

                var titlesTest = newResult.Where(x => x.Last().posTaggerOutputType != PosTaggerOutputType.Punctuation).ToList();

                var sections = new List<Section>();

                var section = new Section();

                // try to get paper name from the text by comparing with user provided papername.
                var paperNameTemp = paperName;
                var paperNameWords = new List<WordHtml>();
                // if user did not provide paper name, paperName will be null and this will be skipped
                while (!string.IsNullOrEmpty(paperNameTemp) && newResult.Count > 0)
                {
                    paperNameTemp = paperNameTemp.Trim();
                    var r = newResult[0];
                    var sentence = new SentenceHtml(SentenceType.Basic, r.Select(x => new WordHtml(x.valueOriginal, x.posTaggerOutputType, x.startIndex)));
                    // compare word by word to avoid mismatch when restore string form of sentencehtml, 
                    // for example, when 3D-сканирования is mistakenly detected as Numerical, there won't be space before it in the string representation
                    bool match = true;
                    foreach (var w in sentence.Words)
                    {
                        if (paperNameTemp.StartsWith(w.Original))
                        {
                            paperNameTemp = paperNameTemp.Substring(w.Original.Length).Trim();
                        }
                        else
                        {
                            // completed extracting title from the text, put the left over of the current "sentence/line" back to the list for further processing
                            if (string.IsNullOrWhiteSpace(paperNameTemp))
                            {
                                Word[] leftover = newResult[0].Skip(sentence.Words.IndexOf(w)).ToArray();
                                newResult[0] = leftover;
                                paperNameWords.AddRange(sentence.Words.Take(sentence.Words.IndexOf(w)));
                            }
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        newResult.RemoveAt(0);
                        paperNameWords.AddRange(sentence.Words);
                    }
                    else
                    {
                        paperNameTemp = string.Empty;
                    }
                }
                // if paper name is found -> add to sections
                if (paperNameWords.Count > 0)
                {
                    section.Type = SectionType.PaperTitle;
                    section.Sentences.Add(new SentenceHtml(SentenceType.Basic, paperNameWords));
                    sections.Add(section);
                    section = new Section();
                }
                else
                {
                    // paper name not provided by user or provided but not found in text 
                    // NOTE: uncomment code below if we want to assume that first sentence is the paper's name
                    // DEPRECATED: newResult[0] does not contain the whole first sentence, instead it contains first phrase (?) 
                    // new implementation of auto extract is above (extract whole first line of the text)
                    //if (newResult.Count > 0) 
                    //{
                    //    section.Type = SectionType.PaperTitle;
                    //    var sentence = new SentenceHtml(SentenceType.Basic, newResult[0].Select(x => new WordHtml(x.valueOriginal, x.posTaggerOutputType, x.startIndex)));
                    //    paperName = sentence.ToStringVersion();
                    //    section.Sentences.Add(sentence);
                    //    sections.Add(section);
                    //    section = new Section();
                    //}
                }


                // processing keywords (mark appearance of keywords and store in the dictionary)
                // loop through r to get keyword in sentence
                var wholeTextWords = newResult.SelectMany(x => x).ToArray();
                var keywordInSentence = MarkKeywordInSentence(keywordDict, wholeTextWords);
                // add sentence's result to text's result
                foreach (KeyValuePair<string, List<int>> kv in keywordInSentence)
                {
                    if (keywordMarks.ContainsKey(kv.Key))
                    {
                        keywordMarks[kv.Key].AddRange(kv.Value);
                    }
                    else
                    {
                        keywordMarks[kv.Key] = kv.Value;
                    }
                }
                // end processing keywords

                // processing paper name (mark appearance of words from paper name and store in the dictionary)
                var papernameInSentence = MarkKeywordInSentence(paperNameDict, wholeTextWords);
                foreach (KeyValuePair<string, List<int>> kv in papernameInSentence)
                {
                    if (papernameMarks.ContainsKey(kv.Key))
                    {
                        papernameMarks[kv.Key].AddRange(kv.Value);
                    }
                    else
                    {
                        papernameMarks[kv.Key] = kv.Value;
                    }
                }
                // end processing paper name

                foreach (var r in newResult)
                {
                    var sentence = new SentenceHtml(SentenceType.Basic, r.Select(x => new WordHtml(x.valueOriginal, x.posTaggerOutputType, x.startIndex)));

                    if (titles.Contains(sentence.ToStringVersion()))
                    {
                        titles.Remove(sentence.ToStringVersion());
                        if (section.Sentences.Count() > 0)
                        {
                            sections.Add(section);
                            section = new Section();
                        }
                        section.Type = SectionType.SectionTitle;
                        section.Sentences.Add(sentence);
                        sections.Add(section);
                        section = new Section();
                        continue;
                    }

                    section.Type = SectionType.Text;
                    section.Sentences.Add(sentence);

                    if (r == newResult.Last())
                        sections.Add(section);

                    foreach (var word in r)
                    {
                        var normalForm = word.morphology.NormalForm;
                        if (string.IsNullOrEmpty(normalForm))
                            continue;
                        if (word.morphology.PartOfSpeech == PartOfSpeechEnum.Pronoun)
                        {
                            if (personalPronouns.Contains(word.morphology.NormalForm))
                                errors.Add(new UseOfPersonalPronounsError(new WordHtml(word.valueOriginal, word.posTaggerOutputType, word.startIndex),
                                    settings.UseOfPersonalPronounsErrorCost, settings.UseOfPersonalPronounsCost, settings.UseOfPersonalPronounsGrading,
                                    settings.UseOfPersonalPronounsGradingType));
                        }

                        if (stopPartsOfSpeech.Contains(word.morphology.PartOfSpeech))
                        {
                            if (stopDictionary.ContainsKey(normalForm))
                                stopDictionary[normalForm]++;
                            else
                                stopDictionary.Add(normalForm, 1);
                        }
                        else
                        {
                            if (dictionary.ContainsKey(normalForm))
                                dictionary[normalForm]++;
                            else
                                dictionary.Add(normalForm, 1);
                        }

                        foreach (var dict in forbiddenDicts)
                        {
                            if (dict.Words.Contains(word.morphology.NormalForm))
                            {
                                errors.Add(new UseOfForbiddenWordsError(dict.Name, new WordHtml(word.valueOriginal, word.posTaggerOutputType, word.startIndex),
                                    settings.ForbiddenWordsErrorCost, settings.ForbiddenWordsCost,
                                    settings.ForbiddenWordsGrading, settings.ForbiddenWordsGradingType));
                            }
                        }
                    }
                }

                refSection.References = referencesList;

                if (refSection.References.Count() > 0)
                {
                    sections.Add(refNameSection);
                    sections.Add(refSection);
                }

                var top10 = dictionary.OrderByDescending(x => x.Value).Take(10).ToList();

                var stopWordCount = stopDictionary.Values.Sum();
                var wordCount = dictionary.Values.Sum() + stopWordCount;
                var keyWordsCount = top10.Take(2).Sum(x => x.Value);

                var waterLvl = stopWordCount / (double)wordCount * 100;
                var keyWordsLvl = keyWordsCount / (double)wordCount * 100;
                var zipfLvl = GetZipf(dictionary);

                var top10StopWords = stopDictionary.OrderByDescending(x => x.Value).Take(10).ToList();

                var stopWordsReport = new StringBuilder();

                for (int i = 0; i < 10 && i < top10StopWords.Count; i++)
                    stopWordsReport.Append($"{top10StopWords[i].Key}:   {top10StopWords[i].Value} раз\n");


                var keyWordsReport = new StringBuilder();
                for (int i = 0; i < 10 && i < top10.Count; i++)
                    keyWordsReport.Append($"{top10[i].Key}:   {top10[i].Value} раз\n");


                // TODO: add criterion here, value from IsAllKeywordsMentioned
                var criteria = new List<Criterion>
                {
                    new NumericalCriterion("Уровень водности", waterLvl,
                        settings.WaterCriteria.LowerBound, settings.WaterCriteria.UpperBound, settings.WaterCriteria.Weight,
                        "Процентное соотношение стоп-слов и общего количества слов в тексте",
                        $"Постарайтесь снизить количество используемых стоп-слов. Часто употребляемые стоп-слова в статье:\n{stopWordsReport.ToString()}",
                        "Текст слишком \"сухой\". Попробуйте добавить связки между разделами."),
                    new NumericalCriterion("Тошнота", keyWordsLvl,
                        settings.KeyWordsCriteria.LowerBound, settings.KeyWordsCriteria.UpperBound, settings.KeyWordsCriteria.Weight,
                        "Показатель повторений в текстовом документе ключевых слов и фраз",
                        $"Слишком частое повторение слов, при возможности, старайтесь использовать синонимы. Наиболее употребляемые слова в тексте:\n{keyWordsReport.ToString()}",
                        $"Постарайтесь увеличить частоту употребления ключевых слов текста:\n{keyWordsReport.ToString()}"),
                    new NumericalCriterion("Zipf", zipfLvl,
                        settings.Zipf.LowerBound, settings.Zipf.UpperBound, settings.Zipf.Weight,
                        "Значение отклонения текста статьи от идеальной кривой по Ципфу",
                        "Постарайтесь разнообразить текст, добавить связки между разделами, возможно, увеличить количество прилагательных.",
                        "Постарайтесь увеличить частоту употребления ключевых слов, возможно, снизить количество прилагательных."),
                    CreateKeywordMentioningCriterion(keywordMarks,keywordDict, settings)
                };

                var personalPronErrorsWordIds = errors.Where(x => x is UseOfPersonalPronounsError)
                    .Select(y => (y as UseOfPersonalPronounsError).ErrorWord.StartIndex).Distinct().ToList();

                var forbiddenWordErorrsWordIds = errors.Where(x => x is UseOfForbiddenWordsError)
                    .Select(x => (x as UseOfForbiddenWordsError).ErrorWord.StartIndex)
                    .Distinct()
                    .ToList();

                foreach (var sect in sections)
                {
                    foreach (var sent in sect.Sentences)
                    {
                        foreach (var word in sent.Words)
                        {
                            if (personalPronErrorsWordIds.Contains(word.StartIndex) && settings.UseOfPersonalPronounsCost > 0)
                            {
                                word.HasErrors = true;
                                word.ErrorCodes = $"{word.ErrorCodes}{(int)ErrorType.UseOfPersonalPronouns}";
                            }

                            if (forbiddenWordErorrsWordIds.Contains(word.StartIndex) && settings.ForbiddenWordsCost > 0)
                            {
                                word.HasErrors = true;
                                word.ErrorCodes = $"{word.ErrorCodes}{(int)ErrorType.UseOfForbiddenWord}";
                            }
                        }
                    }
                }

                foreach (var reference in referencesList)
                {
                    if (!reference.ReferedTo)
                    {
                        errors.Add(new SourceNotReferencedError(reference.Number,
                            settings.SourceNotReferencedErrorCost, settings.SourceNotReferencedCost,
                            settings.SourceNotReferencedGrading, settings.SourceNotReferencedGradingType));
                    }
                }

                for (int i = 0; i < sections.Count; i++)
                {
                    if (sections[i].Type == SectionType.SectionTitle
                        && i + 1 < sections.Count
                        && sections[i + 1].Type == SectionType.Text
                        && sections[i + 1].Sentences.Where(x => x.Words.Last().Original == ".").ToList().Count < 3)
                    {
                        sections[i].HasErrors = true;
                        var error = new ShortSectionError(sections[i].Id, sections[i].ToStringVersion(),
                            sections[i + 1].Sentences.Where(x => x.Words.Last().Original == ".").ToList().Count,
                            settings.ShortSectionErrorCost, settings.ShortSectionCost,
                            settings.ShortSectionGrading, settings.ShortSectionGradingType);
                        errors.Add(error);
                    }
                }

                var picRegex = new Regex(@"\b(\w*Рисунок (([1-9]|[1-9][0-9])*)*\w)\b");
                var tableRegex = new Regex(@"\b(\w*Таблица (([1-9]|[1-9][0-9])*)*\w)\b");
                var picMatches = picRegex.Matches(text).Select(x => int.Parse(x.Value.Split(" ")[1])).ToList();
                var tableMatches = tableRegex.Matches(text).Select(x => int.Parse(x.Value.Split(" ")[1])).ToList();

                var picRefRegex = new Regex(@"\b(\w*рис. (([1-9]|[1-9][0-9])*)*\w)\b");
                var tableRefRegex = new Regex(@"\b(\w*табл. (([1-9]|[1-9][0-9])*)*\w)\b");
                var picRefMatches = picRefRegex.Matches(text).Select(x => int.Parse(x.Value.Split(" ")[1])).ToList();
                var tableRefMatches = tableRefRegex.Matches(text).Select(x => int.Parse(x.Value.Split(" ")[1])).ToList();

                var picsNotRefd = picMatches.Except(picRefMatches).ToList();
                var tablesNotRefd = tableMatches.Except(tableRefMatches).ToList();

                foreach (var notRefdPic in picsNotRefd)
                    errors.Add(new PictureNotReferencedError(notRefdPic,
                        settings.PictureNotReferencedErrorCost, settings.PictureNotReferencedCost,
                        settings.PictureNotReferencedGrading, settings.PictureNotReferencedGradingType));

                foreach (var notRefdTable in tablesNotRefd)
                    errors.Add(new TableNotReferencedError(notRefdTable,
                        settings.TableNotReferencedErrorCost, settings.TableNotReferencedCost,
                        settings.TableNotReferencedGrading, settings.TableNotReferencedGradingType));

                // create default object for non-occurred errors
                if(!errors.Any(e => e.ErrorType == ErrorType.UseOfPersonalPronouns))
                {
                    errors.Add(new UseOfPersonalPronounsError(null, -1, settings.UseOfPersonalPronounsCost, settings.UseOfPersonalPronounsGrading,
                                    settings.UseOfPersonalPronounsGradingType));
                }
                if (!errors.Any(e => e.ErrorType == ErrorType.UseOfForbiddenWord))
                {
                    errors.Add(new UseOfForbiddenWordsError(null, null,
                                    -1, settings.ForbiddenWordsCost,
                                    settings.ForbiddenWordsGrading, settings.ForbiddenWordsGradingType));
                }
                if (!errors.Any(e => e.ErrorType == ErrorType.SourceNotReferenced))
                {
                    errors.Add(new SourceNotReferencedError(-1,
                            -1, settings.SourceNotReferencedCost,
                            settings.SourceNotReferencedGrading, settings.SourceNotReferencedGradingType));
                }
                if (!errors.Any(e => e.ErrorType == ErrorType.ShortSection))
                {
                    errors.Add(new ShortSectionError(Guid.Empty, "",
                            -1,
                            -1, settings.ShortSectionCost,
                            settings.ShortSectionGrading, settings.ShortSectionGradingType));
                }
                if (!errors.Any(e => e.ErrorType == ErrorType.PictureNotReferenced))
                {
                    errors.Add(new PictureNotReferencedError(-1,
                        -1, settings.PictureNotReferencedCost,
                        settings.PictureNotReferencedGrading, settings.PictureNotReferencedGradingType));
                }
                if (!errors.Any(e => e.ErrorType == ErrorType.TableNotReferenced))
                {
                    errors.Add(new TableNotReferencedError(-1,
                        -1, settings.TableNotReferencedCost,
                        settings.TableNotReferencedGrading, settings.TableNotReferencedGradingType));
                }            


                var analysisResult = new PaperAnalysisResult(sections, criteria, errors, settings.MaxScore);
                // save keyword marks in result set
                analysisResult.Keywords = keywordMarks;
                analysisResult.PaperTitle = paperName;
                analysisResult.PaperTitleRefs = papernameMarks;

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                return analysisResult;
            }
            catch (Exception ex)
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw ex;
            }

        }

        private static double GetZipf(Dictionary<string, int> dictionary)
        {
            var wordsForCalculating = dictionary.OrderByDescending(x => x.Value).Where(x => x.Value >= 5).ToList();
            var idealZipf = new List<double>();
            for (int i = 1; i <= wordsForCalculating.Count; i++)
                idealZipf.Add(wordsForCalculating[0].Value / (double)i);

            var deviation = .0;

            for (int i = 0; i < idealZipf.Count; i++)
                deviation += Math.Pow(idealZipf[i] - wordsForCalculating[i].Value, 2);

            return Math.Sqrt(deviation / idealZipf.Count);
        }
    }
}
