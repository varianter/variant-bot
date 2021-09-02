using System;
using System.Threading.Tasks;

namespace VariantBot.Slack
{
    public static class KeeperAliver
    {
        private const string PostMessageUrl = "https://slack.com/api/chat.postMessage";
        private const string ChannelId = "C02CUFUGZN3"; // #ping in prod

        public static async Task DontDie()
        {
            try
            {
                var url = $"{PostMessageUrl}?channel={ChannelId}&text=ping :robot:";
                await SlackMessage.Post("", url);
            }
            catch (Exception e)
            {
                Console.WriteLine("Aww man, ping failed");
            }
        }
    }
}