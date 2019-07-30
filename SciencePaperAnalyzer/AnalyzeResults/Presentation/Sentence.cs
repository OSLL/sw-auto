using System.Collections.Generic;
using System.Linq;
using System.Text;
using LangAnalyzerStd.Postagger;

namespace AnalyzeResults.Presentation
{
    public class Sentence
    {
        public Sentence(SentenceType type)
        {
            Words = new List<Word>();
            SentenceType = type;
        }

        public Sentence(SentenceType type, IEnumerable<Word> words) : this(type)
        {
            Words.AddRange(words);
        }

        public List<Word> Words { get; set; }

        public SentenceType SentenceType { get; set; }

        public string Original { get; set; }

        public string ToStringVersion()
        {
            if (SentenceType == SentenceType.Reference)
                return Original;

            var sb = new StringBuilder();

            for (int i = 0; i < Words.Count(); i++)
            {
                if (i == 0)
                {
                    sb.Append(Words[i].Original);
                    continue;
                }

                if (!(Words[i].Type == PosTaggerOutputType.Punctuation && Words[i].Original != "-") && Words[i].Type != PosTaggerOutputType.Numeral)
                {
                    sb.Append(" ");
                }

                sb.Append(Words[i].Original);
            }

            return sb.ToString();
        }
    }
}
