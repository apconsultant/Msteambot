using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Msteambot.Bots;

public class MeetingBot : ActivityHandler
{
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var replyText = "I can help join a Teams meeting and manage transcript capture.";
        await turnContext.SendActivityAsync(MessageFactory.Text(replyText), cancellationToken);
    }

    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        foreach (var member in membersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                var welcomeText = "Hello! I can join a Teams meeting and help capture transcript data.";
                await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText), cancellationToken);
            }
        }
    }
}
