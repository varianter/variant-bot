using System;
using System.Threading.Tasks;

namespace VariantBot.Slack
{
    public static class Info
    {
        public static class InteractionValue
        {
            public const string Wifi = "wifi";
            public const string OrganizationNr = "organizationNr";
            public const string Address = "adr";
            public const string WebMail = "webMail";
            public const string TravelInsurance = "travelInsurance";
            public const string TravelBill = "travelBill";
            public const string OwnerChangeForm = "ownerChangeForm";
            public const string SlackTheme = "slackTheme";
            public const string SlackGuide = "slackGuide";
        }

        private static string WifiSSIDAndPassword =>
            Environment.GetEnvironmentVariable("VARIANT_WIFI_SSID_AND_PASSWORD");

        private static string OrgNrAndAddress =>
            Environment.GetEnvironmentVariable("VARIANT_ORG_NR_AND_ADR");

        private static string WebMail =>
            Environment.GetEnvironmentVariable("VARIANT_WEBMAIL");

        private static string TravelInsurance =>
            Environment.GetEnvironmentVariable("VARIANT_TRAVEL_INSURANCE");

        private static string OwnerChangeForm =>
            Environment.GetEnvironmentVariable("VARIANT_OWNER_CHANGE_FORM");

        private static string SlackTheme =>
            "Slack theme sendt på PM! (Så får du en knapp som skifter tema med et klikk)";

        private static string SlackGuide =>
            Environment.GetEnvironmentVariable("VARIANT_SLACK_GUIDE");

        private static string TravelBill =>
            Environment.GetEnvironmentVariable("VARIANT_TRAVEL_BILL");

        public static async Task SendInteractionResponse(string interactionValue,
            string url, string channelId = null, string userId = null)
        {
            var responseString = string.Empty;

            switch (interactionValue)
            {
                case InteractionValue.Wifi:
                    responseString += WifiSSIDAndPassword;
                    break;

                case InteractionValue.OrganizationNr:
                case InteractionValue.Address:
                    responseString += OrgNrAndAddress;
                    break;

                case InteractionValue.WebMail:
                    responseString += WebMail;
                    break;

                case InteractionValue.TravelInsurance:
                    responseString += TravelInsurance;
                    break;

                case InteractionValue.OwnerChangeForm:
                    responseString += OwnerChangeForm;
                    break;

                case InteractionValue.SlackTheme:
                    responseString += SlackTheme;
                    break;

                case InteractionValue.SlackGuide:
                    responseString += SlackGuide;
                    break;

                case InteractionValue.TravelBill:
                    responseString += TravelBill;
                    break;

                default:
                    responseString += $"Kjenner ikke til \"{interactionValue}\", dessverre :sweat_smile:";
                    break;
            }

            await SlackMessage.PostSimpleTextMessage(responseString, url, channelId, userId);
        }
    }
}