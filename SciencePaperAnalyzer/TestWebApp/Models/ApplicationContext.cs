using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnalyzeResults.Settings;

namespace WebPaperAnalyzer.Models
{
    public interface IApplicationContext
    {
        Task<IEnumerable<User>> GetUsers();
        Task<IEnumerable<ResultCriterion>> GetCriteria();
        Task AddUser(User u);
        Task AddCriterion(ResultCriterion c);
    }
    public class ApplicationContext : IApplicationContext
    {
        public IMongoCollection<User> Users;

        public IMongoCollection<ResultCriterion> Criteria;

        public ApplicationContext(MongoSettings settings)
        {
            string connectionString = @"mongodb://root:example@mongo:27017";
//            var internalIdentity = new MongoInternalIdentity("admin", settings.User);
//            var passwordEvidence = new PasswordEvidence(settings.Password);
//            var mongoCredential = new MongoCredential("SCRAM-SHA-1", internalIdentity, passwordEvidence);
//            var credentials = new List<MongoCredential> { mongoCredential };
//            var mongoSettings = new MongoClientSettings {Credentials = credentials};
//            var address = new MongoServerAddress(settings.Host);
//            mongoSettings.Server = address;
            MongoClient client = new MongoClient(connectionString);
            IMongoDatabase database = client.GetDatabase("resultsDB");
            Users = database.GetCollection<User>("users");
            Criteria = database.GetCollection<ResultCriterion>("criteria");
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var builder = new FilterDefinitionBuilder<User>();
            var filter = builder.Empty;

            return await Users.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<ResultCriterion>> GetCriteria()
        {
            var builder = new FilterDefinitionBuilder<ResultCriterion>();
            var filter = builder.Empty;

            return await Criteria.Find(filter).ToListAsync();
        }

        public async Task AddUser(User u) => await Users.InsertOneAsync(u);

        public async Task AddCriterion(ResultCriterion c) => await Criteria.InsertOneAsync(c);
    }
}
