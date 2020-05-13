using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VariantBot.Slack
{
    public class SlackMessageHandler
    {
        private readonly ILogger<SlackMessageHandler> _logger;

        public SlackMessageHandler(ILogger<SlackMessageHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleMessage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogDebug("New message from Slack");

            var slackMessage = turnContext.Activity.ChannelData["SlackMessage"] as JObject;

            try
            {
                var urls = (slackMessage["event"]["blocks"]
                    .Where(block => block["type"].Value<string>().Equals("rich_text"))
                    .SelectMany(block => block["elements"], (block, blockElement) => new {block, blockElement})
                    .Where(@t => @t.blockElement["type"].Value<string>().Equals("rich_text_section"))
                    .SelectMany(@t => @t.blockElement["elements"], (@t, element) => new {@t, element})
                    .Where(@t => @t.element["type"].Value<string>().Equals("link"))
                    .Select(@t => @t.element["url"].Value<string>())).ToList();

                if (!urls.Any())
                    return;

                var userId = turnContext.Activity.From.Id.Split(":").FirstOrDefault();
                var channelId = slackMessage["event"]["channel"].Value<string>();

                var jsonContent = JsonConvert.SerializeObject(SlackMessage.CreateNewsletterUrlMessage(channelId, userId));
                await SlackMessage.Post(jsonContent,
                    "https://slack.com/api/chat.postEphemeral");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse SlackMessage");
            }
        }
    }
}