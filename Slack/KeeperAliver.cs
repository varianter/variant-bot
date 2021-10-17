using System;
using System.Text.Json;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Newtonsoft.Json.Linq;

namespace VariantBot.Slack
{
    public static class KeeperAliver
    {
        private const string DeleteMessageUrl = "https://slack.com/api/chat.delete";
        private const string PostMessageUrl = "https://slack.com/api/chat.postMessage";
        private const string ChannelId = "C02CUFUGZN3"; // #ping in prod

        public static async Task DontDie()
        {
            try
            {
                var url = $"{PostMessageUrl}?channel={ChannelId}&text=ping :robot:";
                await SlackMessage.Post("", url);
            }
            catch (Exception)
            {
                Console.WriteLine("Aww man, ping failed");
            }
        }

        public static async Task DeleteMessages()
        {
            try
            {
                var allMessages = await SlackMessage.GetAllMessages(ChannelId);
                var parsedResult = JObject.Parse(allMessages);
                var messages = parsedResult["messages"];
                if (messages is null)
                {
                    Console.WriteLine("#ping message history result was null");
                    return;
                }

                messages.ForEach(async message =>
                {
                    var timestamp = message["ts"]?.Value<string>();
                    var json = new
                    {
                        channel = ChannelId, ts = timestamp
                    };

                    var serializedJson = JsonSerializer.Serialize(json);
                    await SlackMessage.Post(serializedJson, DeleteMessageUrl);
                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}