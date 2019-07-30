namespace LangAnalyzerStd.Ner
{
    public interface INerInputTypeProcessor
    {
        unsafe NerInputType GetNerInputType(char* _base, int length);
    }

    public interface INerInputTypeProcessorFactory
    {
        INerInputTypeProcessor CreateInstance();
    }

    internal sealed class DummyNerInputTypeProcessor : INerInputTypeProcessor
    {
        public static readonly DummyNerInputTypeProcessor Instance = new DummyNerInputTypeProcessor();
        private DummyNerInputTypeProcessor() { }

        public unsafe NerInputType GetNerInputType(char* _base, int length)
        {
            return NerInputType.O;
        }
    }
}
