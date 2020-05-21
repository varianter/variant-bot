using System;
using System.Linq;
using System.Threading.Tasks;

namespace VariantBot.Slack
{
    public static class Info
    {
        public class InfoItem
        {
            public string InteractionValue { get; set; }
            public string ResponseText { get; set; }
        }

        private static string SlackTheme =>
            $"Slack theme sendt på PM! :art: {Environment.NewLine} (Så får du en knapp som skifter tema (funker ikke på mobil :no_mobile_phones:))";


        public static async Task SendInteractionResponse(string interactionValue,
            string url, string channelId = null, string userId = null)
        {
            var responseString = string.Empty;
            var infoItem = Config.InfoItems
                .FirstOrDefault(infoItem =>
                    infoItem.InteractionValue.Equals(interactionValue, StringComparison.OrdinalIgnoreCase));

            if (infoItem == null)
                responseString += $"Kjenner ikke til \"{interactionValue}\", dessverre :sweat_smile:";
            else if (interactionValue.Equals("Slack-theme"))
            {
                // Slack-theme is special, it needs to be sent as a PM in order 
                // for Slack to display a button that changes theme automatically
                // We also send a response to the channel /info was called from 
                // explaining this. The text used is the "SlackTheme" text in this class
                if (interactionValue.Equals("Slack-theme"))
                {
                    await SlackMessage.Post(
                        $"{{\"channel\": \"{userId}\",\"text\": \"{infoItem.ResponseText}\"}}",
                        "https://slack.com/api/chat.postMessage");

                    responseString += SlackTheme;
                }
            }
            else
                responseString += infoItem.ResponseText;

            await SlackMessage.PostSimpleTextMessage(responseString, url, channelId, userId);
        }
    }
}