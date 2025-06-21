using MongoDB.Driver;
using NotesApp.Api.Models;
using System.Threading.Tasks;

namespace NotesApp.Api.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(MongoService mongoService)
        {
            _users = mongoService.GetCollection<User>("users");
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }
    }
}
