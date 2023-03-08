using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface ITestServices
    {
        Task<List<Test>> GetAsync();

        Task<Test?> GetAsync(string id);

        Task CreateAsync(Test newtest);

        Task UpdateAsync(string id, Test updatedTest);

        Task RemoveAsync(string id);
    }
    public class TestServices : ITestServices
    {
        private readonly IMongoCollection<Test> _testCollection;

        public TestServices(
            IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(
                DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                DatabaseSettings.Value.DatabaseName);

            _testCollection = mongoDatabase.GetCollection<Test>(
                "Test");
        }

        public async Task<List<Test>> GetAsync() =>
            await _testCollection.Find(_ => true).ToListAsync();

        public async Task<Test?> GetAsync(string id) =>
            await _testCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Test newtest) =>
            await _testCollection.InsertOneAsync(newtest);

        public async Task UpdateAsync(string id, Test updatedTest) =>
            await _testCollection.ReplaceOneAsync(x => x.Id == id, updatedTest);

        public async Task RemoveAsync(string id) =>
            await _testCollection.DeleteOneAsync(x => x.Id == id);
    }
}
