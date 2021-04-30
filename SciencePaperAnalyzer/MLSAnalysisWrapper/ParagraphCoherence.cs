using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MLSAnalysisWrapper
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ParagraphCoherence
    {
        public Boolean HasMissing { get; set; }
        public List<Boolean> MissingSentences { get; set; }

        public Boolean HasIncoherent { get; set; }
        public List<Boolean> IncoherentSentences { get; set; }
        public Boolean IsSkipped { get { return MissingSentences == null || IncoherentSentences == null; } }
        public override string ToString()
        {
            var s1 = MissingSentences!=null ? new string(MissingSentences.Select(x => x ? '1' : '0').ToArray()): "null";
            var s2 = MissingSentences != null ? new string(IncoherentSentences.Select(x => x ? '1' : '0').ToArray()) : "null";
            return $"[ {HasMissing} ; {HasIncoherent} ; {s1} ; {s2} ]";
        }

    }
}
