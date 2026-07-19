using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Msteambot.Bots;

public class MeetingBot : ActivityHandler
{
    // Handles chat messages sent to the bot in Teams and returns a simple response.
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        // Build the reply text that explains the bot's purpose.
        var replyText = "I can help join a Teams meeting and manage transcript capture.";

        // Send the reply back to the user in the same Teams conversation.
        await turnContext.SendActivityAsync(MessageFactory.Text(replyText), cancellationToken);
    }

    // Handles when new users are added to a conversation with the bot.
    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        // Loop through each newly added member and greet them.
        foreach (var member in membersAdded)
        {
            // Skip the bot itself so it does not send a greeting to itself.
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                // Compose a welcome message for a human user who added the bot.
                var welcomeText = "Hello! I can join a Teams meeting and help capture transcript data.";

                // Send the greeting to the conversation.
                await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText), cancellationToken);
            }
        }
    }
}
