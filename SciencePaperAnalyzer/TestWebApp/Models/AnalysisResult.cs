using AnalyzeResults.Presentation;
using MLSAnalysisWrapper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebPaperAnalyzer.Models
{
    public class AnalysisResult
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("result")]
        public PaperAnalysisResult Result { get; set; }
        [BsonElement("mlsResult")]
        public MLSAnalysisResult MLSResult { get; set; }

        public string StudentLogin { get; set; }
        public string TeacherLogin { get; set; }
        public string Criterion { get; set; }
    }
}
