using AnalyzeResults.Presentation;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using AnalyzeResults.Settings;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class DiscordantSentenceError : Error
    {
        public DiscordantSentenceError(Sentence errorSentence, double errorCost, double weight, List<ScopePair> grading, GradingType gType, bool isPlaceholder = false)
            : base(ErrorType.DiscordantSentence, "Предложение, не связанное по смыслу с остальным текстом", errorSentence?.ToStringVersion(),
                  "Убедитесь, что предложение действительно по смыслу необходимо в данном фрагменте", errorCost, weight, grading, gType, isPlaceholder)
        {
            ErrorSentence = errorSentence;
        }

        [BsonElement("errorSentence")]
        public Sentence ErrorSentence { get; set; }
    }
}
