namespace CisApi.Data;

using Entities;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"];
        var databaseName = configuration["MongoDB:DatabaseName"];
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Topic> Topics => _database.GetCollection<Topic>("topics");
    public IMongoCollection<Idea> Ideas => _database.GetCollection<Idea>("ideas");
    public IMongoCollection<Vote> Votes => _database.GetCollection<Vote>("votes");
}