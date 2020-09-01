using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VariantBot.Slack;

namespace VariantBot.Services
{

    public enum Platform
    {
        Spotify,
        YouTube,
        AppleMusic,
        Tidal
    }
    public static class StringExtensions
    {
        public static (string id, string type) FindMusicInformation(this string text)
        {


            return (id: "", type: "");

        }
    }
    public class SlackMusicRecommendations : IHostedService
    {
        private readonly ILogger<SlackMusicRecommendations> _logger;

        private static readonly HttpClient HttpClient = new HttpClient();

        public SlackMusicRecommendations(ILogger<SlackMusicRecommendations> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var musicChannelId = "C012Z37M1P1";

            var allMessages = await SlackMessage.GetAllMessages(musicChannelId);
            var parsedResult = JObject.Parse(allMessages);

            var musicRecommendations = new List<MusicRecommendation>();

            parsedResult["messages"].ForEach(message =>
            {
                try
                {
                    var text = message["text"].Value<string>();

                    var musicInformation = text.FindMusicInformation();

                    if (!string.IsNullOrWhiteSpace(musicInformation.id))
                    {
                        var userId = message["user"].Value<string>();

                        var musicRecommendation = new MusicRecommendation
                        {
                            User = userId,
                            Timestamp = message["ts"].Value<string>(),
                            Id = musicInformation.id,
                            Type = musicInformation.type,
                            Platform = Platform.Spotify
                        };

                        musicRecommendations.Add(musicRecommendation);
                    }
                }
                catch (System.Exception) { }
            });

            musicRecommendations.ForEach(async musicRecommendation =>
            {
                var functionUrl = "";

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, functionUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(musicRecommendation), Encoding.UTF8, "application/json")
                };

                using var result = await HttpClient.SendAsync(httpRequest);

                var resultString = await result.Content.ReadAsStringAsync();

                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogDebug(resultString);
                }


            });


        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private class MusicRecommendation
        {
            public string User { get; internal set; }
            public string Timestamp { get; internal set; }
            public string Id { get; internal set; }
            public string Type { get; internal set; }
            public Platform Platform { get; internal set; }
        }


    }
}