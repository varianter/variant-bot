using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VariantBot.Slack
{
    public class SlackMessage
    {
        private static readonly HttpClient HttpClient = new();

        static SlackMessage()
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    Environment.GetEnvironmentVariable("VARIANT_SLACK_OAUTH_ACCESS_TOKEN"));
        }

        [JsonProperty("replace_original")] public bool ReplaceOriginalMessage => false;
        [JsonProperty("channel")] public string Channel { get; set; }

        [JsonProperty("user")] public string User { get; set; }
        [JsonProperty("ts")] public string Timestamp { get; set; }
        [JsonProperty("text")] public string Text { get; set; }

        [JsonProperty("blocks")] public List<Block> Blocks { get; set; }

        public static async Task PostSimpleTextMessage(string message, string url, string channelId, string userId)
        {
            var jsonContent = JsonConvert.SerializeObject(CreateSimpleTextMessage(message, channelId, userId));
            await Post(jsonContent, url);
        }

        public static async Task Post(string jsonContent, string url)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            using var result = await HttpClient.SendAsync(httpRequest);
            var resultString = await result.Content.ReadAsStringAsync();

            if (!result.IsSuccessStatusCode ||
                (!resultString.Contains("\"ok\":true") && !resultString.Contains("ok")))
            {
                throw new Exception(
                    $"Failed to send Slack message, response status code was: '{result.StatusCode}'. Message body: '{resultString}'");
            }
        }

        public static async Task<string> GetAllMessages(string channelId, string cursor = "")
        {
            var requestUri = $"https://slack.com/api/conversations.history?channel={channelId}&pretty=1";
            if (!string.IsNullOrEmpty(cursor))
                requestUri += $"&cursor={cursor}";
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

            using var result = await HttpClient.SendAsync(httpRequest);

            var resultString = await result.Content.ReadAsStringAsync();

            return resultString;
        }

        private static SlackMessage CreateSimpleTextMessage(string text, string channelId, string userId)
        {
            return new SlackMessage
            {
                Channel = channelId,
                User = userId,
                Blocks = new List<Block>
                {
                    new Block
                    {
                        Type = "section",
                        Text = new Text
                        {
                            Type = "mrkdwn",
                            TextText = text
                        }
                    }
                }
            };
        }

        public static SlackMessage CreateNewsletterUrlConfirmationMessage(string channelId,
            string userId, IEnumerable<string> urls)
        {
            var interactionValue = $"{InteractionValueConstants.NewsletterUrlConfirmation} {string.Join(" ", urls)}";
            return new SlackMessage
            {
                Channel = channelId,
                User = userId,
                Blocks = new List<Block>
                {
                    new Block
                    {
                        Type = "section",
                        Text = new Text
                        {
                            Type = "mrkdwn",
                            TextText =
                                "That looks like an URL\n *Would you like to submit it for the monthly newsletter?*"
                        }
                    },
                    new Block
                    {
                        Type = "actions",
                        Elements = new List<Element>
                        {
                            new Element
                            {
                                Type = "button",
                                Text = new Text
                                {
                                    Type = "plain_text",
                                    TextText = "Make it so"
                                },
                                Value = interactionValue
                            }
                        }
                    }
                }
            };
        }

        public static async Task<SlackMessage> CreateInfoCommandMessage()
        {
            await Config.LoadConfigFromSharePoint();
            var slackMessage = new SlackMessage
            {
                Blocks = new List<Block>
                {
                    new Block
                    {
                        Type = "section",
                        Text = new Text
                        {
                            Type = "mrkdwn",
                            TextText =
                                "Hva lurer du på?"
                        }
                    }
                }
            };

            var buttonBlocks = new List<Block>();

            foreach (var infoItemBatch in Config.InfoItems.Batch(5))
            {
                var elements = infoItemBatch
                    .Select(CreateButtonElement)
                    .ToList();

                var buttonBlock = new Block
                {
                    Type = "actions",
                    Elements = elements
                };

                buttonBlocks.Add(buttonBlock);
            }

            slackMessage.Blocks.AddRange(buttonBlocks);
            return slackMessage;
        }

        private static Element CreateButtonElement(Info.InfoItem infoItem)
        {
            return new Element
            {
                Type = "button",
                Text = new Text
                {
                    Type = "plain_text",
                    TextText = infoItem.InteractionValue
                },
                Value = infoItem.InteractionValue
            };
        }

    }

    public class Block
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public Text Text { get; set; }

        [JsonProperty("elements", NullValueHandling = NullValueHandling.Ignore)]
        public List<Element> Elements { get; set; }
    }


    public class Element
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("text")]
        [JsonConverter(typeof(TextElementConverter))]
        public Text Text { get; set; }

        [JsonProperty("value")] public string Value { get; set; }

        [JsonProperty("url")] public string Url { get; set; }

        public bool ShouldSerializeUrl() => !string.IsNullOrWhiteSpace(Url);

        [JsonProperty("elements")] public List<Element> Elements { get; set; }

        public bool ShouldSerializeElements() => Elements != null;
    }

    public class Text
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("text")] public string TextText { get; set; }
    }

    internal class TextElementConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.String)
            {
                return new Text { TextText = token.Value<string>() };
            }

            return token.ToObject<Text>();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Text);
        }
    }
}