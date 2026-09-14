namespace CisApi.Business.DTOs;

using System.ComponentModel.DataAnnotations;

public class UpdateTopicRequest
{
    [StringLength(150, MinimumLength = 5)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }
}
