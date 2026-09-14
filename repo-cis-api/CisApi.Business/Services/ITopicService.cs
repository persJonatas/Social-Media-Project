namespace CisApi.Business.Services;

using CisApi.Business.DTOs;

public interface ITopicService
{
    Task<IEnumerable<TopicResponse>> GetAllAsync();
    Task<TopicResponse?> GetByIdAsync(string id);
    Task<TopicResponse> CreateTopicAsync(string userId, CreateTopicRequest request);
    Task<TopicResponse> UpdateAsync(string id, string userId, UpdateTopicRequest request);
    Task DeleteAsync(string id, string userId);
}
