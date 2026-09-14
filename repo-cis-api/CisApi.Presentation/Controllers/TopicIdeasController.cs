namespace CisApi.Presentation.Controllers;

using CisApi.Business.DTOs;
using CisApi.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("topics/{topicId}/ideas")]
public class TopicIdeasController : ControllerBase
{
    private readonly IIdeaService _ideaService;

    public TopicIdeasController(IIdeaService ideaService)
    {
        _ideaService = ideaService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(string topicId, [FromBody] CreateIdeaRequestDTO request)
    {
        try
        {
            request.TopicId = topicId;
            var userId = HttpContext.Items["UserId"]?.ToString()!;
            var result = await _ideaService.CreateAsync(userId, request);
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetByTopic(string topicId)
    {
        try
        {
            var result = await _ideaService.GetByTopicIdAsync(topicId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }
}