namespace CisApi.Presentation.Controllers;

using CisApi.Business.DTOs;
using CisApi.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("ideas")]
public class IdeasController : ControllerBase
{
    private readonly IIdeaService _ideaService;

    public IdeasController(IIdeaService ideaService)
    {
        _ideaService = ideaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _ideaService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _ideaService.GetByIdAsync(id);
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateIdeaRequestDTO request)
    {
        try
        {
            var userId = HttpContext.Items["UserId"]?.ToString()!;
            var result = await _ideaService.UpdateAsync(userId, id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var userId = HttpContext.Items["UserId"]?.ToString()!;
            await _ideaService.DeleteAsync(userId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }
}