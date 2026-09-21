namespace CisApi.Business.DTOs;

public class CreateIdeaRequestDTO
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TopicId { get; set; } = string.Empty;
}