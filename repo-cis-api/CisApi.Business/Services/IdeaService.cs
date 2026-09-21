using MongoDB.Driver;

namespace CisApi.Business.Services;

using CisApi.Business.DTOs;
using CisApi.Business.Interfaces;
using CisApi.Data;
using CisApi.Data.Entities;

public class IdeaService : IIdeaService
{
    private readonly IIdeaRepository _ideaRepository;
    private readonly IMongoDbContext _context;

    public IdeaService(IIdeaRepository ideaRepository, IMongoDbContext context)
    {
        _ideaRepository = ideaRepository;
        _context = context;
    }

    public async Task<IdeaResponseDTO> CreateAsync(string userId, CreateIdeaRequestDTO request)
    {
        var topic = await _context.Topics.Find(t => t.Id == request.TopicId).FirstOrDefaultAsync();
        if (topic is null)
            throw new KeyNotFoundException($"Topic {request.TopicId} not found.");

        var idea = new Idea
        {
            TopicId = request.TopicId,
            Title = request.Title,
            Description = request.Description,
            CreatedByUserId = userId
        };

        var created = await _ideaRepository.CreateAsync(idea);
        return MapToResponse(created);
    }

    public async Task<IEnumerable<IdeaResponseDTO>> GetAllAsync()
    {
        var ideas = await _ideaRepository.GetAllAsync();
        return ideas.Select(MapToResponse);
    }

    public async Task<IEnumerable<IdeaResponseDTO>> GetByTopicIdAsync(string topicId)
    {
        var topic = await _context.Topics.Find(t => t.Id == topicId).FirstOrDefaultAsync();
        if (topic is null)
            throw new KeyNotFoundException($"Topic {topicId} not found.");

        var ideas = await _ideaRepository.GetByTopicIdAsync(topicId);
        return ideas.Select(MapToResponse);
    }

    public async Task<IdeaResponseDTO> GetByIdAsync(string id)
    {
        var idea = await _ideaRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Idea {id} not found.");

        return MapToResponse(idea);
    }
    
    public async Task<IdeaResponseDTO> UpdateAsync(string userId, string ideaId, UpdateIdeaRequestDTO request)
    {
        var idea = await _ideaRepository.GetByIdAsync(ideaId)
                   ?? throw new KeyNotFoundException($"Idea {ideaId} not found.");

        if (idea.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("You are not the creator of this idea.");

        if (request.Title is not null)
            idea.Title = request.Title;

        if (request.Description is not null)
            idea.Description = request.Description;

        var updated = await _ideaRepository.UpdateAsync(idea);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(string userId, string ideaId)
    {
        var idea = await _ideaRepository.GetByIdAsync(ideaId)
                   ?? throw new KeyNotFoundException($"Idea {ideaId} not found.");

        if (idea.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("You are not the creator of this idea.");

        await _ideaRepository.DeleteAsync(idea);
    }

    private static IdeaResponseDTO MapToResponse(Idea idea) => new()
    {
        Id = idea.Id,
        TopicId = idea.TopicId,
        Title = idea.Title,
        Description = idea.Description,
        CreatedByUserId = idea.CreatedByUserId,
        VoteCount = idea.VoteCount,
        CreatedAt = idea.CreatedAt,
        UpdatedAt = idea.UpdatedAt
    };
}
