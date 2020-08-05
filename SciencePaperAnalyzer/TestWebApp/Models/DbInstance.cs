using System;
using System.Collections.Generic;
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

    [BsonIgnoreExtraElements]
    public class ResultCriterion : AnalyzeResults.Settings.ResultScoreSettings
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

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