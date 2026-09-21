using CisApi.Business.DTOs;
using CisApi.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace CisApi.Presentation.Controllers;

[ApiController]
[Route("cis-api/v1/ideas")]
public class VotesController(VoteService voteService) : ControllerBase
{
    [HttpPost("{ideaId}/votes")]
    public async Task<IActionResult> CastVote(string ideaId, [FromBody] VoteRequestDTO request)
    {
        try
        {
            if (ideaId != request.IdeaId)
            {
                return BadRequest(new { error = "Idea ID in route does not match Idea ID in request body." });
            }

            var userId = HttpContext.Items["UserId"]?.ToString()!;
            var result = await voteService.CastAsync(request, userId);
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    [HttpDelete("{ideaId}/votes")]
    public async Task<IActionResult> CancelVote(string ideaId)
    {
        try
        {
            var userId = HttpContext.Items["UserId"]?.ToString()!;
            await voteService.CancelAsync(ideaId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }
}
