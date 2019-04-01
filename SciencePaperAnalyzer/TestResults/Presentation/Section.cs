using System.Collections.Generic;
using System.Linq;

namespace TestResults.Presentation
{
    public class Section
    {
        public Section()
        {
            Sentences = new List<Sentence>();
        }

        public Section(IEnumerable<Sentence> sentences)
        {
            Sentences.AddRange(sentences);
        }

        public SectionType Type { get; set; }

        public List<Sentence> Sentences { get; set; }

        public string ToStringVersion()
        {
            switch (Type)
            {
                case SectionType.PaperTitle:
                    return $"<p style =\"font-weight: bold; font-size: 20px\">{Sentences[0].ToStringVersion()}</p>";
                case SectionType.SectionTitle:
                    return $"<p style =\"font-weight: bold; font-size: 16px\">{Sentences[0].ToStringVersion()}</p>";
                case SectionType.Text:
                    return $"<p style =\"font-size: 14px\">{string.Join(" ", Sentences.Select(x => x.ToStringVersion()))}</p>";
                case SectionType.ReferencesList:
                    return $"<p style =\"font-size: 14px\">{string.Join("\n", Sentences.Select(x => x.Original))}</p>";
                default:
                    return "Что то не так";
            }
        }
    }
}
