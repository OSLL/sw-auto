using AnalyzeResults.Presentation;
using AnalyzeResults.Settings;
using TextExtractor;

namespace PaperAnalyzer.Service
{
    public interface IPaperAnalyzerService
    {
        ITextExtractor GetTextExtractor(string path);

        PaperAnalysisResult GetAnalyze(UploadFile file, string titles, string paperName, string refsName,
            ResultScoreSettings settings);
    }
}
