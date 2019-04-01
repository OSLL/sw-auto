using System.Collections.Generic;

namespace TextExtractor
{
    /// <summary>
    /// Basic interface for text-extractor implementations
    /// </summary>
    public interface ITextExtractor
    {
        /// <summary>
        /// Get all text from file
        /// </summary>
        /// <returns>All text from file as string</returns>
        string GetAllText();

        /// <summary>
        /// Get all text from each page
        /// </summary>
        /// <returns>List of string, each being text from page</returns>
        List<string> GetTextByPages();
    }
}
