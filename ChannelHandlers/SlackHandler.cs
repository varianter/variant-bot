using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace VariantBot.ChannelHandlers
{
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal class SlackApiResponse
    {
        public bool Ok { get; set; }
        public Channel Channel { get; set; }
        public string Error { get; set; }
    }

    internal class Channel
    {
        public string Name { get; set; }
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore ClassNeverInstantiated.Global

    public class SlackHandler
    {
        private readonly ILogger<SlackHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SlackHandler(ILogger<SlackHandler> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task HandleMessage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogDebug("New message from Slack");

            var userName = turnContext.Activity.From.Name;
            var userId = turnContext.Activity.From.Id;

            string channelId = turnContext.Activity.ChannelData["SlackMessage"]["event"]["channel"];
            var channelName = await GetSlackChannelName(channelId);

            var message = $"Hello, {userName} / {userId}. We are in '{channelName} / {channelId}'";
            await turnContext.SendActivityAsync(MessageFactory.Text(message, message), cancellationToken);
        }

        private async Task<string> GetSlackChannelName(string channelId)
        {
            var contentDictionary = new Dictionary<string, string>
            {
                {"token", "<api key>"},
                {"channel", channelId}
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                "https://slack.com/api/conversations.info")
            {
                Content = new FormUrlEncodedContent(contentDictionary)
            };

            using var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.SendAsync(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();

            var slackApiResponse = JsonSerializer.Deserialize<SlackApiResponse>(responseString, _jsonSerializerOptions);

            if (!slackApiResponse.Ok)
                return ChannelNameNotFound();

            return string.IsNullOrWhiteSpace(slackApiResponse.Channel?.Name)
                ? ChannelNameNotFound()
                : slackApiResponse.Channel?.Name;

            string ChannelNameNotFound()
            {
                _logger.LogError(
                    $"Could not find channel name for ID '{channelId}'. " +
                    $"Response from Slack API was HTTP {response.StatusCode}, " +
                    $"message '{slackApiResponse.Error}'");

                return string.Empty;
            }
        }
    }
}