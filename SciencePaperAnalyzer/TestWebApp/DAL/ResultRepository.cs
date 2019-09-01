using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using AnalyzeResults.Presentation;
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

        public ResultRepository(string connectionString)
        {
            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase("db");
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
