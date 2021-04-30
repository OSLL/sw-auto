using System;
using System.Collections.Generic;
using WebPaperAnalyzer.Models;

namespace AnalyzeResults.Settings
{
    /// <summary>
    /// Критерии для оценивания работы
    /// </summary>
    [Serializable]
    public class ResultScoreSettings
    {
        /// <summary>
        /// Критерий водности
        /// </summary>
        public BoundedCriteria WaterCriteria { get; set; }
        /// <summary>
        /// Критерий использования ключевых слов
        /// </summary>
        public BoundedCriteria KeyWordsCriteria { get; set; }
        /// <summary>
        /// Фактор Ципф
        /// </summary>
        public BoundedCriteria Zipf { get; set; }
        /// <summary>
        /// Фактор Ципф
        /// </summary>
        public BoundedCriteria KeywordsQuality { get; set; }
        /// <summary>
        /// Упоминание ключевых слов
        /// </summary>
        public BoundedCriteria KeywordsMentioning { get; set; }
        public double UseOfPersonalPronounsCost { get; set; }
        public double UseOfPersonalPronounsErrorCost { get; set; }
        public List<ScopePair> UseOfPersonalPronounsGrading = new List<ScopePair>();
        public GradingType UseOfPersonalPronounsGradingType { get; set; }
        public double SourceNotReferencedCost { get; set; }
        public double SourceNotReferencedErrorCost { get; set; }
        public List<ScopePair> SourceNotReferencedGrading = new List<ScopePair>();
        public GradingType SourceNotReferencedGradingType { get; set; }
        public double ShortSectionCost { get; set; }
        public double ShortSectionErrorCost { get; set; }
        public List<ScopePair> ShortSectionGrading = new List<ScopePair>();
        public GradingType ShortSectionGradingType { get; set; }
        public double PictureNotReferencedCost { get; set; }
        public double PictureNotReferencedErrorCost { get; set; }
        public List<ScopePair> PictureNotReferencedGrading = new List<ScopePair>();
        public GradingType PictureNotReferencedGradingType { get; set; }
        public double TableNotReferencedCost { get; set; }
        public double TableNotReferencedErrorCost { get; set; }
        public List<ScopePair> TableNotReferencedGrading = new List<ScopePair>();
        public GradingType TableNotReferencedGradingType { get; set; }

        public double DiscordantSentenceCost { get; set; }
        public double DiscordantSentenceErrorCost { get; set; }
        public List<ScopePair> DiscordantSentenceGrading = new List<ScopePair>();
        public GradingType DiscordantSentenceGradingType { get; set; }

        public double MissingSentenceCost { get; set; }
        public double MissingSentenceErrorCost { get; set; }
        public List<ScopePair> MissingSentenceGrading = new List<ScopePair>();
        public GradingType MissingSentenceGradingType { get; set; }

        public double MaxScore { get; set; }
        public IEnumerable<ForbiddenWords> ForbiddenWords { get; set; }
        public double ForbiddenWordsCost { get; set; }
        public double ForbiddenWordsErrorCost { get; set; }
        public List<ScopePair> ForbiddenWordsGrading = new List<ScopePair>();
        public GradingType ForbiddenWordsGradingType { get; set; }
    }

    [Serializable]
    public class ScopePair
    {
        public int Boarder { get; set; }
        public int Value { get; set; }
    }
}
