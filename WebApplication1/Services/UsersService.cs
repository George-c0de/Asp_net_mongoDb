using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<Users> _usersCollection;

        public UsersService(
            IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(
                DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                DatabaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<Users>(
                DatabaseSettings.Value.UsersCollectionName);
        }

        public async Task<string> GetToken(string id)
        {
            char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            Random rand = new Random();
            string lstWords;
                string word = "";
                for (int j = 1; j <= 24; j++)
                {
                    int letter_num = rand.Next(0, letters.Length - 1);
                    word += letters[letter_num];
                }
                lstWords = word;
                var a = await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
                Users user = a;
                user.Token = lstWords;
                await UpdateAsync(a.Id,user);
                return (lstWords);
        } 
        public async Task<List<Users>> GetAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task<Users?> GetAsync(string id) =>
            await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Users newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(string id, Users updatedUser) =>
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

        public async Task RemoveAsync(string id) =>
            await _usersCollection.DeleteOneAsync(x => x.Id == id);
    }
}
