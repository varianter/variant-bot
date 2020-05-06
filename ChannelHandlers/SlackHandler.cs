using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace VariantBot.ChannelHandlers
{
    public class SlackHandler
    {
        private readonly ILogger<SlackHandler> _logger;

        public SlackHandler(ILogger<SlackHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleMessage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogDebug("New message from Slack");

            var userName = turnContext.Activity.From.Name;
            var userId = turnContext.Activity.From.Id;

            string channelId = turnContext.Activity.ChannelData["SlackMessage"]["event"]["channel"];

            var message = $"Hello, {userName} / {userId}. We are in '{channelId}'";
            await turnContext.SendActivityAsync(MessageFactory.Text(message, message), cancellationToken);
        }
    }
}