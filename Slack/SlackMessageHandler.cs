using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VariantBot.Slack
{
    public class SlackMessageHandler
    {
        private readonly ILogger<SlackMessageHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public SlackMessageHandler(ILogger<SlackMessageHandler> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task HandleMessage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogDebug("New message from Slack");

            var slackMessage = turnContext.Activity.ChannelData["SlackMessage"] as JObject;

            try
            {
                var urls = (slackMessage["event"]["blocks"]
                    .Where(block => block["type"].Value<string>().Equals("rich_text"))
                    .SelectMany(block => block["elements"], (block, blockElement) => new {block, blockElement})
                    .Where(@t => @t.blockElement["type"].Value<string>().Equals("rich_text_section"))
                    .SelectMany(@t => @t.blockElement["elements"], (@t, element) => new {@t, element})
                    .Where(@t => @t.element["type"].Value<string>().Equals("link"))
                    .Select(@t => @t.element["url"].Value<string>())).ToList();

                if (!urls.Any())
                    return;


                var userId = turnContext.Activity.From.Id.Split(":").FirstOrDefault();
                var channelId = slackMessage["event"]["channel"].Value<string>();

                var ephemeralMessageBody = CreateNewsletterUrlEphemeralMessage(channelId, userId);
                await PostEphemeralSlackMessage(_httpClientFactory.CreateClient(), ephemeralMessageBody,
                    "https://slack.com/api/chat.postEphemeral");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse SlackMessage");
            }
        }

        public static EphemeralSlackMessageBody CreateSimpleTextEphemeralMessage(string text)
        {
            return new EphemeralSlackMessageBody
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

        public static EphemeralSlackMessageBody CreateInfoCommandEphemeralMessage()
        {
            return new EphemeralSlackMessageBody
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
                                Value = "wifi"
                            }
                        }
                    }
                }
            };
        }

        private static EphemeralSlackMessageBody CreateNewsletterUrlEphemeralMessage(string channelId,
                string userId)
            {
                return new EphemeralSlackMessageBody
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

            public static async Task PostEphemeralSlackMessage(HttpClient httpClient,
                EphemeralSlackMessageBody ephemeralSlackMessageBody, string url)
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer",
                        Environment.GetEnvironmentVariable("SLACK_OAUTH_ACCESS_TOKEN"));

                var contentString = JsonConvert.SerializeObject(ephemeralSlackMessageBody);
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(contentString, Encoding.UTF8, "application/json")
                };

                var result = await httpClient.SendAsync(httpRequest);
                var resultString = await result.Content.ReadAsStringAsync();

                if (!result.IsSuccessStatusCode ||
                    (!resultString.Contains("\"ok\":true") && !resultString.Contains("ok")))
                {
                    throw new Exception(
                        $"Failed to send ephemeral Slack message, response status code was: '{result.StatusCode}'. Message body: '{resultString}'");
                }
            }
        }
    }