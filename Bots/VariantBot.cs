using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using VariantBot.Slack;

namespace VariantBot.Bots
{
    public class VariantBot : ActivityHandler
    {
        private readonly ILogger<VariantBot> _logger;
        private readonly SlackHandler _slackHandler;

        public VariantBot(ILogger<VariantBot> logger, SlackHandler slackHandler)
        {
            _logger = logger;
            _slackHandler = slackHandler;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.ChannelId)
            {
                case "slack":
                    await _slackHandler.HandleMessage(turnContext, cancellationToken);
                    break;

                default:
                    _logger.LogError($"Message from unknown channel: '{turnContext.Activity.ChannelId}'");
                    break;
            }
        }
    }
}