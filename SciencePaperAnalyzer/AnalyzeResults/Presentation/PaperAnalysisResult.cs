using System;
using System.Collections.Generic;
using System.Linq;
using AnalyzeResults.Errors;
using MongoDB.Bson.Serialization.Attributes;

namespace AnalyzeResults.Presentation
{
    [Serializable]
    public class PaperAnalysisResult
    {
        public PaperAnalysisResult(IEnumerable<Section> sections, IEnumerable<Criterion> criteria, IEnumerable<Error> errors)
        {
            Sections = new List<Section>();
            Sections.AddRange(sections);
            Criteria = new List<Criterion>();
            Criteria.AddRange(criteria);
            Errors = new List<Error>();
            Errors.AddRange(errors);
            Error = "";
        }

        [BsonElement("sections")]
        public List<Section> Sections { get; set; }

        [BsonElement("criteria")]
        public List<Criterion> Criteria { get; set; }

        [BsonElement("errors")]
        public List<Error> Errors { get; set; }

        [BsonElement("error")]
        public string Error { get; set; }

        public bool IsScientific()
        {
            return Criteria.All(x => x.IsMet());
        }

        public double GetPaperGrade()
        {
            var resultScore = Criteria.Where(x => x is NumericalCriterion).Select(crit => (crit as NumericalCriterion).GetGradePart())
                .Aggregate((result, part) => result + part);

            foreach (var error in Enum.GetValues(typeof(ErrorType)))
            {
                var specialError = Errors.FirstOrDefault(e => e.ErrorType == ((ErrorType) error));

                if (specialError == null)
                    continue;

                var weight = specialError.Weight;
                var errorCost = specialError.ErrorCost;
                resultScore += Math.Max(weight - Errors.Count(e => e.ErrorType == (ErrorType)error)*errorCost, 0);
            }

            return Math.Round(resultScore);
        }
    }
}
