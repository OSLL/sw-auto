using System;

namespace LangAnalyzerStd.Morphology
{
    /// <summary>
    /// части речи
    /// </summary>
    [Flags]
    public enum PartOfSpeechEnum : ushort
    {
        /// <summary>
        /// Другое
        /// </summary>
        Other = 0x0,
        /// <summary>
        /// Существительное
        /// </summary>
        Noun = 0x1,
        /// <summary>
        /// Прилагательное
        /// </summary>
        Adjective = (1 << 1),
        /// <summary>
        /// Местоимение
        /// </summary>
        Pronoun = (1 << 2),
        /// <summary>
        /// Числительное
        /// </summary>
        Numeral = (1 << 3),
        /// <summary>
        /// Глагол
        /// </summary>
        Verb = (1 << 4),
        /// <summary>
        /// Наречие
        /// </summary>
        Adverb = (1 << 5),
        /// <summary>
        /// Союз
        /// </summary>
        Conjunction = (1 << 6),
        /// <summary>
        /// Предлог
        /// </summary>
        Preposition = (1 << 7),
        /// <summary>
        /// Междометие
        /// </summary>
        Interjection = (1 << 8),
        /// <summary>
        /// Частица
        /// </summary>
        Particle = (1 << 9),
        /// <summary>
        /// Артикль
        /// </summary>
        Article = (1 << 10),
        /// <summary>
        /// Сказуемое
        /// </summary>
        Predicate = (1 << 11)
    }
}
