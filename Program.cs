using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Msteambot;
using Msteambot.Bots;
using Msteambot.Buffer;
using Msteambot.DocGen;
using Msteambot.Graph;
using Msteambot.Webhooks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
builder.Services.AddSingleton<MeetingBot>();
builder.Services.AddSingleton<TeamsMeetingService>();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => new
{
    status = "ok",
    message = "Teams transcript bot C# scaffold is running."
});

app.MapPost("/api/messages", async (HttpContext context) =>
{
    var bot = context.RequestServices.GetRequiredService<MeetingBot>();
    var adapter = context.RequestServices.GetRequiredService<IBotFrameworkHttpAdapter>();
    await adapter.ProcessAsync(context.Request, context.Response, bot);
});

app.MapPost("/api/notifications", (string payload) =>
{
    var receiver = new NotificationReceiver();
    receiver.Handle(payload);
    return Results.Ok(new { received = true });
});

app.Run();
