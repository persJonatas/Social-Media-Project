using CisApi.Business.DTOs;
using CisApi.Business.Interfaces;
using CisApi.Data.Interfaces;

namespace CisApi.Business.Services;

public class VoteService(
    IVoteRepository voteRepository,
    IIdeaRepository ideaRepository)
{
    public async Task<VoteResponseDTO> CastAsync(VoteRequestDTO request, string userId)
    {
        if (!await ideaRepository.ExistsAsync(request.IdeaId)) 
            throw new KeyNotFoundException("Idea not found.");

        if (await voteRepository.ExistsAsync(request.IdeaId, userId))
            throw new InvalidOperationException("User has already voted on this idea.");

        var vote = await voteRepository.CastAsync(request.IdeaId, userId);

        await ideaRepository.UpdateVoteCountAsync(request.IdeaId, 1);
        
        return new VoteResponseDTO
        {
            Id = vote.Id,
            IdeaId = vote.IdeaId,
            UserId = vote.UserId,
            CreatedAt = vote.CreatedAt
        };
    }

    public async Task CancelAsync(string ideaId, string userId)
    {
        if (!await ideaRepository.ExistsAsync(ideaId)) 
            throw new KeyNotFoundException("Idea not found.");

        var vote = await voteRepository.GetAsync(ideaId, userId);
        if (vote is null)
            throw new KeyNotFoundException("Vote not found.");

        await voteRepository.DeleteAsync(vote);
        await ideaRepository.UpdateVoteCountAsync(ideaId, -1);
    }
}
