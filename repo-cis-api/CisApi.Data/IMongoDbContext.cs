using CisApi.Data.Entities;
using MongoDB.Driver;

namespace CisApi.Data;

public interface IMongoDbContext
{
    IMongoCollection<Topic> Topics { get; }
    IMongoCollection<Idea> Ideas { get; }
    IMongoCollection<Vote> Votes { get; }
}