using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Syncfusion.Pdf.Parsing;

namespace TextExtractor
{
    /// <summary>
    /// Text extractor from pdf files
    /// </summary>
    public class PdfTextExtractor : ITextExtractor
    {

        public string ExtractTextFromFileStream(Stream fileStream)
        {
            var loadedDocument = new PdfLoadedDocument(fileStream);
            var buffer = new StringBuilder();
            for (var i = 0; i < loadedDocument.Pages.Count; i++)
            {
                string page = loadedDocument.Pages[i].ExtractText(true);
                // monkey patch the string, due to this pdf extractor occasionally adds an 'a' to the end of the line
                string replaced = Regex.Unescape("a\\ \\r");
                string replacedWith = Regex.Unescape("\\ \\r");
                page = page.Replace(replaced, replacedWith);
                buffer.Append(page);
            }
            return buffer.ToString();
        }
    }
}
