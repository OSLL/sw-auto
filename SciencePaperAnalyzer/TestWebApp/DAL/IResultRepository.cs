using System;
using WebPaperAnalyzer.Models;

namespace WebPaperAnalyzer.DAL
{
    public interface IResultRepository
    {
        void AddResult(AnalysisResult result);

        AnalysisResult GetResult(string id);
    }
}
