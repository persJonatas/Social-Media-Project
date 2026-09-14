using System;
using System.Threading.Tasks;
using CisApi.Data.Entities;

namespace CisApi.Business.Interfaces
{
    public interface IIdeaRepository
    {
        Task<bool> ExistsAsync(string ideaId);
        Task<Idea> CreateAsync(Idea idea);
        Task<IEnumerable<Idea>> GetAllAsync();
        Task<IEnumerable<Idea>> GetByTopicIdAsync(string topicId);
        Task<Idea?> GetByIdAsync(string id);
        Task<Idea> UpdateAsync(Idea idea);
        Task DeleteAsync(Idea idea);
        Task UpdateVoteCountAsync(string ideaId, int increment);
    }
}