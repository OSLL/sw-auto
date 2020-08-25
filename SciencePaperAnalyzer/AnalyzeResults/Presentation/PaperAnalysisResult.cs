using System;
using System.Collections.Generic;
using System.Linq;
using AnalyzeResults.Errors;
using AnalyzeResults.Settings;
using MongoDB.Bson.Serialization.Attributes;

namespace AnalyzeResults.Presentation
{
    [Serializable]
    public class PaperAnalysisResult
    {
        public PaperAnalysisResult(IEnumerable<Section> sections, IEnumerable<Criterion> criteria, IEnumerable<Error> errors, double maxScore)
        {
            Sections = new List<Section>();
            Sections.AddRange(sections);
            Criteria = new List<Criterion>();
            Criteria.AddRange(criteria);
            Errors = new List<Error>();
            Errors.AddRange(errors);
            Error = "";
            MaxScore = maxScore;
        }

        [BsonElement("sections")]
        public List<Section> Sections { get; set; }

        [BsonElement("criteria")]
        public List<Criterion> Criteria { get; set; }

        [BsonElement("errors")]
        public List<Error> Errors { get; set; }

        [BsonElement("paperTitle")]
        public string PaperTitle { get; set; }
        [BsonElement("paperTitleRefs")]
        public Dictionary<string, List<int>> PaperTitleRefs { get; set; }
        [BsonElement("keywords")]
        public Dictionary<string, List<int>> Keywords { get; set; }

        [BsonElement("error")]
        public string Error { get; set; }

        [BsonElement("maxScore")]
        public double MaxScore { get; set; }

        public bool IsScientific()
        {
            return Criteria.All(x => x.IsMet());
        }

        public double GetPaperGrade()
        {
            var resultScore = Criteria.Where(x => x is Criterion).Select(crit => crit.GetGradePart())
                .Aggregate((result, part) => result + part);
            var weightTmp = MaxScore - Criteria.Where(x => x is Criterion)
                .Sum(crit => crit.Factor);


            foreach (var error in Enum.GetValues(typeof(ErrorType)))
            {
                var specialError = Errors.FirstOrDefault(e => e.ErrorType == ((ErrorType) error));

                if (specialError == null)
                {
                    continue;
                }

                var weight = specialError.Weight;
                weightTmp -= weight;
                var errorCost = specialError.ErrorCost;
                var errorCount = Errors.Count(e => e.ErrorType == (ErrorType) error);
                switch (specialError.GradingType)
                {
                    case GradingType.ErrorCostSubtraction:
                        resultScore += Math.Max(weight - errorCount * errorCost, 0);
                        break;
                    case GradingType.GradingTable:
                        var result = specialError.Grading.OrderBy(g => g.Key)
                            .Select(g => (KeyValuePair<int, double>?) g)
                            .FirstOrDefault(g => g.Value.Key <= errorCount);
                        if (result == null)
                            resultScore += 0;
                        else
                            resultScore += result.Value.Value;
                        break;
                }
            }

            return Math.Round(resultScore) + weightTmp;
        }
    }
}
