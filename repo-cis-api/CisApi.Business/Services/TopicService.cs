using MongoDB.Driver;

namespace CisApi.Business.Services;

using CisApi.Business.DTOs;
using CisApi.Data;
using CisApi.Data.Entities;

public class TopicService(IMongoDbContext context) : ITopicService
{
    public async Task<IEnumerable<TopicResponse>> GetAllAsync()
    {
        var topics = await context.Topics.Find(_ => true).ToListAsync();
        return topics.Select(MapToResponse);
    }

    public async Task<TopicResponse?> GetByIdAsync(string id)
    {
        var topic = await context.Topics.Find(t => t.Id == id).FirstOrDefaultAsync();
        return topic is null ? null : MapToResponse(topic);
    }

    public async Task<TopicResponse> CreateTopicAsync(string userId, CreateTopicRequest request)
    {
        var now = DateTime.UtcNow;
        var topic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Description = request.Description,
            CreatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        };
        await context.Topics.InsertOneAsync(topic);
        return MapToResponse(topic);
    }

    public async Task<TopicResponse> UpdateAsync(string id, string userId, UpdateTopicRequest request)
    {
        var topic = await FindTopicOrThrowAsync(id);
        EnsureOwnership(topic, userId);

        if (request.Title is not null)
            topic.Title = request.Title;

        if (request.Description is not null)
            topic.Description = request.Description;

        topic.UpdatedAt = DateTime.UtcNow;
        await context.Topics.ReplaceOneAsync(t => t.Id == id, topic);
        return MapToResponse(topic);
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var topic = await FindTopicOrThrowAsync(id);
        EnsureOwnership(topic, userId);

        await context.Topics.DeleteOneAsync(t => t.Id == id);
    }

    private async Task<Topic> FindTopicOrThrowAsync(string id)
    {
        var topic = await context.Topics.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (topic is null)
            throw new KeyNotFoundException($"Topic {id} not found.");
        return topic;
    }

    private static void EnsureOwnership(Topic topic, string userId)
    {
        if (topic.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("User is not the owner of this topic.");
    }

    private static TopicResponse MapToResponse(Topic topic) => new()
    {
        Id = topic.Id,
        Title = topic.Title,
        Description = topic.Description,
        CreatedByUserId = topic.CreatedByUserId,
        CreatedAt = topic.CreatedAt,
        UpdatedAt = topic.UpdatedAt
    };
}
