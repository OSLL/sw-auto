using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using AnalyzeResults.Presentation;
using AnalyzeResults.Settings;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MLSAnalysisWrapper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json.Bson;
using WebPaperAnalyzer.Models;

namespace WebPaperAnalyzer.DAL
{
    public class ResultRepository : IResultRepository
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        //private readonly IMongoCollection<BinaryForm> _resultsCollection;
        private readonly IMongoCollection<AnalysisResult> _resultsCollection;
        private readonly ILogger<ResultRepository> _logger;

        public ResultRepository(IOptions<MongoSettings> settings, ILogger<ResultRepository> logger)
        {
            var internalIdentity = new MongoInternalIdentity("admin", settings.Value.User);
            var passwordEvidence = new PasswordEvidence(settings.Value.Password);
            var mongoCredential = new MongoCredential("SCRAM-SHA-1", internalIdentity, passwordEvidence);
            var credentials = new List<MongoCredential> { mongoCredential };

            var mongoSettings = new MongoClientSettings();
            mongoSettings.Credentials = credentials;
            var address = new MongoServerAddress(settings.Value.Host);
            mongoSettings.Server = address;

            _client = new MongoClient(mongoSettings); //new MongoClient(settings.ConnectionString);
            _database = _client.GetDatabase(settings.Value.Database);
            //_resultsCollection = _database.GetCollection<BinaryForm>("results");
            _resultsCollection = _database.GetCollection<AnalysisResult>("results");
            _logger = logger;
        }

        public void AddResult(AnalysisResult result)
        {
            try
            {
                //var data = new byte[] { };
                //BinaryFormatter bf = new BinaryFormatter();
                //using (var ms = new MemoryStream())
                //{
                //    bf.Serialize(ms, result.Result);
                //    data = ms.ToArray();
                //}

                //var test = new BinaryForm
                //{
                //    Id = result.Id,
                //    Data = data,
                //    StudentLogin = result.StudentLogin,
                //    TeacherLogin = result.TeacherLogin,
                //    Criterion = result.Criterion
                //};

                //_resultsCollection.InsertOne(test);
                _resultsCollection.InsertOne(result);
                Console.WriteLine($"Successfull save result by {result.Id}");
                _logger.LogInformation($"Successfull save result by {result.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void UpdateResult(ObjectId id, AnalysisResult result)
        {
            var filter = Builders<AnalysisResult>.Filter.Eq("_id", id);

            var update = Builders<AnalysisResult>.Update.Set(e => e.MLSResult, result.MLSResult).Set(e => e.Result, result.Result);
                //Set<MLSAnalysisResult>("mlsResult", result.MLSResult).Set<PaperAnalysisResult>("", result.Result);
            _resultsCollection.UpdateOne(filter, update);
        }
        public AnalysisResult GetResult(ObjectId id)
        {
            var result =  _resultsCollection.Find(Builders<AnalysisResult>.Filter.Eq("_id", id)).ToList();
            Console.WriteLine(result.Count);
            if (result.Count == 0)
            {
                return null;
            }
            Console.WriteLine(result[0].ToString());
            return result[0];
            //var filter = Builders<BinaryForm>.Filter.Eq("_id", id);
            //var result = _resultsCollection.Find(filter).ToList();
            //if (result.Count == 0)
            //    return null;
            //using (var memStream = new MemoryStream())
            //{
            //    var binForm = new BinaryFormatter();
            //    memStream.Write(result[0].Data, 0, result[0].Data.Length);
            //    memStream.Seek(0, SeekOrigin.Begin);
            //    var obj = binForm.Deserialize(memStream);
            //    return new AnalysisResult
            //    {
            //        Id = id,
            //        Result = obj as PaperAnalysisResult,
            //        Criterion = result[0].Criterion,
            //        StudentLogin = result[0].StudentLogin,
            //        TeacherLogin = result[0].TeacherLogin
            //    };
            //}
        }

        public IEnumerable<AnalysisResult> GetResultsByLogin(string login, bool type)
        {
            var filter = Builders<AnalysisResult>.Filter.Eq(type ? "TeacherLogin" : "StudentLogin", login);
            var resultList = _resultsCollection.Find(filter).ToList();
            return resultList;

            //var filter = Builders<BinaryForm>.Filter.Eq(type ? "TeacherLogin" : "StudentLogin", login);
            //var binaryFormCollection = _resultsCollection.Find(filter).ToList();
            //var resultList = new List<AnalysisResult>();
            //foreach (var result in binaryFormCollection)
            //{
            //    using (var memStream = new MemoryStream())
            //    {
            //        var binForm = new BinaryFormatter();
            //        memStream.Write(result.Data, 0, result.Data.Length);
            //        memStream.Seek(0, SeekOrigin.Begin);
            //        var obj = binForm.Deserialize(memStream);
            //        resultList.Add(new AnalysisResult
            //        {
            //            Id = result.Id,
            //            StudentLogin = result.StudentLogin,
            //            TeacherLogin = result.TeacherLogin,
            //            Criterion = result.Criterion,
            //            Result = obj as PaperAnalysisResult
            //        });
            //    }
            //}

            //return resultList;
        }

        public class BinaryForm
        {
            [BsonId]
            public string Id { get; set; }

            [BsonElement("data")]
            public byte[] Data { get; set; }

            public string StudentLogin { get; set; }
            public string TeacherLogin { get; set; }
            public string Criterion { get; set; }
        }
    }
}
