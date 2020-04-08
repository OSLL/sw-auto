using System.IO;
using System.Text;
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
                buffer.Append(loadedDocument.Pages[i].ExtractText(true));
            }

            return buffer.ToString();
        }
    }
}
