using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;

namespace Msteambot.Graph;

public class TeamsMeetingService
{
    // Holds the authenticated Microsoft Graph client, if configuration is present.
    private readonly GraphServiceClient? _graphClient;

    // Construct the service using bot/App Registration settings from configuration.
    public TeamsMeetingService(IConfiguration configuration)
    {
        var tenantId = configuration["AAD_APP_TENANT_ID"] ?? "";
        var clientId = configuration["AAD_APP_CLIENT_ID"] ?? "";
        var clientSecret = configuration["AAD_APP_CLIENT_SECRET"] ?? "";

        // If the required Azure AD values are missing, the service remains disabled.
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            _graphClient = null;
            return;
        }

        // Create a Graph client with the app's client secret credentials.
        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _graphClient = new GraphServiceClient(clientSecretCredential, new[] { "https://graph.microsoft.com/.default" });
    }

    // Tries to initiate the Teams meeting join flow through Microsoft Graph.
    public async Task<string> JoinMeetingAsync(string meetingId, string userId)
    {
        // If the Graph client is not configured, return a helpful placeholder message.
        if (_graphClient is null)
        {
            return "Graph client is not configured. Add AAD_APP_CLIENT_ID, AAD_APP_TENANT_ID, and AAD_APP_CLIENT_SECRET to enable a real meeting join.";
        }

        // Build a minimal online meeting payload for the join flow scaffold.
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

        // Submit the meeting model to Microsoft Graph.
        await _graphClient.Communications.OnlineMeetings.PostAsync(onlineMeeting);
        return "Meeting join flow scaffold initialized";
    }
}
