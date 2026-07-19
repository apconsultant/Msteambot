using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Msteambot;

public class AdapterWithErrorHandler : CloudAdapter
{
    public AdapterWithErrorHandler(IConfiguration configuration, ILogger<CloudAdapter> logger)
        : base(new ConfigurationBotFrameworkAuthentication(configuration), logger)
    {
    }
}
