using AnalyzeResults.Errors;
using AnalyzeResults.Presentation;
using AnalyzeResults.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MLSAnalysisWrapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using WebPaperAnalyzer.DAL;
using MongoDB.Bson;

namespace WebPaperAnalyzer.Services
{
    public interface IMLSAnalysisService
    {
        Task<MLSAnalysisResult> GetMLSAnalysisAsync(string resultId, string articleContent, string keywordList);
        Task<MLSAnalysisResult> GetMLSAnalysisAsync(string resultId, List<Section> articleContent, List<string> keywordList);

        void UpdateToDatabase(ObjectId resultId, MLSAnalysisResult mlsResult);
    }
    public class MLSAnalysisService : IMLSAnalysisService
    {
        IConfiguration _appConfig;
        protected IResultRepository Repository;
        MLSAnalysisAPIWrapper wrapper = null;
        public MLSAnalysisService(IResultRepository repository, IConfiguration appConfig = null)
        {
            _appConfig = appConfig;
            Repository = repository;
            if (appConfig != null)
            {
                string address = _appConfig.GetValue("MLSAnalysisServiceBaseAddress", "http://localhost:6543/");
                wrapper = new MLSAnalysisAPIWrapper(address);
            }
        }

        public async Task<MLSAnalysisResult> GetMLSAnalysisAsync(string resultId, string articleContent, string keywordList)
        {
            string callbackUrl = _appConfig.GetValue<string>("Domain", "-");
            Console.WriteLine("callback url: "+callbackUrl);
            if (!callbackUrl.Equals("-"))
            {
                // send article to the service with a callback url, which the server will call after it finishes processing
                callbackUrl = $"{callbackUrl}/WebHook/{resultId}";
                Console.WriteLine("callback url: "+callbackUrl);
                await wrapper.Analyze(articleContent, keywordList, callbackUrl);
                return null;
            }
            else
            {
                // send article to the service then call FetchResult, which will continuously polling the server for processing status
                string jobId = await wrapper.Analyze(articleContent, keywordList);
                return await FetchResult(jobId, ObjectId.Parse(resultId));
            }
        }
        // similar to above, just an overload for input types (structured vs unstructured)
        public async Task<MLSAnalysisResult> GetMLSAnalysisAsync(string resultId, List<Section> articleContent, List<string> keywordList)
        {
            string callbackUrl = _appConfig.GetValue<string>("Domain", "-");
            Console.WriteLine("callback url: "+callbackUrl);
            if (!callbackUrl.Equals("-"))
            {
                callbackUrl = $"{(callbackUrl.EndsWith('/')?callbackUrl:callbackUrl+'/')}WebHook/{resultId}";
                Console.WriteLine("callback url: "+callbackUrl);
                await wrapper.Analyze(articleContent, keywordList, callbackUrl);
                return null;
            }
            else
            {
                string jobId = await wrapper.Analyze(articleContent, keywordList);
                return await FetchResult(jobId, ObjectId.Parse(resultId));
            }
        }

        private async Task<MLSAnalysisResult> FetchResult(string jobId, ObjectId analysisResultId)
        {
            MLSAnalysisResult result = null;
            int count = 0;
            while (count < 150 && (result == null || result.Status == "PENDING"))
            {
                await Task.Delay(2000);
                result = await wrapper.GetResult(jobId);
                Console.WriteLine(result);
                // maximum wait time (150 ~ 5 mins)
                // TODO: configurable parameter
                count += 1;
            }
            if (result != null && result.Status != "PENDING")
            {
                // insert back to database
                UpdateToDatabase(analysisResultId, result);

                await wrapper.ForgetResult(jobId);
                return result;
            }
            return null;
        }

        public void UpdateToDatabase(ObjectId resultId, MLSAnalysisResult mlsResult)
        {
            var result = Repository.GetResult(resultId);
            if (result != null)
            {
                var criteriaList = result.Result.Criteria;
                var errorList = result.Result.Errors;

                var baseKeywordsQualityCriterion = criteriaList.Find(e => e.Name == "Keywords Quality");

                // TODO: somehow e.ErrorType is incorrectly deserialized (all set to default value of UseOfPersonalPronouns), but type check still works
                foreach (var error in errorList)
                {
                    Console.WriteLine(error.ErrorType + "   " + error.GetType().Name);
                }
                var baseDiscordantSentenceError = errorList.Find(e => e is DiscordantSentenceError);
                var baseMissingSentenceError = errorList.Find(e => e is MissingSentenceError);

                // Updating Criterion
                criteriaList.Add(CreateKeywordsQualityCriterion(mlsResult, (NumericalCriterion)baseKeywordsQualityCriterion));
                // Updating Errors
                var discordantErrors = CreateDiscordantSentenceErrorList(result.Result, mlsResult, baseDiscordantSentenceError);
                var missingErrors = CreateMissingSentenceErrorList(result.Result, mlsResult, baseMissingSentenceError);

                errorList.AddRange(discordantErrors);
                errorList.AddRange(missingErrors);

                // TODO: check exists?
                //TODO: only remove the base errors if the list of errors found has length > 0
                criteriaList.Remove(baseKeywordsQualityCriterion);
                if (missingErrors.Count == 0)
                {
                    errorList.Add(new MissingSentenceError(null, -1, baseMissingSentenceError.Weight, baseMissingSentenceError.Grading, baseMissingSentenceError.GradingType));
                }
                errorList.Remove(baseMissingSentenceError);

                if (discordantErrors.Count == 0)
                {
                    errorList.Add(new DiscordantSentenceError(null, -1, baseDiscordantSentenceError.Weight, baseDiscordantSentenceError.Grading, baseDiscordantSentenceError.GradingType));
                }
                errorList.Remove(baseDiscordantSentenceError);

                // at the moment of writing, not sure if the list is copied or passed by reference, assign back just to be sure
                result.Result.Criteria = criteriaList;
                result.Result.Errors = errorList;
                result.MLSResult = mlsResult;

                Repository.UpdateResult(resultId, result);
            }
        }

        private NumericalCriterion CreateKeywordsQualityCriterion(MLSAnalysisResult mlsResult, NumericalCriterion baseCriterion)
        {
            double value = mlsResult.UserPhrases.Average(e => e.Score);
            var criterion = new NumericalCriterion(baseCriterion.Name,
                value,
                baseCriterion.Interval.LowerBound,
                baseCriterion.Interval.UpperBound,
                baseCriterion.Factor,
                "description",
                "adviceToLower",
                "adviceToHigher");
            return criterion;
        }

        private List<Error> CreateDiscordantSentenceErrorList(PaperAnalysisResult result, MLSAnalysisResult mlsResult, Error baseError)
        {
            List<Error> errors = new List<Error>();
            // kind of long, but ok
            for (int paraIndex = 0; paraIndex < mlsResult.Coherence.Count; paraIndex++)
            {
                if (!mlsResult.Coherence[paraIndex].IsSkipped)
                {
                    for (int sentIndex = 0; sentIndex < mlsResult.Coherence[paraIndex].IncoherentSentences.Count; sentIndex++)
                    {
                        if (mlsResult.Coherence[paraIndex].IncoherentSentences[sentIndex])
                        {
                            Sentence sentence = result.Sections[paraIndex].Sentences[sentIndex];
                            DiscordantSentenceError error = new DiscordantSentenceError(sentence, baseError.ErrorCost, baseError.Weight, baseError.Grading, baseError.GradingType);
                            errors.Add(error);
                        }
                    }
                }
            }
            return errors;
        }

        private List<Error> CreateMissingSentenceErrorList(PaperAnalysisResult result, MLSAnalysisResult mlsResult, Error baseError)
        {
            List<Error> errors = new List<Error>();
            for (int paraIndex = 0; paraIndex < mlsResult.Coherence.Count; paraIndex++)
            {
                if (!mlsResult.Coherence[paraIndex].IsSkipped)
                {
                    for (int sentIndex = 0; sentIndex < mlsResult.Coherence[paraIndex].MissingSentences.Count; sentIndex++)
                    {
                        if (mlsResult.Coherence[paraIndex].MissingSentences[sentIndex])
                        {
                            Sentence sentence = result.Sections[paraIndex].Sentences[sentIndex];
                            MissingSentenceError error = new MissingSentenceError(sentence, baseError.ErrorCost, baseError.Weight, baseError.Grading, baseError.GradingType);
                            errors.Add(error);
                        }
                    }
                }
            }

            return errors;
        }
    }
}
