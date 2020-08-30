using System.Collections.Generic;
using AnalyzeResults.Presentation;
using AnalyzeResults.Settings;

namespace PaperAnalyzer
{
	public interface IPaperAnalyzer
	{
		PaperAnalysisResult ProcessTextWithResult(string text, 
            string titlesString, string paperName, string refsName, string keywords,
			ResultScoreSettings settings);
	}
}
