using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ResultServices
    {
        private readonly IMongoCollection<Result> _ResultCollection;

        public ResultServices(
            IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(
                DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                DatabaseSettings.Value.DatabaseName);

            _ResultCollection = mongoDatabase.GetCollection<Result>(
                "Result");
        }

        public async Task<List<Result>> GetAsync() =>
            await _ResultCollection.Find(_ => true).ToListAsync();

        public async Task<Result?> GetAsync(string id) =>
            await _ResultCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Result newUser) =>
            await _ResultCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(string id, Result updatedResult) =>
            await _ResultCollection.ReplaceOneAsync(x => x.Id == id, updatedResult);

        public async Task RemoveAsync(string id) =>
            await _ResultCollection.DeleteOneAsync(x => x.Id == id);
    }
}
