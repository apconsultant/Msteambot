using Microsoft.AspNetCore.Mvc;
using Msteambot.Graph;

namespace Msteambot.Controllers;

[ApiController]
[Route("api")]
public class TeamsController : ControllerBase
{
    private readonly TeamsMeetingService _meetingService;

    public TeamsController(TeamsMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    [HttpPost("join-meeting")]
    public async Task<IActionResult> JoinMeeting([FromQuery] string meetingId, [FromQuery] string userId)
    {
        var result = await _meetingService.JoinMeetingAsync(meetingId, userId);
        return Ok(new { result });
    }
}
