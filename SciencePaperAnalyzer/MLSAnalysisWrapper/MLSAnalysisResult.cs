using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLSAnalysisWrapper
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class MLSAnalysisResult
    {
        public string Status {get; set;}
        public List<ParagraphCoherence> Coherence { get; set; }

        public List<KeywordCandidate> UserPhrases { get; set; }
        public List<KeywordCandidate> KeywordCandidates { get; set; }
        public static MLSAnalysisResult Parse(string inp)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true }
                }
            };
            var result = JsonConvert.DeserializeObject<MLSAnalysisResult>(inp, settings);
            return result;
        }
        public override string ToString()
        {
            string s = "";
            if (Coherence != null)
                foreach (var c in Coherence)
                {
                    s += c.ToString() + "\n";
                }
            if (UserPhrases != null)
                foreach (var c in UserPhrases)
                {
                    s += c.ToString() + "\n";
                }
            if (KeywordCandidates != null)
                foreach (var c in KeywordCandidates)
                {
                    s += c.ToString() + "\n";
                }
            return s;
        }
    }
}
