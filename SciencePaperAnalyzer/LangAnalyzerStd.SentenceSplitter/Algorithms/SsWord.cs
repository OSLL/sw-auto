using System.Text;

namespace LangAnalyzerStd.SentenceSplitter
{
    unsafe internal sealed class SsWord
    {
        public SsWord(char* _startPtr, int _length)
        {
            startPtr = _startPtr;
            length = _length;
            valueOriginal = new string(_startPtr, 0, _length);
        }

        public string valueOriginal;
        public string valueUpper;
        public char* startPtr;
        public int length;
        public SsWord prev;
        public SsWord next;

        public char* EndPtr()
        {
            return startPtr + length;
        }

        public bool HasPrev
        {
            get { return prev != null; }
        }

        public bool HasNext
        {
            get { return next != null; }
        }

        public override string ToString()
        {
            return $"{'\''}{valueOriginal}' [0x{((long)startPtr).ToString("x")}:{length}]";
        }

        public string GetAllWordsChain()
        {
            var sb = new StringBuilder();
            for (var w = this; w != null; w = w.next)
            {
                sb.Append(w.valueOriginal).Append(' ');
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }
}
