using MongoDB.Driver;

namespace NotesApp.Api.Services
{
    public class MongoService
    {
        private readonly IMongoDatabase _database;

        public MongoService()
        {
            var connectionString = "mongodb+srv://PiyushSingh:KTXbrTvlw4dDXwsp@cluster0.m50rmcy.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
            var mongoClient = new MongoClient(connectionString);
            _database = mongoClient.GetDatabase("notesApp");
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}


