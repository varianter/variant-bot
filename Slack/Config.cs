using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VariantBot.Slack
{
    public static class Config
    {
        private static readonly string SharePointConfigListUrl =
            Environment.GetEnvironmentVariable("VARIANT_BOT_SHAREPOINT_CONFIG_LIST_URL");

        private static readonly string AuthUri = Environment.GetEnvironmentVariable("VARIANT_BOT_SHAREPOINT_AUTH_URL");
        private static readonly HttpClient HttpClient = new HttpClient();

        public static readonly List<Info.InfoItem> InfoItems = new List<Info.InfoItem>();
        public static readonly Dictionary<string, string> DirectInfoTriggers = new Dictionary<string, string>();

        public static async Task LoadConfigFromSharePoint()
        {
            var formParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id",
                    Environment.GetEnvironmentVariable("VARIANT_BOT_CLIENT_ID")),
                new KeyValuePair<string, string>("client_secret",
                    Environment.GetEnvironmentVariable("VARIANT_BOT_CLIENT_SECRET")),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("resource", "https://graph.microsoft.com")
            };

            string authToken;

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, AuthUri)
            {
                Content = new FormUrlEncodedContent(formParameters)
            };

            using var authResult = await HttpClient.SendAsync(httpRequest);
            if (authResult.IsSuccessStatusCode)
            {
                var resultString = await authResult.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<SharePointAccessToken>(resultString);
                authToken = token.Token;
            }
            else
            {
                throw new Exception("Failed to get auth token for config loading");
            }

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var configRequest = new HttpRequestMessage(HttpMethod.Get, SharePointConfigListUrl);
            var configResult = await HttpClient.SendAsync(configRequest);
            var configString = await configResult.Content.ReadAsStringAsync();
            var config = JsonConvert.DeserializeObject<SharePointList>(configString);

            ReloadInfoItems(config);
        }

        private static void ReloadInfoItems(SharePointList config)
        {
            InfoItems.Clear();
            DirectInfoTriggers.Clear();
            
            foreach (var configItem in config.Values)
            {
                var item = new Info.InfoItem()
                {
                    InteractionValue = configItem.Fields.Title,
                    ResponseText = configItem.Fields.ResponseText
                };
                
                DirectInfoTriggers.Add(item.InteractionValue.ToLower(), item.ResponseText);

                foreach (var directTrigger in configItem.Fields.DirectTriggers)
                    DirectInfoTriggers.Add(directTrigger.ToLower(), item.ResponseText);
                
                InfoItems.Add(item);
            }
        }
    }

    public class SharePointAccessToken
    {
        [JsonProperty("access_token")] public string Token { get; set; }
    }

    public class SharePointList
    {
        [JsonProperty("value")] public Value[] Values { get; set; }
    }

    public class Value
    {
        [JsonProperty("fields")] public Fields Fields { get; set; }
    }


    public class Fields
    {
        [JsonProperty("Title")] public string Title { get; set; }

        [JsonProperty("gesj")] public string ResponseText { get; set; }

        [JsonProperty("Direktetrigger")]
        [JsonConverter(typeof(DirectTriggersConverter))]
        public List<string> DirectTriggers { get; set; }
    }

    internal class DirectTriggersConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var value = token.Value<string>();
            return value.Split(" ").ToList();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}