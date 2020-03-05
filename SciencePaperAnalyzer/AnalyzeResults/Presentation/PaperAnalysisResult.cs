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
        public PaperAnalysisResult(IEnumerable<Section> sections, IEnumerable<Criterion> criteria, IEnumerable<Error> errors, double errorCost = 2.0)
        {
            Sections = new List<Section>();
            Sections.AddRange(sections);
            Criteria = new List<Criterion>();
            Criteria.AddRange(criteria);
            Errors = new List<Error>();
            Errors.AddRange(errors);
            Error = "";
            ErrorCost = errorCost;
        }

        [BsonElement("sections")]
        public List<Section> Sections { get; set; }

        [BsonElement("criteria")]
        public List<Criterion> Criteria { get; set; }

        [BsonElement("errors")]
        public List<Error> Errors { get; set; }

        [BsonElement("error")]
        public string Error { get; set; }

        [BsonElement("error_cost")]
        public double ErrorCost { get; set; }

        public bool IsScientific()
        {
            return Criteria.All(x => x.IsMet());
        }

        public double GetPaperGrade()
        {
            var baseValue = Criteria.Where(x => x is NumericalCriterion).Select(crit => (crit as NumericalCriterion).GetGradePart())
                .Aggregate((result, part) => result + part);
            var fines = Errors.Count * ErrorCost;
            return Math.Max(baseValue - fines, 0);
        }

        /// <summary>
        /// Техническая информация о процессе анализа
        /// Вынесена из out-параметров метода анализа в результат анализа
        /// Не требует сохранения в БД
        /// </summary>
        [BsonIgnore]
        public PaperAnalysisProcessingMetrics Metrics { get; set; }
    }
}
