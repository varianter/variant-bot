﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VariantBot.Slack
{
    public class EphemeralSlackMessage
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        [JsonProperty("channel")] public string Channel { get; set; }

        [JsonProperty("user")] public string User { get; set; }

        [JsonProperty("blocks")] public Block[] Blocks { get; set; }

        public static async Task PostSimpleTextMessage(string message, string url)
        {
            var ephemeralMessage = CreateSimpleTextMessage(message);
            await PostMessage(ephemeralMessage, url);
        }

        public static async Task PostMessage(EphemeralSlackMessage ephemeralSlackMessage, string url)
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    Environment.GetEnvironmentVariable("SLACK_OAUTH_ACCESS_TOKEN"));

            var contentString = JsonConvert.SerializeObject(ephemeralSlackMessage);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(contentString, Encoding.UTF8, "application/json")
            };

            using var result = await HttpClient.SendAsync(httpRequest);
            var resultString = await result.Content.ReadAsStringAsync();

            if (!result.IsSuccessStatusCode ||
                (!resultString.Contains("\"ok\":true") && !resultString.Contains("ok")))
            {
                throw new Exception(
                    $"Failed to send ephemeral Slack message, response status code was: '{result.StatusCode}'. Message body: '{resultString}'");
            }
        }

        public static EphemeralSlackMessage CreateSimpleTextMessage(string text)
        {
            return new EphemeralSlackMessage
            {
                Blocks = new[]
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

        public static EphemeralSlackMessage CreateNewsletterUrlMessage(string channelId,
            string userId)
        {
            return new EphemeralSlackMessage
            {
                Channel = channelId,
                User = userId,
                Blocks = new[]
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
                        Elements = new[]
                        {
                            new Element
                            {
                                Type = "button",
                                Text = new Text
                                {
                                    Type = "plain_text",
                                    TextText = "Make it so"
                                },
                                Value = "URL values go here"
                            }
                        }
                    }
                }
            };
        }

        public static EphemeralSlackMessage CreateInfoCommandMessage()
        {
            return new EphemeralSlackMessage
            {
                Blocks = new[]
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
                    },
                    new Block
                    {
                        Type = "actions",
                        Elements = new[]
                        {
                            new Element
                            {
                                Type = "button",
                                Text = new Text
                                {
                                    Type = "plain_text",
                                    TextText = "Wifi"
                                },
                                Value = Info.InteractionValue.Wifi
                            },
                            new Element
                            {
                                Type = "button",
                                Text = new Text
                                {
                                    Type = "plain_text",
                                    TextText = "OrgNr/Adr"
                                },
                                Value = Info.InteractionValue.OrganizationNr
                            },
                            new Element
                            {
                                Type = "button",
                                Text = new Text
                                {
                                    Type = "plain_text",
                                    TextText = "E-post"
                                },
                                Value = Info.InteractionValue.WebMail
                            },
                            new Element
                            {
                                Type = "button",
                                Text = new Text
                                {
                                    Type = "plain_text",
                                    TextText = "Reiseforsikring"
                                },
                                Value = Info.InteractionValue.TravelInsurance
                            },
                            new Element
                            {
                                Type = "button",
                                Text = new Text
                                {
                                    Type = "plain_text",
                                    TextText = "Reiseregning"
                                },
                                Value = Info.InteractionValue.TravelBill
                            },
                        }
                    },
                    new Block
                    {
                        Type = "actions",
                        Elements = new[]
                        {
                            new Element
                            {
                                Type = "button",
                                Text = new Text
                                {
                                    Type = "plain_text",
                                    TextText = "Eierskifteskjema"
                                },
                                Value = Info.InteractionValue.OwnerChangeForm
                            },
                            new Element
                            {
                                Type = "button",
                                Text = new Text
                                {
                                    Type = "plain_text",
                                    TextText = "Slack-guide"
                                },
                                Value = Info.InteractionValue.SlackGuide
                            },
                            new Element
                            {
                                Type = "button",
                                Text = new Text
                                {
                                    Type = "plain_text",
                                    TextText = "Slack-theme"
                                },
                                Value = Info.InteractionValue.SlackTheme
                            },
                        }
                    }
                }
            };
        }
    }

    public class Block
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public Text Text { get; set; }

        [JsonProperty("elements", NullValueHandling = NullValueHandling.Ignore)]
        public Element[] Elements { get; set; }
    }


    public class Element
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("text")] public Text Text { get; set; }

        [JsonProperty("value")] public string Value { get; set; }
    }

    public class Text
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("text")] public string TextText { get; set; }
    }
}