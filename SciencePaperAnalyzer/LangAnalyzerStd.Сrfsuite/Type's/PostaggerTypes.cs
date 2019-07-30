namespace LangAnalyzerStd.Postagger
{
    public enum PosTaggerInputType : byte
    {
        #region common
        O, // other's (другой)
        Com,    // – запятая;
        Dash,   // – тире;
        Col,    // – двоеточие.        
        OneCP,  // - первая заглавная с точкой;
        FstC,   // - первая заглавная, не содержит пробелов;
        Num,    // – содержит хотя бы одну цифру и не содержит букв;
        CompPh, // - составные (имеющие хотя бы один пробел);
        #endregion

        #region russian-language
        AllLat, // - только латиница: нет строчных и точек;
        #endregion
    }

    public enum PosTaggerOutputType : byte
    {
        Other = 0, //  = Other

        Article,                // = Article
        Adjective,              // = Adj
        AdjectivePronoun,       // = AdjPron
        Adverb,                 // = Adv
        AdverbialParticiple,    // = AdvPart
        AdverbialPronoun,       // = AdvPron
        AuxiliaryVerb,          // = AuxVerb
        Conjunction,            // = Conj
        Gerund,
        Infinitive,             // = Inf
        Interjection,           // = Intr
        ModalVerb,
        Noun,                   // = Noun
        Numeral,                // = Num
        PastParticiple,
        Participle,             // = Part
        Particle,               // = Pr
        PossessivePronoun,      // = PosPron
        Predicate,              // = Pred
        Preposition,            // = Prep
        Pronoun,                // = Pron
        Punctuation,            // = Punct
        Verb,                   // = Verb

        #region en
        /*
        Gerund         (L) – Герундий
        ModalVerb      (K) – модальный глагол
        PastParticiple (B) – Причастие прошедшего времени

        отсутствуют 
        AdverbialParticiple –  Деепричастие
        AdverbialPronoun    –  Местоимённое наречие          
        */
        #endregion
    }

    public enum PosTaggerExtraWordType : byte
    {
        __DEFAULT__,

        Abbreviation,
        Punctuation,
    }

    public static class PosTaggerExtensions
    {
        public static string ToText(this PosTaggerInputType posTaggerInputType)
        {
            switch (posTaggerInputType)
            {
                case PosTaggerInputType.Num: return "Num";    // – содержит хотя бы одну цифру и не содержит букв;
                case PosTaggerInputType.AllLat: return "AllLat"; // - только латиница: нет строчных и точек;
                case PosTaggerInputType.OneCP: return "OneCP";  // - первая заглавная с точкой;
                case PosTaggerInputType.CompPh: return "CompPh"; // - составные (имеющие хотя бы один пробел);
                case PosTaggerInputType.FstC: return "FstC";   // - первая заглавная, не содержит пробелов;
                case PosTaggerInputType.Com: return "Com";    // – запятая;
                case PosTaggerInputType.Dash: return "Dush";   // – тире;
                case PosTaggerInputType.Col: return "Col";    // – двоеточие.	
                default: return "O";
            }
        }
        public static string ToText(this PosTaggerOutputType posTaggerOutputType)
        {
            switch (posTaggerOutputType)
            {
                case PosTaggerOutputType.Article: return "Article";
                case PosTaggerOutputType.Adjective: return "Adj";
                case PosTaggerOutputType.AdjectivePronoun: return "AdjPron";
                case PosTaggerOutputType.Adverb: return "Adv";
                case PosTaggerOutputType.AdverbialParticiple: return "AdvPart";
                case PosTaggerOutputType.AdverbialPronoun: return "AdvPron";
                case PosTaggerOutputType.AuxiliaryVerb: return "AuxVerb";
                case PosTaggerOutputType.Conjunction: return "Conj";
                case PosTaggerOutputType.Gerund: return "Gerund";
                case PosTaggerOutputType.Infinitive: return "Inf";
                case PosTaggerOutputType.Interjection: return "Intr";
                case PosTaggerOutputType.ModalVerb: return "ModalVerb";
                case PosTaggerOutputType.Noun: return "Noun";
                case PosTaggerOutputType.Numeral: return "Num";
                case PosTaggerOutputType.PastParticiple: return "PastParticiple";
                case PosTaggerOutputType.Participle: return "Part";
                case PosTaggerOutputType.Particle: return "Pr";
                case PosTaggerOutputType.PossessivePronoun: return "PosPron";
                case PosTaggerOutputType.Predicate: return "Pred";
                case PosTaggerOutputType.Preposition: return "Prep";
                case PosTaggerOutputType.Pronoun: return "Pron";
                case PosTaggerOutputType.Punctuation: return "Punct";
                case PosTaggerOutputType.Verb: return "Verb";
                default: return "Other";
            }
        }

        public static char ToCrfChar(this PosTaggerInputType posTaggerInputType)
        {
            return (char)posTaggerInputType.ToCrfByte();
        }
        public static byte ToCrfByte(this PosTaggerInputType posTaggerInputType)
        {
            switch (posTaggerInputType)
            {
                case PosTaggerInputType.Num: return (byte)'N';  // – содержит хотя бы одну цифру и не содержит букв;
                case PosTaggerInputType.AllLat: return (byte)'L';  // - только латиница: нет строчных и точек;
                case PosTaggerInputType.OneCP: return (byte)'P';  // - первая заглавная с точкой;
                case PosTaggerInputType.CompPh: return (byte)'H';  // - составные (имеющие хотя бы один пробел);
                case PosTaggerInputType.FstC: return (byte)'F';  // - первая заглавная, не содержит пробелов;
                case PosTaggerInputType.Com: return (byte)'M';  // – запятая;
                case PosTaggerInputType.Dash: return (byte)'D';  // – тире;
                case PosTaggerInputType.Col: return (byte)'C';  // – двоеточие.	
                default: return (byte)'O';
            }
        }

        public static char ToCrfChar(this PosTaggerOutputType posTaggerOutputType)
        {
            return (char)posTaggerOutputType.ToCrfByte();
        }
        public static byte ToCrfByte(this PosTaggerOutputType posTaggerOutputType)
        {
            switch (posTaggerOutputType)
            {
                case PosTaggerOutputType.Adjective: return (byte)'J';
                case PosTaggerOutputType.AdjectivePronoun: return (byte)'R';
                case PosTaggerOutputType.Adverb: return (byte)'D';
                case PosTaggerOutputType.AdverbialParticiple: return (byte)'X';
                case PosTaggerOutputType.AdverbialPronoun: return (byte)'H';
                case PosTaggerOutputType.Article: return (byte)'A';
                case PosTaggerOutputType.AuxiliaryVerb: return (byte)'G';
                case PosTaggerOutputType.Conjunction: return (byte)'C';
                case PosTaggerOutputType.Gerund: return (byte)'L';
                case PosTaggerOutputType.Infinitive: return (byte)'F';
                case PosTaggerOutputType.Interjection: return (byte)'I';
                case PosTaggerOutputType.ModalVerb: return (byte)'K';
                case PosTaggerOutputType.Noun: return (byte)'N';
                case PosTaggerOutputType.Numeral: return (byte)'M';
                case PosTaggerOutputType.PastParticiple: return (byte)'B';
                case PosTaggerOutputType.Participle: return (byte)'Z';
                case PosTaggerOutputType.Particle: return (byte)'W';
                case PosTaggerOutputType.PossessivePronoun: return (byte)'S';
                case PosTaggerOutputType.Preposition: return (byte)'E';
                case PosTaggerOutputType.Pronoun: return (byte)'Y';
                case PosTaggerOutputType.Punctuation: return (byte)'T';
                case PosTaggerOutputType.Verb: return (byte)'V';
                default: return (byte)'O';
            }
        }

        unsafe public static PosTaggerOutputType ToPosTaggerOutputType(byte* value)
        {
            switch (*value)
            {
                case (byte)'J': return PosTaggerOutputType.Adjective;
                case (byte)'R': return PosTaggerOutputType.AdjectivePronoun;
                case (byte)'D': return PosTaggerOutputType.Adverb;
                case (byte)'X': return PosTaggerOutputType.AdverbialParticiple;
                case (byte)'H': return PosTaggerOutputType.AdverbialPronoun;
                case (byte)'A': return PosTaggerOutputType.Article;
                case (byte)'G': return PosTaggerOutputType.AuxiliaryVerb;
                case (byte)'C': return PosTaggerOutputType.Conjunction;
                case (byte)'L': return PosTaggerOutputType.Gerund;
                case (byte)'F': return PosTaggerOutputType.Infinitive;
                case (byte)'I': return PosTaggerOutputType.Interjection;
                case (byte)'K': return PosTaggerOutputType.ModalVerb;
                case (byte)'N': return PosTaggerOutputType.Noun;
                case (byte)'M': return PosTaggerOutputType.Numeral;
                case (byte)'B': return PosTaggerOutputType.PastParticiple;
                case (byte)'Z': return PosTaggerOutputType.Participle;
                case (byte)'W': return PosTaggerOutputType.Particle;
                case (byte)'S': return PosTaggerOutputType.PossessivePronoun;
                case (byte)'E': return PosTaggerOutputType.Preposition;
                case (byte)'Y': return PosTaggerOutputType.Pronoun;
                case (byte)'V': return PosTaggerOutputType.Verb;
                case (byte)'T': return PosTaggerOutputType.Punctuation;
                default: return PosTaggerOutputType.Other;
            }
        }
    }
}
