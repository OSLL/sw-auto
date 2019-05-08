using System;
using System.Collections.Generic;
using System.Linq;
using AnalyzeResults.Errors;

namespace AnalyzeResults.Presentation
{
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

        public List<Section> Sections { get; set; }

        public List<Criterion> Criteria { get; set; }

        public List<Error> Errors { get; set; }

        public string Error { get; set; }

        public bool IsScientific()
        {
            return Criteria.All(x => x.IsMet());
        }

        public double GetPaperGrade()
        {
            var baseValue = Criteria.Where(x => x is NumericalCriterion).Select(crit => (crit as NumericalCriterion).GetGradePart())
                .Aggregate((result, part) => result + part);
            var fines = Errors.Count * 2;
            return Math.Max(baseValue - fines, 0);
        }
    }
}
