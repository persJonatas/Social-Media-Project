using CisApi.Business.Interfaces;
using CisApi.Data.Entities;
using MongoDB.Driver;

namespace CisApi.Data.Repositories;

public class IdeaRepository(IMongoDbContext context) : IIdeaRepository
{
    public async Task<Idea> CreateAsync(Idea idea)
    {
        idea.Id = Guid.NewGuid().ToString();
        idea.CreatedAt = DateTime.UtcNow;
        idea.UpdatedAt = DateTime.UtcNow;
        await context.Ideas.InsertOneAsync(idea);
        return idea;
    }

    public async Task<IEnumerable<Idea>> GetAllAsync()
    {
        return await context.Ideas.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Idea>> GetByTopicIdAsync(string topicId)
    {
        return await context.Ideas.Find(i => i.TopicId == topicId).ToListAsync();
    }

    public async Task<Idea?> GetByIdAsync(string id)
    {
        return await context.Ideas.Find(i => i.Id == id).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await context.Ideas.Find(i => i.Id == id).AnyAsync();
    }
    
    public async Task<Idea> UpdateAsync(Idea idea)
    {
        idea.UpdatedAt = DateTime.UtcNow;
        await context.Ideas.ReplaceOneAsync(i => i.Id == idea.Id, idea);
        return idea;
    }

    public async Task DeleteAsync(Idea idea)
    {
        await context.Ideas.DeleteOneAsync(i => i.Id == idea.Id);
    }
    
    public async Task UpdateVoteCountAsync(string ideaId, int increment)
    {
        var filter = Builders<Idea>.Filter.Eq(i => i.Id, ideaId);
        var update = Builders<Idea>.Update.Inc(i => i.VoteCount, increment);
        await context.Ideas.UpdateOneAsync(filter, update);
    }
}