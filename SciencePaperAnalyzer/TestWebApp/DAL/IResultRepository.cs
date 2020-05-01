using System;
using System.Collections.Generic;
using WebPaperAnalyzer.Models;

namespace WebPaperAnalyzer.DAL
{
    public interface IResultRepository
    {
        void AddResult(AnalysisResult result);

        AnalysisResult GetResult(string id);

        IEnumerable<AnalysisResult> GetResultsByLogin(string login, bool type);
    }
}
