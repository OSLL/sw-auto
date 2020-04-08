using LangAnalyzerStd.Morphology;
using LangAnalyzerStd.Postagger;

namespace PaperAnalyzer
{
	public interface IPaperAnalyzerEnvironment
	{
		MorphoAmbiguityResolverModel MorphoAmbiguityResolverModel { get; }
		IMorphoModel MorphoModel { get; }
		PosTaggerProcessor Processor { get; }
	}
}
