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
        private ILogger<SlackMessageHistoryService> _logger;
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
            _channels.Value.ForEach(async slackChannel =>
            {
                var allMessages = await SlackMessage.GetAllMessages(slackChannel.ChannelId);
                var parsedResult = JObject.Parse(allMessages);
                
                // pass result to correct service
            });
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}