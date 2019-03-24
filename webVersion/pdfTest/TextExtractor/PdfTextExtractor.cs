using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;

namespace TextExtractor
{
    /// <summary>
    /// Text extractor from pdf files
    /// </summary>
    public class PdfTextExtractor : ITextExtractor
    {
        /// <summary>
        /// Path to .pdf file
        /// </summary>
        private string FilePath { get; set; }

        /// <summary>
        /// List of strings, each being text from page
        /// </summary>
        private List<string> PageTexts { get; set; }

        /// <summary>
        /// Flag showing was text extracted or not
        /// </summary>
        private bool TextExtracted { get; set; }

        public PdfTextExtractor(string path)
        {
            PageTexts = new List<string>();
            FilePath = path;
        }

        public string GetAllText()
        {
            if (!TextExtracted)
                ExtractTextFromFile();

            var sb = new StringBuilder();
            foreach (var pageText in PageTexts)
                sb.Append(pageText);

            return sb.ToString();
        }

        public List<string> GetTextByPages()
        {
            if (!TextExtracted)
                ExtractTextFromFile();
            return PageTexts;
        }

        private void ExtractTextFromFile()
        {
            var reader = new PdfReader(FilePath);

            for (int page = 1; page <= reader.NumberOfPages; page++)
                PageTexts.Add(iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, page));

            TextExtracted = true;
            reader.Close();
        }
    }
}
