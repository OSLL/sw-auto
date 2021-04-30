using AnalyzeResults.Presentation;
using AnalyzeResults.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using SentenceHtml = AnalyzeResults.Presentation.Sentence;
using WordHtml = AnalyzeResults.Presentation.Word;
using Word = LangAnalyzerStd.Tokenizing.Word;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LangAnalyzerStd.Morphology;
using AnalyzeResults.Errors;
using LangAnalyzerStd.Postagger;

namespace PaperAnalyzer
{
    public sealed class PapersAnalyzerRemake : IPaperAnalyzer
    {
        private readonly IPaperAnalyzerEnvironment _environment;
        private readonly IConfiguration _appConfig;

        private static readonly List<string> personalPronouns = new List<string>
        {
            "я", "ты", "мой", "мое", "моё", "моя", "твой", "твое", "твоё", "твоя"
        };

        private List<PartOfSpeechEnum> stopPartsOfSpeech = new List<PartOfSpeechEnum>
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

        private Regex picRegex = new Regex(@"\b(\w*Рисунок (([1-9]|[1-9][0-9])*)*\w)\b");
        private Regex tableRegex = new Regex(@"\b(\w*Таблица (([1-9]|[1-9][0-9])*)*\w)\b");
        private Regex picRefRegex = new Regex(@"\b(\w*рис. (([1-9]|[1-9][0-9])*)*\w)\b");
        private Regex tableRefRegex = new Regex(@"\b(\w*табл. (([1-9]|[1-9][0-9])*)*\w)\b");

        public PapersAnalyzerRemake(IPaperAnalyzerEnvironment environment, IConfiguration appConfig = null)
        {
            _environment = environment;
            _appConfig = appConfig;
        }

        public PaperAnalysisResult ProcessTextWithResult(
            List<Section> sections,
            string paperName,
            string keywords,
            ResultScoreSettings settings)
        {
            try
            {
                // prepare the keyword dictionary and result dictionary for keyword marking
                var kwInText = TryGetKeywordFromText(ref sections);
                var keywordDict = PrepareKeywordsDict(kwInText + "," + keywords, ',');
                Dictionary<string, List<int>> keywordMarks = new Dictionary<string, List<int>>();


                // use user defined title if auto extract is set, else use the paper name provided
                bool autoExtractPaperName = paperName.Equals(_appConfig == null ? "" : _appConfig.GetValue("AutoTitleExtractionToken", "#auto#"));
                if (autoExtractPaperName)
                {
                    var paperTitleSection = sections.Find(e => e.Type == SectionType.PaperTitle);
                    paperName = paperTitleSection == null ? "" : Regex.Replace(paperTitleSection.OriginalText.Replace(",", " , "), @"\s+", " ");
                }
                else if (!string.IsNullOrEmpty(paperName))
                {
                    Section section = new Section();
                    section.Type = SectionType.PaperTitle;
                    section.OriginalText = paperName;
                    sections.Insert(0, section);
                }
                // prepare the keyword dictionary and result dictionary for paper name marking
                var paperNameDict = PrepareKeywordsDict(paperName, ' ');
                Dictionary<string, List<int>> papernameMarks = new Dictionary<string, List<int>>();
                //Console.WriteLine(paperName);


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

                // definition of result variables of paper processing
                var dictionary = new Dictionary<string, int>(); // dictionary of word counts
                var stopDictionary = new Dictionary<string, int>(); // dictionary of stop-word counts
                var errors = new List<Error>(); // list of errors

                var picMatches = new List<int>();
                var tableMatches = new List<int>();

                var picRefMatches = new List<int>();
                var tableRefMatches = new List<int>();

                var sectionStartPositionOffset = 0;

                var sentenceCount = 0;
                foreach (Section section in sections)
                {

                    List<Word[]> notatedSentences = SectionPreprocessing(section);

                    if (section.Type == SectionType.PaperTitle || section.Type == SectionType.SectionTitle)
                    {
                        // merge all word[] in the paper title section to 1 sentence as the title will consist of only 1 sentence.
                        var processedWordList = notatedSentences.SelectMany(e => e).ToList();
                        var sentence = new SentenceHtml(SentenceType.Basic, processedWordList.Select(x => new WordHtml(x.valueOriginal, x.posTaggerOutputType, x.startIndex + sectionStartPositionOffset)));
                        sentence.SentenceIndex = sentenceCount++;
                        section.Sentences.Add(sentence);
                    }

                    if (section.Type == SectionType.Text)
                    {
                        // processing keywords (mark appearance of keywords and store in the dictionary)
                        var wholeTextWords = notatedSentences.SelectMany(x => x).ToArray();
                        var keywordInSentence = MarkKeywordInSentence(keywordDict, wholeTextWords, sectionStartPositionOffset);
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
                        var papernameInSentence = MarkKeywordInSentence(paperNameDict, wholeTextWords, sectionStartPositionOffset);
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

                        // actual processing of the text (finding errors, counting stop words, forbidden words....)
                        foreach (var r in notatedSentences)
                        {
                            var sentence = new SentenceHtml(SentenceType.Basic, r.Select(x => new WordHtml(x.valueOriginal, x.posTaggerOutputType, x.startIndex + sectionStartPositionOffset)));
                            sentence.SentenceIndex = sentenceCount++;
                            section.Sentences.Add(sentence);

                            foreach (var word in r)
                            {
                                var normalForm = word.morphology.NormalForm;
                                if (string.IsNullOrEmpty(normalForm))
                                    continue;
                                if (word.morphology.PartOfSpeech == PartOfSpeechEnum.Pronoun)
                                {
                                    if (personalPronouns.Contains(word.morphology.NormalForm))
                                        errors.Add(new UseOfPersonalPronounsError(new WordHtml(word.valueOriginal, word.posTaggerOutputType, word.startIndex + sectionStartPositionOffset),
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
                                        errors.Add(new UseOfForbiddenWordsError(dict.Name, new WordHtml(word.valueOriginal, word.posTaggerOutputType, word.startIndex + sectionStartPositionOffset),
                                            settings.ForbiddenWordsErrorCost, settings.ForbiddenWordsCost,
                                            settings.ForbiddenWordsGrading, settings.ForbiddenWordsGradingType));
                                    }
                                }
                            }
                        }

                        // finding picture and table references
                        picMatches.AddRange(picRegex.Matches(section.OriginalText).Select(x => int.Parse(x.Value.Split(" ")[1])).ToList());
                        tableMatches.AddRange(tableRegex.Matches(section.OriginalText).Select(x => int.Parse(x.Value.Split(" ")[1])).ToList());

                        picRefMatches.AddRange(picRefRegex.Matches(section.OriginalText).Select(x => int.Parse(x.Value.Split(" ")[1])).ToList());
                        tableRefMatches.AddRange(tableRefRegex.Matches(section.OriginalText).Select(x => int.Parse(x.Value.Split(" ")[1])).ToList());
                    }
                    sectionStartPositionOffset += section.OriginalText.Length;
                }

                Section referenceSection = sections.Find(e => e.Type == SectionType.ReferencesList);
                ProcessingReferenceList(sections, referenceSection);

                // finished processing, moving to finalizing results and building result structures/reports
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
                    new NumericalCriterion("Keywords Quality", -1,
                        settings.KeywordsQuality.LowerBound, settings.KeywordsQuality.UpperBound, settings.KeywordsQuality.Weight,
                        "description",
                        "advice on lower value",
                        "advice on higher value",isPlaceholder:true),
                    CreateKeywordMentioningCriterion(keywordMarks,keywordDict, settings)
                };

                // back-tracking error position (mark words with errors)
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

                if (referenceSection != null)
                {
                    // only verifired 1 way; TODO: notify if a reference is used without definition
                    foreach (var reference in referenceSection.References)
                    {
                        if (!reference.ReferedTo)
                        {
                            errors.Add(new SourceNotReferencedError(reference.Number,
                                settings.SourceNotReferencedErrorCost, settings.SourceNotReferencedCost,
                                settings.SourceNotReferencedGrading, settings.SourceNotReferencedGradingType));
                        }
                    }
                }

                for (int i = 0; i < sections.Count-1; i++)
                {
                    if(sections[i].Type == SectionType.SectionTitle && sections[i+1].Type!= SectionType.SectionTitle)
                    {
                        for(int j = i + 1; j < sections.Count; j++)
                        {
                            if(sections[j].Type == SectionType.SectionTitle)
                            {
                                var numverOfSentence = sections.GetRange(i + 1, j - i - 1).Sum(e => e.Sentences.Count);
                                if (numverOfSentence < 3)
                                {
                                    sections[i].HasErrors = true;
                                    var error = new ShortSectionError(sections[i].Id, sections[i].ToStringVersion(),
                                        sections[i + 1].Sentences.Where(x => x.Words.Last().Original == ".").ToList().Count,
                                        settings.ShortSectionErrorCost, settings.ShortSectionCost,
                                        settings.ShortSectionGrading, settings.ShortSectionGradingType);
                                    errors.Add(error);
                                }
                                break;
                            }
                        }
                    }
                }

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
                if (!errors.Any(e => e.ErrorType == ErrorType.UseOfPersonalPronouns))
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
                
                errors.Add(new DiscordantSentenceError(null, settings.DiscordantSentenceErrorCost, settings.DiscordantSentenceCost,
                        settings.DiscordantSentenceGrading, settings.DiscordantSentenceGradingType, true));

                errors.Add(new MissingSentenceError(null, settings.MissingSentenceErrorCost, settings.MissingSentenceCost,
                        settings.MissingSentenceGrading, settings.MissingSentenceGradingType, true));


                //foreach (var s in sections)
                //{
                //    Console.WriteLine(s.OriginalText);
                //}
                Console.WriteLine($"Section count: {sections.Count}");
                Console.WriteLine($"Paper name: {paperName}");

                var analysisResult = new PaperAnalysisResult(null, sections, criteria, errors, settings.MaxScore);
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

        private void ProcessingReferenceList(List<Section> sections, Section referenceSection)
        {
            var referencesList = new List<Reference>();

            // start processing reference list, can be moved to separate function
            if (referenceSection != null)
            {
                string referenceSectionContent = referenceSection.OriginalText;

                var tstRefs = referenceSectionContent.Split("\n");

                var references = new List<string>();

                var refStartRegex = new Regex(@"^(([1-9]|[1-9][0-9])\. )");

                // split reference section to lines and process numbering?
                for (int i = 0; i < tstRefs.Length; i++)
                {
                    var regexResult = refStartRegex.Match(tstRefs[i]);

                    // note: in the original analyzer, this part is used to work with unstructured content, meaning: it deals with references that might be broken to multiple lines
                    //if (regexResult.Success)
                    //{
                    //    if (regexResult.Value == $"{references.Count + 1}. ")
                    //    {
                    //        references.Add(tstRefs[i].Trim());
                    //    }
                    //    else
                    //    {
                    //        var last = references.Last();
                    //        if (last.Contains($"{references.Count + 1}. "))
                    //        {
                    //            var refs = last.Split($"{references.Count + 1}. ");
                    //            references.RemoveAt(references.Count - 1);
                    //            references.Add(refs[0].Trim());
                    //            references.Add($"{references.Count + 1}. {refs[1].Trim()}");
                    //            references.Add(tstRefs[i].Trim());
                    //        }
                    //        else
                    //        {
                    //            references.RemoveAt(references.Count - 1);
                    //            references.Add(last + tstRefs[i].Trim());
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    if (references.Count != 0)
                    //    {
                    //        var last = references.Last();
                    //        references.RemoveAt(references.Count - 1);
                    //        references.Add(last + tstRefs[i].Trim());
                    //    }
                    //}
                    // REWORK: added consideration for auto numbering list (all started with 1.), in exchange for multiple lines processing
                    if (regexResult.Success)
                    {
                        if (regexResult.Value == $"{references.Count + 1}. ")
                        {
                            references.Add(tstRefs[i].Trim());
                        }
                        else
                        {
                            references.Add(tstRefs[i].Trim().Replace(regexResult.Value, (references.Count + 1) + ". "));
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

                // find references between [ and ] from the text (section from array sections)
                var matches = new List<string>();
                foreach (Section section in sections)
                {
                    var firstMatches = referenceRegex.Matches(section.OriginalText).Select(x => x.Value.Replace("[", "").Replace("]", "")).Select(x => x.Split(",")).ToList();
                    foreach (var match in firstMatches)
                    {
                        matches.AddRange(match);
                    }
                }
                matches = matches.Distinct().ToList();

                // assumed this is to deal with reference of the format [1-4]?
                // convert text reference "1" or "1-4" or other type, to a list of number
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
                    else if (match.Length > 0)
                    {
                        var num = int.Parse(match);
                        referenceIndexes.Add(num);
                    }
                }

                referenceIndexes = referenceIndexes.Distinct().ToList(); // get distinc list of mentioned references

                var refYearRegex = new Regex(@"((19|20)\d{2}\.)");
                foreach (var reference in references)
                {
                    try
                    {
                        var number = int.Parse(reference.Split(".")[0]);
                        var refSentence = new SentenceHtml(SentenceType.Reference)
                        {
                            Original = reference
                        };
                        referenceSection.Sentences.Add(refSentence);
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
                referenceSection.References = referencesList;
            }
            // done processing reference list -----------------------------------------------------------

        }

        private List<Word[]> SectionPreprocessing(Section section)
        {
            var result = _environment.Processor.RunFullAnalysis(section.OriginalText, true, true, true, true);

            return result;

            // purpose??
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
                            //if (tmpSentence.Count > 0 && tmpSentence.Last().morphology.PartOfSpeech == PartOfSpeechEnum.Preposition)
                            //{
                            //    var lastWord = tmpSentence.Last();
                            //    tmpSentence.RemoveAt(tmpSentence.Count - 1);
                            //    var newSentence = tmpSentence.ConvertAll(x => x);
                            //    tmpSentence.Clear();
                            //    tmpSentence.Add(lastWord);
                            //    tmpSentence.Add(word);
                            //    newResult.Add(newSentence.ToArray());
                            //}
                            //else
                            //{
                            tmpSentence.Add(word);
                            continue;
                            //}
                        }
                    }
                }
                newResult.Add(tmpSentence.ToArray());
            }

            newResult = newResult.Where(x => x.Length > 0).ToList();

            return newResult;
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

        private Dictionary<string, List<int>> MarkKeywordInSentence(Dictionary<string, Word[]> keywords, Word[] sentence, int sectionStartPositionOffset = 0)
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
                            siTemp.Add(sentence[i + offset].startIndex + sectionStartPositionOffset);
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
        private string TryGetKeywordFromText(ref List<Section> sections)
        {
            string lookBehind = @"Ключевые слова: ";
            string lookForward = "$";
            foreach (var section in sections)
            {
                Match m = Regex.Match(section.OriginalText, $@"(?<={lookBehind})[\S\s]*?(?=({lookForward}))");
                if (m.Success)
                {
                    Section keywordList = section;
                    sections.Remove(section);
                    return Regex.Replace(keywordList.OriginalText.Replace(lookBehind, "").Replace('\n', ' ').Replace('\r', ' '), @"\s+", " ");
                }
            }
            return "";
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


        public PaperAnalysisResult ProcessTextWithResult(string text, string titlesString, string paperName, string refsName, string keywords, ResultScoreSettings settings)
        {
            // utilize old analyzer for unstructured text
            return new PapersAnalyzer(_environment, _appConfig).ProcessTextWithResult(text, titlesString, paperName, refsName, keywords, settings);
        }
    }
}
