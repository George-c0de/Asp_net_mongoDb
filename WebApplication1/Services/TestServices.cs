using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class TestServices
    {
        private readonly IMongoCollection<Test> _testCollection;

        public TestServices(
            IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(
                DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                DatabaseSettings.Value.DatabaseName);

            _testCollection = mongoDatabase.GetCollection<Test>("Test");
        }

        public async Task<List<Test>> GetAsync() =>
            await _testCollection.Find(_ => true).ToListAsync();

#pragma warning disable CS8632 // Аннотацию для ссылочных типов, допускающих значения NULL, следует использовать в коде только в контексте аннотаций "#nullable".
		public async Task<Test?> GetAsync(string id) =>
            await _testCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
#pragma warning restore CS8632 // Аннотацию для ссылочных типов, допускающих значения NULL, следует использовать в коде только в контексте аннотаций "#nullable".

		public async Task CreateAsync(Test newUser) =>
            await _testCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(string id, Test updatedTest) =>
            await _testCollection.ReplaceOneAsync(x => x.Id == id, updatedTest);

        public async Task RemoveAsync(string id) =>
            await _testCollection.DeleteOneAsync(x => x.Id == id);
    }
}
