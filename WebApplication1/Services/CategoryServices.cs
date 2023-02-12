using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class CategoryServices
    {
        private readonly IMongoCollection<Category> _CategoryCollection;

        public CategoryServices(
            IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(
                DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                DatabaseSettings.Value.DatabaseName);

            _CategoryCollection = mongoDatabase.GetCollection<Category>(
                "Category");
        }

        public async Task<List<Category>> GetAsync() =>
            await _CategoryCollection.Find(_ => true).ToListAsync();

#pragma warning disable CS8632 // Аннотацию для ссылочных типов, допускающих значения NULL, следует использовать в коде только в контексте аннотаций "#nullable".
		public async Task<Category?> GetAsync(string id) =>
            await _CategoryCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
#pragma warning restore CS8632 // Аннотацию для ссылочных типов, допускающих значения NULL, следует использовать в коде только в контексте аннотаций "#nullable".

		public async Task CreateAsync(Category newUser) =>
            await _CategoryCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(string id, Category updatedCategory) =>
            await _CategoryCollection.ReplaceOneAsync(x => x.Id == id, updatedCategory);

        public async Task RemoveAsync(string id) =>
            await _CategoryCollection.DeleteOneAsync(x => x.Id == id);
    }
}
