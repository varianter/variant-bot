using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VariantBot.Controllers;
using VariantBot.Services;

namespace VariantBot.Slack
{
    public class SlackMessageHandler
    {
        private readonly ILogger<SlackMessageHandler> _logger;
        private readonly MusicRecommendationService _musicRecommendationService;
        private readonly List<SlackChannel> _slackChannels;

        public SlackMessageHandler(ILogger<SlackMessageHandler> logger,
            MusicRecommendationService musicRecommendationService,
            IOptions<List<SlackChannel>> slackChannels)
        {
            _musicRecommendationService = musicRecommendationService;
            _slackChannels = slackChannels.Value;
            _logger = logger;
        }

        public async Task HandleMessage(EventsController.SlackEventBody slackEventBody)
        {
            _logger.LogDebug("New message from Slack");

            if(_slackChannels.Any(chan => slackEventBody.Event.Channel.Equals(chan.ChannelId)))
            {
                await _musicRecommendationService.HandleMessage(
                    slackEventBody.Event.User,
                    slackEventBody.Event.Text,
                    slackEventBody.Event.Timestamp
                );

                return;
            }

            try
            {
                var urls = slackEventBody.Event.Blocks
                    .Where(block => block.Type.Equals("rich_text"))
                    .SelectMany(block => block.Elements, (block, blockElement) => new { block, blockElement })
                    .Where(@t => @t.blockElement.Type.Equals("rich_text_section"))
                    .SelectMany(@t => @t.blockElement.Elements, (@t, element) => new { @t, element })
                    .Where(@t => @t.element.Type.Equals("link"))
                    .Select(@t => @t.element.Url).ToList();

                if (!urls.Any())
                    return;

                var userId = slackEventBody.Event.User;
                var channelId = slackEventBody.Event.Channel;

                var jsonContent = JsonConvert.SerializeObject(SlackMessage.CreateNewsletterUrlConfirmationMessage(channelId, userId, urls));
                // await SlackMessage.Post(jsonContent,
                //     "https://slack.com/api/chat.postEphemeral");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse SlackMessage");
            }
        }
    }
}