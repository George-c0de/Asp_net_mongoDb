using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class QuestionsServices
    {
        private readonly IMongoCollection<Question> _QuestionCollection;

        public QuestionsServices(
            IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(
                DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                DatabaseSettings.Value.DatabaseName);

            _QuestionCollection = mongoDatabase.GetCollection<Question>(
                "Question");
        }

        public async Task<List<Question>> GetAsync() =>
            await _QuestionCollection.Find(_ => true).ToListAsync();

#pragma warning disable CS8632 // Аннотацию для ссылочных типов, допускающих значения NULL, следует использовать в коде только в контексте аннотаций "#nullable".
		public async Task<Question?> GetAsync(string id) =>
            await _QuestionCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
#pragma warning restore CS8632 // Аннотацию для ссылочных типов, допускающих значения NULL, следует использовать в коде только в контексте аннотаций "#nullable".

		public async Task<List<Question>> GetAsync_id_cat(string id) =>
            await _QuestionCollection.Find(x => x.id_category == id).ToListAsync();
        public async Task CreateAsync(Question newUser) =>
            await _QuestionCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(string id, Question updatedQuestion) =>
            await _QuestionCollection.ReplaceOneAsync(x => x.Id == id, updatedQuestion);

        public async Task RemoveAsync(string id) =>
            await _QuestionCollection.DeleteOneAsync(x => x.Id == id);
    }
}
