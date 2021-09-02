using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MoreLinq.Extensions;

namespace VariantBot.Services
{
    public class MusicRecommendationService
    {
        private readonly ILogger<MusicRecommendationService> _logger;
        private readonly string _musicAppFunctionUrl;
        private static readonly HttpClient HttpClient = new();

        public MusicRecommendationService(ILogger<MusicRecommendationService> logger,
            IOptions<MusicRecommendationAppConfig> musicAppConfig)
        {
            _logger = logger;
            _musicAppFunctionUrl = musicAppConfig.Value.URL;
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

        public void HandleMessages(JToken messages)
        {
            var musicRecommendations = new List<MusicRecommendation>();
            try
            {
                messages.ForEach(message =>
                {
                    var botId = message["bot_id"]?.Value<string>();
                    if (botId is not null)
                        return;
                    
                    var text = message["text"]?.Value<string>();
                    var userId = message["user"]?.Value<string>();
                    var timestamp = message["ts"]?.Value<string>();

                    if (!string.IsNullOrEmpty(timestamp) &&timestamp.Contains("."))
                    {
                        timestamp = timestamp.Split(".")[0];
                    }

                    var musicRecommendation = new MusicRecommendation
                    {
                        User = userId,
                        Timestamp = timestamp,
                        Text = text,
                    };

                    musicRecommendations.Add(musicRecommendation);
                });
                
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Music recommendation parsing failed");
            }
            
            
            musicRecommendations.ForEach(async musicRecommendation =>
            {
                await SendMusicRecommendation(musicRecommendation);
            });
        }

        private async Task SendMusicRecommendation(MusicRecommendation musicRecommendation)
        {
            _logger.LogInformation($"Sending new music recommendation: {musicRecommendation.Text}");

            var json = JsonConvert.SerializeObject(musicRecommendation);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _musicAppFunctionUrl)
            {
                Content = content
            };

            using var result = await HttpClient.SendAsync(httpRequest);

            var resultString = await result.Content.ReadAsStringAsync();

            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"Music recommendation app call failed: {resultString}");
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