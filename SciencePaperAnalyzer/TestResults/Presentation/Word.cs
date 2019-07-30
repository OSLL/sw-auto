using LangAnalyzer.Postagger;
using System.Collections.Generic;
using AnalyzeResults.Errors;

namespace AnalyzeResults.Presentation
{
    public class Word
    {
        public Word(string original, PosTaggerOutputType type)
        {
            Original = original;
            Type = type;
            Errors = new List<Error>();
        }

        public string Original { get; set; }

        public PosTaggerOutputType Type { get; set; }

        public List<Error> Errors { get; set; }
    }
}
