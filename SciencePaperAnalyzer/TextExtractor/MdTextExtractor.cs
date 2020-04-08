using System.IO;
using System.Text;

namespace TextExtractor
{
    public class MdTextExtractor : ITextExtractor
    {
        public string ExtractTextFromFileStream(Stream fileStream)
        {
            fileStream.Position = 0;
            using (var reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                var result = reader.ReadToEnd();
                return result;
            }
        }
    }
}
