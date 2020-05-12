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
        private readonly SlackMessageHandler _slackMessageHandler;

        public VariantBot(ILogger<VariantBot> logger, SlackMessageHandler slackMessageHandler)
        {
            _logger = logger;
            _slackMessageHandler = slackMessageHandler;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.ChannelId)
            {
                case "slack":
                    await _slackMessageHandler.HandleMessage(turnContext, cancellationToken);
                    break;

                default:
                    _logger.LogError($"Message from unknown channel: '{turnContext.Activity.ChannelId}'");
                    break;
            }
        }
    }
}