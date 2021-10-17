using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using VariantBot.Slack;

namespace VariantBot.Services
{

    /// <summary>
    /// Fetches all messages since last time the bot ran
    /// </summary>
    public class SlackMessageHistoryService : IHostedService
    {
        private readonly ILogger<SlackMessageHistoryService> _logger;
        private readonly MusicRecommendationService _musicRecommendationService;
        private readonly IOptions<List<SlackChannel>> _channels;

        public SlackMessageHistoryService(ILogger<SlackMessageHistoryService> logger,
            MusicRecommendationService musicRecommendationService, IOptions<List<SlackChannel>> channels)
        {
            _logger = logger;
            _musicRecommendationService = musicRecommendationService;
            _channels = channels;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _channels.Value.ForEach(async slackChannel =>
                {
                    _logger.LogInformation($"Fetching old messages from channel {slackChannel.ChannelId}/{slackChannel.ChannelName}");

                    var cursor = string.Empty;
                    bool? hasMore;
                    
                    do
                    {
                        var allMessages = await SlackMessage.GetAllMessages(slackChannel.ChannelId, cursor);
                        var parsedResult = JObject.Parse(allMessages);

                        var messages = parsedResult["messages"];
                        if (messages is null)
                        {
                            _logger.LogError("Message history result was null");
                            return;
                        }
                        hasMore = parsedResult["has_more"]?.Value<bool>();
                        if(hasMore == true)
                            cursor = parsedResult["response_metadata"]["next_cursor"]?.Value<string>(); 
                        // _musicRecommendationService.HandleMessages(messages);
                    } while (hasMore == true);
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Fetching old messages failed");
            }

        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}