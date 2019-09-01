using AnalyzeResults.Presentation;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebPaperAnalyzer.Models
{
    public class AnalysisResult
    {
        [BsonId]
        public string Id { get; set; }
        [BsonElement("result")]
        public PaperAnalysisResult Result { get; set; }
    }
}
