using LangAnalyzer.Tokenizing;

namespace LangAnalyzer.Postagger
{
    public struct PosTaggerInputTypeResult
    {
        private PosTaggerInputTypeResult(PosTaggerInputType _posTaggerInputType)
        {
            posTaggerInputType = _posTaggerInputType;
            posTaggerExtraWordType = PosTaggerExtraWordType.__DEFAULT__;
            posTaggerLastValueUpperInNumeralChain = null;
            posTaggerLastValueUpperInNumeralChainIsValueOriginal = false;
        }

        private PosTaggerInputTypeResult(PosTaggerInputType _posTaggerInputType, PosTaggerExtraWordType _posTaggerExtraWordType)
        {
            posTaggerInputType = _posTaggerInputType;
            posTaggerExtraWordType = _posTaggerExtraWordType;
            posTaggerLastValueUpperInNumeralChain = null;
            posTaggerLastValueUpperInNumeralChainIsValueOriginal = false;
        }

        public string posTaggerLastValueUpperInNumeralChain;
        public bool posTaggerLastValueUpperInNumeralChainIsValueOriginal;
        public PosTaggerInputType posTaggerInputType;
        public PosTaggerExtraWordType posTaggerExtraWordType;

        public static readonly PosTaggerInputTypeResult Num = new PosTaggerInputTypeResult(PosTaggerInputType.Num);
        public static readonly PosTaggerInputTypeResult AllLat = new PosTaggerInputTypeResult(PosTaggerInputType.AllLat);
        public static readonly PosTaggerInputTypeResult Col = new PosTaggerInputTypeResult(PosTaggerInputType.Col, PosTaggerExtraWordType.Punctuation);
        public static readonly PosTaggerInputTypeResult Com = new PosTaggerInputTypeResult(PosTaggerInputType.Com, PosTaggerExtraWordType.Punctuation);
        public static readonly PosTaggerInputTypeResult Dash = new PosTaggerInputTypeResult(PosTaggerInputType.Dash, PosTaggerExtraWordType.Punctuation);
        public static readonly PosTaggerInputTypeResult FstC = new PosTaggerInputTypeResult(PosTaggerInputType.FstC);
        public static readonly PosTaggerInputTypeResult OneCP = new PosTaggerInputTypeResult(PosTaggerInputType.OneCP);
        public static readonly PosTaggerInputTypeResult O = new PosTaggerInputTypeResult(PosTaggerInputType.O);
        public static readonly PosTaggerInputTypeResult IsAbbreviation = new PosTaggerInputTypeResult(PosTaggerInputType.O, PosTaggerExtraWordType.Abbreviation);
        public static readonly PosTaggerInputTypeResult IsPunctuation = new PosTaggerInputTypeResult(PosTaggerInputType.O, PosTaggerExtraWordType.Punctuation);

        public static PosTaggerInputTypeResult CreateNum(string _posTaggerLastValueUpperInNumeralChain)
        {
            var r = new PosTaggerInputTypeResult(PosTaggerInputType.Num)
            {
                posTaggerLastValueUpperInNumeralChain = _posTaggerLastValueUpperInNumeralChain,
            };
            return (r);
        }
        private static readonly PosTaggerInputTypeResult _Num_1 = new PosTaggerInputTypeResult(PosTaggerInputType.Num) { posTaggerLastValueUpperInNumeralChainIsValueOriginal = true };
        public static PosTaggerInputTypeResult CreateNum()
        {
            return (_Num_1);
        }
    }

    public interface IPosTaggerInputTypeProcessor
    {
        unsafe PosTaggerInputTypeResult GetResult(char* _base, int length, Word word);
    }

    public interface IPosTaggerInputTypeProcessorFactory
    {
        IPosTaggerInputTypeProcessor CreateInstance();
    }

    internal sealed class DummyPosTaggerInputTypeProcessor : IPosTaggerInputTypeProcessor
    {
        public static readonly DummyPosTaggerInputTypeProcessor Instance = new DummyPosTaggerInputTypeProcessor();
        private DummyPosTaggerInputTypeProcessor() { }

        public unsafe PosTaggerInputTypeResult GetResult(char* _base, int length, Word word) //, string valueUpper )
        {
            return (PosTaggerInputTypeResult.O);
        }
    }
}
