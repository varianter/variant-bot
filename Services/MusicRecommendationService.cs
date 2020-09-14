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
    public static class StringExtensions
    {
        public static (string id, string type) FindMusicInformation(this string text)
        {
            return (id: "", type: "");
        }
    }
    
    public enum MusicPlatform
    {
        Spotify,
        YouTube,
        AppleMusic,
        Tidal
    }
    
    public class MusicRecommendationService
    {
        private readonly ILogger<MusicRecommendationService> _logger;
        private static readonly HttpClient HttpClient = new HttpClient();

        public MusicRecommendationService(ILogger<MusicRecommendationService> logger)
        {
            _logger = logger;
        }
        
        public void HandleMessages(JObject messages)
        {
            var musicRecommendations = new List<MusicRecommendation>();

            messages["messages"].ForEach(message =>
            {
                try
                {
                    var text = message["text"].Value<string>();

                    var (id, type) = text.FindMusicInformation();

                    if (string.IsNullOrWhiteSpace(id)) 
                        return;
                    
                    var userId = message["user"].Value<string>();
                    var musicRecommendation = new MusicRecommendation
                    {
                        User = userId,
                        Timestamp = message["ts"].Value<string>(),
                        Id = id,
                        Type = type,
                        MusicPlatform = MusicPlatform.Spotify
                    };

                    musicRecommendations.Add(musicRecommendation);
                }
                catch (Exception) { }
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
    }
    
    public class MusicRecommendation
    {
        public string User { get; internal set; }
        public string Timestamp { get; internal set; }
        public string Id { get; internal set; }
        public string Type { get; internal set; }
        public MusicPlatform MusicPlatform { get; internal set; }
    }
}