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

    public class ResultCriterion
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string TeacherLogin { get; set; }
        public string Name { get; set; }
        public double ErrorCost { get; set; }
        public double WaterCriterionFactor { get; set; }
        public double WaterCriterionLowerBound { get; set; }
        public double WaterCriterionUpperBound { get; set; }
        public double KeyWordsCriterionFactor { get; set; }
        public double KeyWordsCriterionLowerBound { get; set; }
        public double KeyWordsCriterionUpperBound { get; set; }
        public double ZipfFactor { get; set; }
        public double ZipfFactorLowerBound { get; set; }
        public double ZipfFactorUpperBound { get; set; }
        public IEnumerable<string> ForbiddenWordDictionary { get; set; }

        public bool IsValid()
        {
            return (Math.Abs(WaterCriterionFactor + KeyWordsCriterionFactor + ZipfFactor - 100) < 0.001) &&
                   WaterCriterionLowerBound < WaterCriterionUpperBound &&
                   KeyWordsCriterionLowerBound < KeyWordsCriterionUpperBound &&
                   ZipfFactorLowerBound < ZipfFactorUpperBound;
        }
    }


}