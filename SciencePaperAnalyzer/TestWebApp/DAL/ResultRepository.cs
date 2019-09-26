using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using AnalyzeResults.Presentation;
using AnalyzeResults.Settings;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using WebPaperAnalyzer.Models;

namespace WebPaperAnalyzer.DAL
{
    public class ResultRepository : IResultRepository
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<BinaryForm> _resultsCollection;

        public ResultRepository(MongoSettings settings)
        {
            var internalIdentity = new MongoInternalIdentity("admin", settings.User);
            var passwordEvidence = new PasswordEvidence(settings.Password);
            var mongoCredential = new MongoCredential("SCRAM-SHA-1", internalIdentity, passwordEvidence);
            var credentials = new List<MongoCredential> { mongoCredential };


            var mongoSettings = new MongoClientSettings();
            mongoSettings.Credentials = credentials;
            var address = new MongoServerAddress(settings.Host);
            mongoSettings.Server = address;

            _client = new MongoClient(mongoSettings); //new MongoClient(settings.ConnectionString);
            _database = _client.GetDatabase(settings.Database);
            _resultsCollection = _database.GetCollection<BinaryForm>("results");
        }

        public void AddResult(AnalysisResult result)
        {
            try
            {
                var data = new byte[] { };
                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, result.Result);
                    data = ms.ToArray();
                }
                var test = new BinaryForm();
                test.Id = result.Id;
                test.Data = data;
            
                _resultsCollection.InsertOne(test);
            }
            catch (Exception ex)
            {
            }
        }

        public AnalysisResult GetResult(string id)
        {
            var filter = Builders<BinaryForm>.Filter.Eq("_id", id);
            var result = _resultsCollection.Find(filter).ToList();
            if (result.Count == 0)
                return null;
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(result[0].Data, 0, result[0].Data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return new AnalysisResult {Id = id, Result = obj as PaperAnalysisResult };
            }
        }

        public class BinaryForm
        {
            [BsonId]
            public string Id { get; set; }

            [BsonElement("data")]
            public byte[] Data { get; set; }
        }
    }
}
