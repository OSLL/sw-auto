using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using AnalyzeResults.Settings;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public class ShortSectionError : Error
    {
        public ShortSectionError(Guid sectionId, string sectionName, int sentencesNum, double errorCost, double weight, List<ScopePair> grading, GradingType gType)
            : base(ErrorType.ShortSection, "Короткий раздел", $"Предложений в разделе \"{sectionName}\": {sentencesNum}",
                  "В разделе меньше трёх предложений. Постарайтесь расширить раздел, либо уберите его.", errorCost, weight, grading, gType)
        {
            SectionId = sectionId;
        }

        [BsonElement("id")]
        public Guid SectionId { get; }
    }
}
