using System;
using System.Collections.Generic;
using AnalyzeResults.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebPaperAnalyzer.Models
{
    public class User
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class ResultCriterion
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public double WaterCriterionFactor { get; set; }

        public double WaterCriterionUpperBound { get; set; }
                public double WaterCriterionLowerBound { get; set; }
        public double KeyWordsCriterionFactor { get; set; }
        public double KeyWordsCriterionLowerBound { get; set; }
        public double KeyWordsCriterionUpperBound { get; set; }
        public double ZipfFactor { get; set; }
        public double ZipfFactorLowerBound { get; set; }
        public double ZipfFactorUpperBound { get; set; }
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

        public string TeacherLogin { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public IEnumerable<string> ForbiddenWordDictionary { get; set; }
        
        public void Recalculate()
        {
            var totalWeight = WaterCriterionFactor + KeyWordsCriterionFactor + ZipfFactor +
                                UseOfPersonalPronounsCost + SourceNotReferencedCost + ShortSectionCost +
                                PictureNotReferencedCost + TableNotReferencedCost + ForbiddenWordsCost;
            double stabilizer = Math.Abs(totalWeight) > 0.01 ? MaxScore / totalWeight : 0;

            WaterCriterionFactor = Math.Round(stabilizer * WaterCriterionFactor, 2);
            KeyWordsCriterionFactor = Math.Round(stabilizer * KeyWordsCriterionFactor, 2);
            ZipfFactor = Math.Round(stabilizer * ZipfFactor, 2);
            UseOfPersonalPronounsCost = Math.Round(stabilizer * UseOfPersonalPronounsCost, 2);
            SourceNotReferencedCost = Math.Round(stabilizer * SourceNotReferencedCost, 2);
            ShortSectionCost = Math.Round(stabilizer * ShortSectionCost, 2);
            PictureNotReferencedCost = Math.Round(stabilizer * PictureNotReferencedCost, 2);
            TableNotReferencedCost = Math.Round(stabilizer * TableNotReferencedCost, 2);
            ForbiddenWordsCost = Math.Round(stabilizer * ForbiddenWordsCost, 2);
        }
    }
}