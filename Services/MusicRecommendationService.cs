using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MoreLinq.Extensions;

namespace VariantBot.Services
{
    public class MusicRecommendationService
    {
        private readonly ILogger<MusicRecommendationService> _logger;
        private static readonly HttpClient HttpClient = new HttpClient();

        public MusicRecommendationService(ILogger<MusicRecommendationService> logger)
        {
            _logger = logger;
        }

        public async Task HandleMessage(string user, string text, string timestamp)
        {
            if (timestamp.Contains("."))
            {
                timestamp = timestamp.Split(".")[0];
            }

            var musicRecommendation = new MusicRecommendation
            {
                User = user,
                Timestamp = timestamp,
                Text = text
            };

            await SendMusicRecommendation(musicRecommendation);
        }

        public void HandleMessages(JObject messages)
        {
            var musicRecommendations = new List<MusicRecommendation>();

            messages["messages"].ForEach(message =>
            {
                try
                {
                    var text = message["text"].Value<string>();
                    var userId = message["user"].Value<string>();

                    var musicRecommendation = new MusicRecommendation
                    {
                        User = userId,
                        Timestamp = message["ts"].Value<string>(),
                        Text = text,
                    };

                    musicRecommendations.Add(musicRecommendation);
                }
                catch (Exception) { }
            });

            musicRecommendations.ForEach(async musicRecommendation =>
            {
                await SendMusicRecommendation(musicRecommendation);
            });
        }

        private async Task SendMusicRecommendation(MusicRecommendation musicRecommendation)
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
        }
    }

    public class MusicRecommendation
    {
        public string User { get; internal set; }
        public string Timestamp { get; internal set; }
        public string Text { get; internal set; }
    }
}