using System;

namespace AnalyzeResults.Errors
{
    public class ShortSectionError : Error
    {
        public ShortSectionError(Guid sectionId, string sectionName, int sentencesNum)
            : base(ErrorType.ShortSection, "Короткий раздел", $"Предложений в разделе \"{sectionName}\": {sentencesNum}",
                  "В разделе меньше трёх предложений. Постарайтесь расширить раздел, либо уберите его.")
        {
            SectionId = sectionId;
        }

        public Guid SectionId { get; }
    }
}
