using Microsoft.AspNetCore.Mvc;
using Msteambot.Graph;

namespace Msteambot.Controllers;

[ApiController]
[Route("api")]
public class TeamsController : ControllerBase
{
    // Stores the meeting service used to trigger Teams meeting actions.
    private readonly TeamsMeetingService _meetingService;

    // Injects the meeting service through dependency injection.
    public TeamsController(TeamsMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    // Accepts a meeting ID and user ID and forwards the request to the Graph meeting service.
    [HttpPost("join-meeting")]
    public async Task<IActionResult> JoinMeeting([FromQuery] string meetingId, [FromQuery] string userId)
    {
        // Call the Graph-based workflow that prepares or starts the join flow.
        var result = await _meetingService.JoinMeetingAsync(meetingId, userId);

        // Return a simple JSON response with the service outcome.
        return Ok(new { result });
    }
}
