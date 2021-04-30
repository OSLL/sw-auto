using AnalyzeResults.Presentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MLSAnalysisWrapper
{
    class RequestObject
    {
        [JsonProperty("article")]
        public List<List<string>> ArticleContent { get; set; }
        [JsonProperty("keywords")]
        public List<string> KeywordList { get; set; }
        [JsonProperty("callbackUrl")]
        public string CallbackUrl { get; set; }
    }
    public class MLSAnalysisAPIWrapper
    {
        //public const string BaseAddress = "http://localhost:6543";

        //static readonly HttpClient client = new HttpClient();
        //static MLSAnalysisAPIWrapper() // TODO: change to object model instead of static
        //{
        //    client.BaseAddress = new Uri(BaseAddress);
        //    client.Timeout = TimeSpan.FromMinutes(10);
        //}
        //public void SetBaseAddress(string baseAddress)
        //{
        //    client.BaseAddress = new Uri(baseAddress);
        //}
        private readonly HttpClient client;
        public MLSAnalysisAPIWrapper(string address)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(address);
            client.Timeout = TimeSpan.FromMinutes(10);
        }
        public async Task<string> Analyze(string articleContent, string keywordList, string callbackUrl = null)
        {
            var content = new FormUrlEncodedContent(new[]{
                    new KeyValuePair<string, string>("article", articleContent),
                    new KeyValuePair<string, string>("keyword", keywordList),
                    new KeyValuePair<string, string>("callbackUrl", callbackUrl)
                });
            //var response = await client.PostAsync("/full_analyze", content);
            var startTime = DateTime.Now;
            Console.WriteLine($"started at {startTime.ToShortTimeString()}");
            var response = await client.PostAsync("/full_analyze", content);
            response.EnsureSuccessStatusCode();
            var finishTime = DateTime.Now;
            Console.WriteLine($"done after {(finishTime - startTime).TotalMilliseconds}");

            var resultString = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"-> {resultString}");
            return resultString;
        }

        public List<List<string>> SectionToStringList(List<Section> sections)
        {
            List<List<string>> result = new List<List<string>>();
            foreach (var section in sections)
            {
                List<string> sentences = new List<string>();
                foreach (var sentence in section.Sentences)
                {
                    sentences.Add(sentence.ToStringVersion());
                }
                result.Add(sentences);

            }
            return result;
        }
        public async Task<string> Analyze(List<Section> articleContent, List<string> keywordList, string callbackUrl = null)
        {
            RequestObject requestObject = new RequestObject
            {
                ArticleContent = SectionToStringList(articleContent),
                KeywordList = keywordList,
                CallbackUrl = callbackUrl
            };
            string json = JsonConvert.SerializeObject(requestObject);
            Console.WriteLine(json);
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var startTime = DateTime.Now;
            Console.WriteLine($"started at {startTime.ToShortTimeString()}");
            var response = await client.PostAsync("/full_analyze", content);
            response.EnsureSuccessStatusCode();
            var finishTime = DateTime.Now;
            Console.WriteLine($"done after {(finishTime - startTime).TotalMilliseconds}");

            var resultString = await response.Content.ReadAsStringAsync();
            resultString = resultString.Replace('\"', ' ').Trim();
            Console.WriteLine($"-> {resultString}");
            return resultString;
        }

        public async Task<MLSAnalysisResult> GetResult(string result_id)
        {
            var response = await client.GetAsync("/check/" + result_id);

            var resultString = await response.Content.ReadAsStringAsync();

            return MLSAnalysisResult.Parse(resultString);
        }
        public async Task<bool> ForgetResult(string result_id)
        {
            var definition = new { Status = "" };
            var response = await client.GetAsync("/forget/" + result_id);

            var resultString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(resultString);
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true }
                }
            };
            var result = JsonConvert.DeserializeAnonymousType(resultString, definition, settings);
            //Console.WriteLine(result);
            return result.Status == "success";
        }
    }
}