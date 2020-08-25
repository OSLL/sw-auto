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
        /// Упоминание ключевых слов
        /// </summary>
        public BoundedCriteria KeywordsMentioning { get; set; }
        public double UseOfPersonalPronounsCost { get; set; }
        public double UseOfPersonalPronounsErrorCost { get; set; }
        public Dictionary<int, double> UseOfPersonalPronounsGrading = new Dictionary<int, double>();
        public GradingType UseOfPersonalPronounsGradingType { get; set; }
        public double SourceNotReferencedCost { get; set; }
        public double SourceNotReferencedErrorCost { get; set; }
        public Dictionary<int, double> SourceNotReferencedGrading = new Dictionary<int, double>();
        public GradingType SourceNotReferencedGradingType { get; set; }
        public double ShortSectionCost { get; set; }
        public double ShortSectionErrorCost { get; set; }
        public Dictionary<int, double> ShortSectionGrading = new Dictionary<int, double>();
        public GradingType ShortSectionGradingType { get; set; }
        public double PictureNotReferencedCost { get; set; }
        public double PictureNotReferencedErrorCost { get; set; }
        public Dictionary<int, double> PictureNotReferencedGrading = new Dictionary<int, double>();
        public GradingType PictureNotReferencedGradingType { get; set; }
        public double TableNotReferencedCost { get; set; }
        public double TableNotReferencedErrorCost { get; set; }
        public Dictionary<int, double> TableNotReferencedGrading = new Dictionary<int, double>();
        public GradingType TableNotReferencedGradingType { get; set; }
        public double MaxScore { get; set; }
        public IEnumerable<ForbiddenWords> ForbiddenWords { get; set; }
        public double ForbiddenWordsCost { get; set; }
        public double ForbiddenWordsErrorCost { get; set; }
        public Dictionary<int, double> ForbiddenWordsGrading = new Dictionary<int, double>();
        public GradingType ForbiddenWordsGradingType { get; set; }
    }
}
