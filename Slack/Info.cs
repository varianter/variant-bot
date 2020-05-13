using System;

namespace VariantBot.Slack
{
    public static class Info
    {
        private static string WifiSSIDAndPassword =>
            Environment.GetEnvironmentVariable("VARIANT_WIFI_SSID_AND_PASSWORD");


        public static async void SendInteractionResponse(string interactionValue, string url)
        {
            switch (interactionValue)
            {
                case "wifi":
                {
                    await EphemeralSlackMessage
                        .PostSimpleTextMessage(Info.WifiSSIDAndPassword, url);
                    break;
                }
            }
        }
    }
}