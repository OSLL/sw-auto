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

    public class ResultCriterion : AnalyzeResults.Settings.ResultScoreSettings
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string TeacherLogin { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }

        public void Recalculate()
        {
            var stabilizer = 100 / (WaterCriterionFactor + KeyWordsCriterionFactor + ZipfFactor +
                                    UseOfPersonalPronounsCost + SourceNotReferencedCost + ShortSectionCost +
                                    PictureNotReferencedCost + TableNotReferencedCost);
            WaterCriterionFactor *= stabilizer;
            KeyWordsCriterionFactor *= stabilizer;
            ZipfFactor *= stabilizer;
            UseOfPersonalPronounsCost *= stabilizer;
            SourceNotReferencedCost *= stabilizer;
            ShortSectionCost *= stabilizer;
            PictureNotReferencedCost *= stabilizer;
            TableNotReferencedCost *= stabilizer;
        }
    }
}