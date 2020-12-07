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
using TestWebApp.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using static WebPaperAnalyzer.DAL.ResultRepository;

namespace WebPaperAnalyzer.Models
{
    public class DbInitializer
    {
        IConfiguration _appConfig;
        private IMongoDatabase database;

        public DbInitializer(IConfiguration appConfig = null, IOptions<MongoSettings> mongoSettings = null)
        {
            var _mongoSettings = mongoSettings?.Value;
            if (_mongoSettings != null)
            {
                MongoClient client = new MongoClient(_mongoSettings.ConnectionString);
                database = client.GetDatabase(_mongoSettings.Database);
            }
            _appConfig = appConfig;
        }
        public async Task InitCollections()
        {
            var collectionNames = database.ListCollectionNames().ToList();
            if (!collectionNames.Contains("users"))
            {
                await database.CreateCollectionAsync("users");
            }
            if (!collectionNames.Contains("criteria"))
            {
                await database.CreateCollectionAsync("criteria");
            }
            if (!collectionNames.Contains("words"))
            {
                await database.CreateCollectionAsync("words");
            }

            var resultCollectionInfo = _appConfig.GetSection("ResultCollection");

            if (!collectionNames.Contains("results"))
            {
                var options = new CreateCollectionOptions
                {
                    Capped = true,
                    MaxSize = resultCollectionInfo.GetValue<long>("MaxSize", 10485760),
                    MaxDocuments = resultCollectionInfo.GetValue<long>("MaxDocuments", 5),
                };
                await database.CreateCollectionAsync("results", options);
            }
            else
            {
                // check if it's capped
                var command = new BsonDocument { { "collStats", "results" }, { "scale", 1 } };
                var result = database.RunCommand<BsonDocument>(command);
                if (!result.GetValue("capped").AsBoolean)
                {
                    database.RunCommand<BsonDocument>(new BsonDocument {
                        { "convertToCapped", "results" }, { "size", resultCollectionInfo.GetValue<long>("MaxSize", 10485760) }
                    });
                }
            }


        }
        public async Task InitAdmin()
        {
            var adminInfo = _appConfig.GetSection("AdminAccount");

            var login = "admin";
            var password = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            if (adminInfo != null)
            {
                login = adminInfo.GetValue<string>("Login", "admin");
                password = adminInfo.GetValue<string>("Password", password);
            }
            var usersCollection = database.GetCollection<User>("users");
            var builder = new FilterDefinitionBuilder<User>();
            var filter = builder.Eq("Login", login);
            var hasAdmin = await usersCollection.Find(filter).CountDocumentsAsync();
            if (hasAdmin == 0)
            {
                await usersCollection.InsertOneAsync(new User
                {
                    Login = login,
                    Password = password,
                    Role = "admin"
                });
                Console.WriteLine("Admin account created:");
                Console.WriteLine("Login: " + login);
                Console.WriteLine("Password: " + password);
            }
            else
            {
                Console.WriteLine("Admin account exists:");
                Console.WriteLine("Login: " + login);
                Console.WriteLine("Password: " + password);
            }
        }
    }
}