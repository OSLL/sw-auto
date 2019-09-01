using LangAnalyzerStd.Postagger;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AnalyzeResults.Presentation
{
    [Serializable]
    public class Word
    {
        public Word(string original, PosTaggerOutputType type, int startIndex = -1)
        {
            Original = original;
            Type = type;
            StartIndex = startIndex;
            ErrorCodes = "";
        }

        [BsonElement("original")]
        public string Original { get; }

        [BsonElement("type")]
        public PosTaggerOutputType Type { get; }

        [BsonElement("startindex")]
        public int StartIndex { get; }

        [BsonElement("haserrors")]
        public bool HasErrors { get; set; }

        [BsonElement("errorcodes")]
        public string ErrorCodes { get; set; }
    }
}
