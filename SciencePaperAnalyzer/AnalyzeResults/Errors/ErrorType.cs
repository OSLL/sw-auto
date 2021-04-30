using System;

namespace AnalyzeResults.Errors
{
    [Serializable]
    public enum ErrorType
    {
        UseOfPersonalPronouns,
        UseOfForbiddenWord,
        SourceNotReferenced,
        ShortSection,
        Other,
        PictureNotReferenced,
        TableNotReferenced,
        DiscordantSentence,
        MissingSentence
    }
}
