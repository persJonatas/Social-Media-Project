using CisApi.Data.Entities;

namespace CisApi.Data.Interfaces;

public interface IVoteRepository
{
    Task<Vote> CastAsync(string ideaId, string userId);
    Task<bool> ExistsAsync(string ideaId, string userId);
    Task<Vote?> GetAsync(string ideaId, string userId);
    Task DeleteAsync(Vote vote);
}