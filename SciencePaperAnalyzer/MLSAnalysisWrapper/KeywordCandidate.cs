using System;
using System.Collections.Generic;
using System.Text;

namespace MLSAnalysisWrapper
{
    public class KeywordCandidate
    {
        public String Text { get; set; }
        public float Score { get; set; }
        public override string ToString()
        {
            return $"[ {Text} ; {Score} ]";
        }
    }
}
