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
        Task DeleteCriterion(string id);
        Task EditCriterion(ResultCriterion c);
    }
    public class ApplicationContext : IApplicationContext
    {
        public IMongoCollection<User> Users;

        public IMongoCollection<ResultCriterion> Criteria;

        public IMongoCollection<ForbiddenWords> Words;

        public ApplicationContext(MongoSettings settings)
        {
            if (settings!=null)
            {
                string connectionString = $@"mongodb://{settings.User}:{settings.Password}@{settings.Host}:{settings.Port}";

                MongoClient client = new MongoClient(connectionString);
                IMongoDatabase database = client.GetDatabase(settings.Database);
                Users = database.GetCollection<User>("users");
                Criteria = database.GetCollection<ResultCriterion>("criteria");
                Words = database.GetCollection<ForbiddenWords>("words");
            }
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

        public ResultCriterion GetCriteriaByName(string name)
        {
            var filter = Builders<ResultCriterion>.Filter.Eq("Name", name);
            return Criteria.Find(filter).FirstOrDefault();
        }

        public ResultCriterion GetCriteriaById(string id)
        {
            var filter = Builders<ResultCriterion>.Filter.Eq("_id", id);
            return Criteria.Find(filter).FirstOrDefault();
        }

        public async Task AddUser(User u) => await Users.InsertOneAsync(u);

        public async Task AddCriterion(ResultCriterion c) => await Criteria.InsertOneAsync(c);

        public async Task DeleteCriterion(string id) => await Criteria.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));

        public async Task EditCriterion(ResultCriterion c) => await Criteria.ReplaceOneAsync(new BsonDocument("_id", new ObjectId(c.Id)), c);

        public async Task<IEnumerable<ForbiddenWords>> GetForbiddenWordDictionary()
        {
            var builder = new FilterDefinitionBuilder<ForbiddenWords>();
            var filter = builder.Empty;

            return await Words.Find(filter).ToListAsync();
        }

        public async Task<ForbiddenWords> GetDictionary(string name)
        {
            var filter = Builders<ForbiddenWords>.Filter.Eq("_id", name);

            return await Words.Find(filter).FirstOrDefaultAsync();
        }

        public async Task AddDictionary(ForbiddenWords fw) => await Words.InsertOneAsync(fw);

        public async Task DeleteDictionary(string name) =>
            await Words.DeleteOneAsync(Builders<ForbiddenWords>.Filter.Eq("_id", name));
    }
}
