using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Msteambot;
using Msteambot.Bots;
using Msteambot.Buffer;
using Msteambot.DocGen;
using Msteambot.Graph;
using Msteambot.Webhooks;

// Create the ASP.NET Core web application host.
var builder = WebApplication.CreateBuilder(args);

// Register API controllers and the HttpClient service used by bot and Graph integrations.
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Register the bot framework adapter and the concrete bot implementation.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
builder.Services.AddSingleton<MeetingBot>();
builder.Services.AddSingleton<TeamsMeetingService>();

// Build the app pipeline.
var app = builder.Build();

// Enable routing and authorization middleware for incoming HTTP requests.
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint for local verification.
app.MapGet("/", () => new
{
    status = "ok",
    message = "Teams transcript bot C# scaffold is running."
});

// Route incoming Teams bot activity requests to the bot framework adapter.
app.MapPost("/api/messages", async (HttpContext context) =>
{
    // Resolve the bot and adapter from DI and process the incoming request.
    var bot = context.RequestServices.GetRequiredService<MeetingBot>();
    var adapter = context.RequestServices.GetRequiredService<IBotFrameworkHttpAdapter>();
    await adapter.ProcessAsync(context.Request, context.Response, bot);
});

// Receives Graph webhook notifications for transcript events and forwards them to the receiver service.
app.MapPost("/api/notifications", (string payload) =>
{
    var receiver = new NotificationReceiver();
    receiver.Handle(payload);
    return Results.Ok(new { received = true });
});

// Start the web host.
app.Run();
