using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using AnalyzeResults.Settings;
using MongoDB.Bson;

namespace AnalyzeResults.Errors
{
    [Serializable]
    [BsonKnownTypes(typeof(MissingSentenceError),
        typeof(DiscordantSentenceError),
        typeof(PictureNotReferencedError),
        typeof(ShortSectionError),
        typeof(SourceNotReferencedError),
        typeof(TableNotReferencedError),
        typeof(UseOfForbiddenWordsError),
        typeof(UseOfPersonalPronounsError)
        )]
    public abstract class Error
    {
        public Error(ErrorType type, string name, string explanation, string tip, double errorCost, double weight,
                     List<ScopePair> grading, GradingType gType, bool isPlaceholder = false)
        {
            ErrorType = type;
            Name = name;
            Explanation = explanation;
            Tip = tip;
            ErrorCost = errorCost;
            Weight = weight;
            Grading = grading;
            GradingType = gType;
            IsPlaceholder = isPlaceholder;
        }

        [BsonElement("name")]
        public string Name { get; private set; }

        [BsonElement("explanation")]
        public string Explanation { get; private set; }

        [BsonElement("tip")]
        public string Tip { get; private set; }

        [BsonElement("errortype")]
        [BsonRepresentation(BsonType.String)]
        public ErrorType ErrorType { get; private set; }

        [BsonElement("errorcost")]
        public double ErrorCost { get; private set; }

        [BsonElement("weight")]
        public double Weight { get; private set; }

        [BsonElement("grading")]
        public List<ScopePair> Grading { get; private set; }

        [BsonElement("isPlaceholder")]
        public bool IsPlaceholder { get; private set; }

        [BsonElement("gradingtype")]
        [BsonRepresentation(BsonType.String)]
        public GradingType GradingType { get; private set; }
    }
}
