namespace CisApi.Business.DTOs;

public class VoteResponseDTO
{
    public string Id { get; set; } = string.Empty;
    public string IdeaId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
