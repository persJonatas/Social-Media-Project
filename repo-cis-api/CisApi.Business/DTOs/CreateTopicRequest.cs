namespace CisApi.Business.DTOs;

using System.ComponentModel.DataAnnotations;

public class CreateTopicRequest
{
    [Required]
    [StringLength(150, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }
}
