using MLSAnalysisWrapper;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using WebPaperAnalyzer.Models;

namespace WebPaperAnalyzer.DAL
{
    public interface IResultRepository
    {
        void AddResult(AnalysisResult result);

        void UpdateResult(ObjectId id, AnalysisResult result);

        AnalysisResult GetResult(ObjectId id);

        IEnumerable<AnalysisResult> GetResultsByLogin(string login, bool type);
    }
}
