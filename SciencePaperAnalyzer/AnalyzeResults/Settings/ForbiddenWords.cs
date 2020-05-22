using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebPaperAnalyzer.Models
{
    public class ForbiddenWords
    {
        [BsonId]
        public string Name { get; set; }

        public IEnumerable<string> Words { get; set; }

    }
}
