namespace CisApi.Presentation.Controllers;

using CisApi.Business.DTOs;
using CisApi.Business.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("cis-api/v1/topics")]
public class TopicsController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicsController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTopics()
    {
        var topics = await _topicService.GetAllAsync();
        return Ok(topics);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTopic(string id)
    {
        var topic = await _topicService.GetByIdAsync(id);
        if (topic is null)
            return NotFound();
        return Ok(topic);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTopic([FromBody] CreateTopicRequest request)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;
        var result = await _topicService.CreateTopicAsync(userId, request);
        return StatusCode(201, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTopic(string id, [FromBody] UpdateTopicRequest request)
    {
        try
        {
            var userId = HttpContext.Items["UserId"]?.ToString()!;
            var result = await _topicService.UpdateAsync(id, userId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTopic(string id)
    {
        try
        {
            var userId = HttpContext.Items["UserId"]?.ToString()!;
            await _topicService.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403);
        }
    }
}
