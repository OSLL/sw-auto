using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

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
    }
}