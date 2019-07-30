using LangAnalyzerStd.Postagger;

namespace AnalyzeResults.Presentation
{
    public class Word
    {
        public Word(string original, PosTaggerOutputType type, int startIndex = -1)
        {
            Original = original;
            Type = type;
            StartIndex = startIndex;
            ErrorCodes = "";
        }

        public string Original { get; }

        public PosTaggerOutputType Type { get; }

        public int StartIndex { get; }

        public bool HasErrors { get; set; }

        public string ErrorCodes { get; set; }
    }
}
