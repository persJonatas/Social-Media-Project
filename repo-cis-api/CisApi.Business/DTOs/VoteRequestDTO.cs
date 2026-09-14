using System.ComponentModel.DataAnnotations;

namespace CisApi.Business.DTOs;

public class VoteRequestDTO
{
    [Required(ErrorMessage = "Idea ID is required.")]
    public string IdeaId { get; set; } = string.Empty;
}
