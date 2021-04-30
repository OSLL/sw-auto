using System;
using System.Collections.Generic;
using System.Linq;
using AnalyzeResults.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        public double KeywordsQualityCriterionFactor { get; set; }
        public double KeywordsQualityCriterionLowerBound { get; set; }
        public double KeywordsQualityCriterionUpperBound { get; set; }

        public double KeywordsMentioningFactor { get; set; }
        public double UseOfPersonalPronounsCost { get; set; }
        public double UseOfPersonalPronounsErrorCost { get; set; }
        public List<ScopePair> UseOfPersonalPronounsGrading { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public GradingType UseOfPersonalPronounsGradingType { get; set; }
        public double SourceNotReferencedCost { get; set; }
        public double SourceNotReferencedErrorCost { get; set; }
        public List<ScopePair> SourceNotReferencedGrading { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public GradingType SourceNotReferencedGradingType { get; set; }
        public double ShortSectionCost { get; set; }
        public double ShortSectionErrorCost { get; set; }
        public List<ScopePair> ShortSectionGrading { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public GradingType ShortSectionGradingType { get; set; }
        public double PictureNotReferencedCost { get; set; }
        public double PictureNotReferencedErrorCost { get; set; }
        public List<ScopePair> PictureNotReferencedGrading { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public GradingType PictureNotReferencedGradingType { get; set; }

        public double TableNotReferencedCost { get; set; }
        public double TableNotReferencedErrorCost { get; set; }
        public List<ScopePair> TableNotReferencedGrading { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public GradingType TableNotReferencedGradingType { get; set; }

        public double DiscordantSentenceCost { get; set; }
        public double DiscordantSentenceErrorCost { get; set; }
        public List<ScopePair> DiscordantSentenceGrading { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public GradingType DiscordantSentenceGradingType { get; set; }

        public double MissingSentenceCost { get; set; }
        public double MissingSentenceErrorCost { get; set; }
        public List<ScopePair> MissingSentenceGrading { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public GradingType MissingSentenceGradingType { get; set; }


        public double MaxScore { get; set; }
        public IEnumerable<ForbiddenWords> ForbiddenWords { get; set; }
        public double ForbiddenWordsCost { get; set; }
        public double ForbiddenWordsErrorCost { get; set; }
        public List<ScopePair> ForbiddenWordsGrading { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public GradingType ForbiddenWordsGradingType { get; set; }

        public string TeacherLogin { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public IEnumerable<string> ForbiddenWordDictionary { get; set; }
        
        public void Recalculate()
        {
            var totalWeight = WaterCriterionFactor + KeyWordsCriterionFactor + ZipfFactor + KeywordsQualityCriterionFactor +  KeywordsMentioningFactor +
                                UseOfPersonalPronounsCost + SourceNotReferencedCost + ShortSectionCost +
                                PictureNotReferencedCost + TableNotReferencedCost + DiscordantSentenceCost + MissingSentenceCost + ForbiddenWordsCost;
            double stabilizer = Math.Abs(totalWeight) > 0.01 ? MaxScore / totalWeight : 0;

            WaterCriterionFactor = Math.Round(stabilizer * WaterCriterionFactor, 2);
            KeyWordsCriterionFactor = Math.Round(stabilizer * KeyWordsCriterionFactor, 2);
            KeywordsMentioningFactor = Math.Round(stabilizer * KeywordsMentioningFactor, 2);
            ZipfFactor = Math.Round(stabilizer * ZipfFactor, 2);
            KeywordsQualityCriterionFactor = Math.Round(stabilizer * KeywordsQualityCriterionFactor, 2);
            UseOfPersonalPronounsCost = Math.Round(stabilizer * UseOfPersonalPronounsCost, 2);
            SourceNotReferencedCost = Math.Round(stabilizer * SourceNotReferencedCost, 2);
            ShortSectionCost = Math.Round(stabilizer * ShortSectionCost, 2);
            PictureNotReferencedCost = Math.Round(stabilizer * PictureNotReferencedCost, 2);
            TableNotReferencedCost = Math.Round(stabilizer * TableNotReferencedCost, 2);
            DiscordantSentenceCost = Math.Round(stabilizer * DiscordantSentenceCost, 2);
            MissingSentenceCost = Math.Round(stabilizer * MissingSentenceCost, 2);
            ForbiddenWordsCost = Math.Round(stabilizer * ForbiddenWordsCost, 2);
        }

        public static ResultCriterion FromViewModelToResultCriterion(ViewModels.AddCriterion model, string teacherName, string id = null)
        {
            var criterion = new ResultCriterion();
            if (id != null)
            {
                criterion.Id = id;
            }
            criterion.Name = model.Name;
            criterion.Summary = model.Summary;
            criterion.TeacherLogin = teacherName;
            criterion.MaxScore = model.MaxScore;
            criterion.WaterCriterionFactor = model.WaterCriterionFactor;
            criterion.WaterCriterionLowerBound = model.WaterCriterionLowerBound;
            criterion.WaterCriterionUpperBound = model.WaterCriterionUpperBound;
            criterion.KeyWordsCriterionFactor = model.KeyWordsCriterionFactor;
            criterion.KeyWordsCriterionLowerBound = model.KeyWordsCriterionLowerBound;
            criterion.KeyWordsCriterionUpperBound = model.KeyWordsCriterionUpperBound;
            criterion.ZipfFactor = model.ZipfFactor;
            criterion.ZipfFactorLowerBound = model.ZipfFactorLowerBound;
            criterion.ZipfFactorUpperBound = model.ZipfFactorUpperBound;

            criterion.KeywordsQualityCriterionFactor = model.KeywordsQualityCriterionFactor;
            criterion.KeywordsQualityCriterionLowerBound = model.KeywordsQualityCriterionLowerBound;
            criterion.KeywordsQualityCriterionUpperBound = model.KeywordsQualityCriterionUpperBound;

            criterion.KeywordsMentioningFactor = model.KeywordsMentioningFactor;
            criterion.UseOfPersonalPronounsCost = model.UseOfPersonalPronounsCost;
            criterion.UseOfPersonalPronounsErrorCost = model.UseOfPersonalPronounsErrorCost;
            criterion.SourceNotReferencedCost = model.SourceNotReferencedCost;
            criterion.SourceNotReferencedErrorCost = model.SourceNotReferencedErrorCost;
            criterion.ShortSectionCost = model.ShortSectionCost;
            criterion.ShortSectionErrorCost = model.ShortSectionErrorCost;
            criterion.PictureNotReferencedCost = model.PictureNotReferencedCost;
            criterion.PictureNotReferencedErrorCost = model.PictureNotReferencedErrorCost;
            criterion.TableNotReferencedCost = model.TableNotReferencedCost;
            criterion.TableNotReferencedErrorCost = model.TableNotReferencedErrorCost;

            criterion.DiscordantSentenceCost = model.DiscordantSentenceCost;
            criterion.DiscordantSentenceErrorCost = model.DiscordantSentenceErrorCost;

            criterion.MissingSentenceCost = model.MissingSentenceCost;
            criterion.MissingSentenceErrorCost = model.MissingSentenceErrorCost;

            try
            {
                criterion.ForbiddenWordDictionary =
                    model.Dictionaries.Where(x => x.IsSelected).Select(x => x.Name);
            }
            catch (Exception)
            {
                criterion.ForbiddenWordDictionary = new List<string>();
            }
            criterion.ForbiddenWordsCost = model.ForbiddenWordsCost;
            criterion.ForbiddenWordsErrorCost = model.ForbiddenWordsErrorCost;

            criterion.ForbiddenWordsGradingType = model.ForbiddenWordsGradingTypeVM ? GradingType.GradingTable : GradingType.ErrorCostSubtraction;
            criterion.TableNotReferencedGradingType = model.TableNotReferencedGradingTypeVM ? GradingType.GradingTable : GradingType.ErrorCostSubtraction;
            criterion.UseOfPersonalPronounsGradingType = model.UseOfPersonalPronounsGradingTypeVM ? GradingType.GradingTable : GradingType.ErrorCostSubtraction;
            criterion.PictureNotReferencedGradingType = model.PictureNotReferencedGradingTypeVM ? GradingType.GradingTable : GradingType.ErrorCostSubtraction;
            criterion.ShortSectionGradingType = model.ShortSectionGradingTypeVM ? GradingType.GradingTable : GradingType.ErrorCostSubtraction;
            criterion.SourceNotReferencedGradingType = model.SourceNotReferencedGradingTypeVM ? GradingType.GradingTable : GradingType.ErrorCostSubtraction;

            criterion.DiscordantSentenceGradingType = model.DiscordantSentenceGradingTypeVM ? GradingType.GradingTable : GradingType.ErrorCostSubtraction;
            criterion.MissingSentenceGradingType = model.MissingSentenceGradingTypeVM ? GradingType.GradingTable : GradingType.ErrorCostSubtraction;

            criterion.ForbiddenWordsGrading = new List<ScopePair>(model.ForbiddenWordsGrading);
            criterion.TableNotReferencedGrading = new List<ScopePair>(model.TableNotReferencedGrading);
            criterion.UseOfPersonalPronounsGrading = new List<ScopePair>(model.UseOfPersonalPronounsGrading);
            criterion.PictureNotReferencedGrading = new List<ScopePair>(model.PictureNotReferencedGrading);
            criterion.ShortSectionGrading = new List<ScopePair>(model.ShortSectionGrading);
            criterion.SourceNotReferencedGrading = new List<ScopePair>(model.SourceNotReferencedGrading);

            criterion.DiscordantSentenceGrading = new List<ScopePair>(model.DiscordantSentenceGrading);
            criterion.MissingSentenceGrading = new List<ScopePair>(model.MissingSentenceGrading);

            criterion.Recalculate();

            return criterion;
        }
    }
}