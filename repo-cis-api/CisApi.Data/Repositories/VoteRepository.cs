using CisApi.Business.Interfaces;
using CisApi.Data.Entities;
using CisApi.Data.Interfaces;
using MongoDB.Driver;

namespace CisApi.Data.Repositories;

public class VoteRepository(IMongoDbContext context) : IVoteRepository
{
    public async Task<Vote> CastAsync(string ideaId, string userId)
    {
        var vote = new Vote
        {
            Id = Guid.NewGuid().ToString(),
            IdeaId = ideaId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await context.Votes.InsertOneAsync(vote);
        return vote;
    }

    public async Task<bool> ExistsAsync(string ideaId, string userId)
    {
        return await context.Votes.Find(v => v.IdeaId == ideaId && v.UserId == userId).AnyAsync();
    }

    public async Task<Vote?> GetAsync(string ideaId, string userId)
    {
        return await context.Votes.Find(v => v.IdeaId == ideaId && v.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task DeleteAsync(Vote vote)
    {
        await context.Votes.DeleteOneAsync(v => v.Id == vote.Id);
    }
}