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
            "я", "ты", "он", "она", "оно", "мой", "мое", "моё", "моя", "твой", "твое", "твоё", "твоя", "его", "её"
        };

        public PapersAnalyzer(IPaperAnalyzerEnvironment environment)
        {
            _environment = environment;
        }

        public PaperAnalysisResult ProcessTextWithResult(
            string text, 
            string titlesString, 
            string paperName, 
            string refsName, 
            ResultScoreSettings settings)
        {
            try
            {
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


                if (string.IsNullOrEmpty(refsName))
                    refsName = "Список литературы";
                if (string.IsNullOrEmpty(paperName))
                    paperName = "";
                if (string.IsNullOrEmpty(titlesString))
                    titlesString = "";
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
                        else
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

                titles.Add(paperName);

                var titleIndex = text.IndexOf(paperName, StringComparison.InvariantCultureIgnoreCase);
                if (titleIndex != -1)
                    text = text.Substring(titleIndex);

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
                        section.Type = sections.Count() == 0 ? SectionType.PaperTitle : SectionType.SectionTitle;
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
                                    settings.UseOfPersonalPronounsErrorCost, settings.UseOfPersonalPronounsCost));
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
                                errors.Add(new UseOfForbiddenWordsError(dict.Name, new WordHtml(word.valueOriginal, word.posTaggerOutputType, word.startIndex), settings.ForbiddenWordsErrorCost, settings.ForbiddenWordsCost));
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

                var criteria = new List<Criterion>
                {
                    new NumericalCriterion("Уровень водности", waterLvl, 
                        settings.WaterCriterionLowerBound, settings.WaterCriterionUpperBound, settings.WaterCriterionFactor,
                        "Процентное соотношение стоп-слов и общего количества слов в тексте",
                        $"Постарайтесь снизить количество используемых стоп-слов. Часто употребляемые стоп-слова в статье:\n{stopWordsReport.ToString()}",
                        "Текст слишком \"сухой\". Попробуйте добавить связки между разделами."),
                    new NumericalCriterion("Тошнота", keyWordsLvl, 
                        settings.KeyWordsCriterionLowerBound, settings.KeyWordsCriterionUpperBound, settings.KeyWordsCriterionFactor,
                        "Показатель повторений в текстовом документе ключевых слов и фраз",
                        $"Слишком частое повторение слов, при возможности, старайтесь использовать синонимы. Наиболее употребляемые слова в тексте:\n{keyWordsReport.ToString()}",
                        $"Постарайтесь увеличить частоту употребления ключевых слов текста:\n{keyWordsReport.ToString()}"),
                    new NumericalCriterion("Zipf", zipfLvl, 
                        settings.ZipfFactorLowerBound, settings.ZipfFactorUpperBound, settings.ZipfFactor,
                        "Значение отклонения текста статьи от идеальной кривой по Ципфу",
                        "Постарайтесь разнообразить текст, добавить связки между разделами, возможно, увеличить количество прилагательных.",
                        "Постарайтесь увеличить частоту употребления ключевых слов, возможно, снизить количество прилагательных.")
                };

                var personalPronErrorsWordIds = errors.Where(x => x is UseOfPersonalPronounsError)
                    .Select(y => (y as UseOfPersonalPronounsError).ErrorWord.StartIndex).Distinct().ToList();

                var forbiddenWordErorrsWordIds = errors.Where(x => x is UseOfForbiddenWordsError)
                    .Select(x => (x as UseOfForbiddenWordsError).ErrorWord.StartIndex)
                    .Distinct()
                    .ToList();

                foreach(var sect in sections)
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

                foreach(var reference in referencesList)
                {
                    if (!reference.ReferedTo)
                    {
                        errors.Add(new SourceNotReferencedError(reference.Number, settings.SourceNotReferencedErrorCost, settings.SourceNotReferencedCost));
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
                            sections[i + 1].Sentences.Where(x => x.Words.Last().Original == ".").ToList().Count, settings.ShortSectionErrorCost, settings.ShortSectionCost);
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
                    errors.Add(new PictureNotReferencedError(notRefdPic, settings.PictureNotReferencedErrorCost, settings.PictureNotReferencedCost));

                foreach (var notRefdTable in tablesNotRefd)
                    errors.Add(new TableNotReferencedError(notRefdTable, settings.TableNotReferencedErrorCost, settings.TableNotReferencedCost));

                var analysisResult = new PaperAnalysisResult(sections, criteria, errors, settings.MaxScore);

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                return analysisResult;
            }
            catch (Exception ex)
            {
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
