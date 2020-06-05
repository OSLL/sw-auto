using System;
using AnalyzeResults.Errors;
using AnalyzeResults.Helpers;
using System.Collections.Generic;
using WebPaperAnalyzer.Models;

namespace AnalyzeResults.Settings
{
    [Serializable]
    public class ResultScoreSettings
    {
        public double WaterCriterionFactor { get; set; }
        public double WaterCriterionLowerBound { get; set; }
        public double WaterCriterionUpperBound { get; set; }
        public double KeyWordsCriterionFactor { get; set; }
        public double KeyWordsCriterionLowerBound { get; set; }
        public double KeyWordsCriterionUpperBound { get; set; }
        public double ZipfFactor { get; set; }
        public double ZipfFactorLowerBound { get; set; }
        public double ZipfFactorUpperBound { get; set; }
        public double UseOfPersonalPronounsCost { get; set; }
        public double UseOfPersonalPronounsErrorCost { get; set; }
        public double SourceNotReferencedCost { get; set; }
        public double SourceNotReferencedErrorCost { get; set; }
        public double ShortSectionCost { get; set; }
        public double ShortSectionErrorCost { get; set; }
        public double PictureNotReferencedCost { get; set; }
        public double PictureNotReferencedErrorCost { get; set; }
        public double TableNotReferencedCost { get; set; }
        public double TableNotReferencedErrorCost { get; set; }
        public double MaxScore { get; set; }
        public IEnumerable<ForbiddenWords> ForbiddenWords { get; set; }
        public double ForbiddenWordsCost { get; set; }
        public double ForbiddenWordsErrorCost { get; set; }
    }
}
