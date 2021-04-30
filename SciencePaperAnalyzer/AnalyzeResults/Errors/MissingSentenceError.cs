using AnalyzeResults.Presentation;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using AnalyzeResults.Settings;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class MissingSentenceError : Error
    {
        public MissingSentenceError(Sentence errorSentence, double errorCost, double weight, List<ScopePair> grading, GradingType gType, bool isPlaceholder = false)
            : base(ErrorType.MissingSentence, "Missing Sentence", errorSentence?.ToStringVersion(),
                  "tip for Missing Sentence", errorCost, weight, grading, gType, isPlaceholder)
        {
            ErrorSentence = errorSentence;
        }

        [BsonElement("errorSentence")]
        public Sentence ErrorSentence { get; set; }
    }
}
