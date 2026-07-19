namespace Msteambot.Webhooks;

public class NotificationReceiver
{
    public void Handle(string payload)
    {
        Console.WriteLine($"Received notification payload: {payload}");
    }
}
