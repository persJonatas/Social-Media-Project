namespace CisApi.Business.Interfaces;

using CisApi.Business.DTOs;

public interface IIdeaService
{
    Task<IdeaResponseDTO> CreateAsync(string userId, CreateIdeaRequestDTO request);
    Task<IEnumerable<IdeaResponseDTO>> GetAllAsync();
    Task<IEnumerable<IdeaResponseDTO>> GetByTopicIdAsync(string topicId);
    Task<IdeaResponseDTO> GetByIdAsync(string id);
    Task<IdeaResponseDTO> UpdateAsync(string userId, string ideaId, UpdateIdeaRequestDTO request);
    Task DeleteAsync(string userId, string ideaId);
}