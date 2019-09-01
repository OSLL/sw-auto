using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AnalyzeResults.Presentation
{
    [Serializable]
    public class Reference
    {
        public Reference(Sentence original, int number)
        {
            Original = original;
            Number = number;
        }

        [BsonElement("original")]
        public Sentence Original { get; set; }

        [BsonElement("number")]
        public int Number { get; set; }

        [BsonElement("year")]
        public int Year { get; set; }

        [BsonElement("referedto")]
        public bool ReferedTo { get; set; }
    }
}
