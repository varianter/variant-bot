using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private const string MusicChannelId = "C012Z37M1P1";

        public SlackMessageHistoryService(ILogger<SlackMessageHistoryService> logger,
            MusicRecommendationService musicRecommendationService)
        {
            _logger = logger;
            _musicRecommendationService = musicRecommendationService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var allMessages = await SlackMessage.GetAllMessages(MusicChannelId);
            var parsedResult = JObject.Parse(allMessages);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}