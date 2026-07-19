using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;

namespace Msteambot.Graph;

public class TeamsMeetingService
{
    private readonly GraphServiceClient? _graphClient;

    public TeamsMeetingService(IConfiguration configuration)
    {
        var tenantId = configuration["AAD_APP_TENANT_ID"] ?? "";
        var clientId = configuration["AAD_APP_CLIENT_ID"] ?? "";
        var clientSecret = configuration["AAD_APP_CLIENT_SECRET"] ?? "";

        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            _graphClient = null;
            return;
        }

        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _graphClient = new GraphServiceClient(clientSecretCredential, new[] { "https://graph.microsoft.com/.default" });
    }

    public async Task<string> JoinMeetingAsync(string meetingId, string userId)
    {
        if (_graphClient is null)
        {
            return "Graph client is not configured. Add AAD_APP_CLIENT_ID, AAD_APP_TENANT_ID, and AAD_APP_CLIENT_SECRET to enable a real meeting join.";
        }

        var onlineMeeting = new OnlineMeeting
        {
            Subject = "Transcript bot join request",
            StartDateTime = DateTimeOffset.UtcNow,
            EndDateTime = DateTimeOffset.UtcNow.AddHours(1),
            JoinWebUrl = $"https://teams.microsoft.com/l/meetup-join/{meetingId}",
            Participants = new MeetingParticipants
            {
                Attendees = new List<MeetingParticipantInfo>
                {
                    new()
                    {
                        Identity = new IdentitySet
                        {
                            User = new Identity { Id = userId }
                        }
                    }
                }
            }
        };

        await _graphClient.Communications.OnlineMeetings.PostAsync(onlineMeeting);
        return "Meeting join flow scaffold initialized";
    }
}
