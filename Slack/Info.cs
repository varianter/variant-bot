using System;

namespace VariantBot.Slack
{
    public static class Info
    {
        public static string WifiSSIDAndPassword =>
            Environment.GetEnvironmentVariable("VARIANT_WIFI_SSID_AND_PASSWORD");
    }
}