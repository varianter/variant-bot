using System;

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
            Environment.GetEnvironmentVariable("VARIANT_SLACK_THEME");

        private static string SlackGuide =>
            Environment.GetEnvironmentVariable("VARIANT_SLACK_GUIDE");

        private static string TravelBill =>
            Environment.GetEnvironmentVariable("VARIANT_TRAVEL_BILL");
        
        public static async void SendInteractionResponse(string interactionValue, string url)
        {
            var responseString = string.Empty;
            
            switch (interactionValue)
            {
                case InteractionValue.Wifi:
                    responseString += Info.WifiSSIDAndPassword;
                    break;

                case InteractionValue.OrganizationNr:
                case InteractionValue.Address:
                    responseString += Info.OrgNrAndAddress;
                    break;

                case InteractionValue.WebMail:
                    responseString += Info.WebMail;
                    break;
                
                case InteractionValue.TravelInsurance:
                    responseString += Info.TravelInsurance;
                    break;
                
                case InteractionValue.OwnerChangeForm:
                    responseString += Info.OwnerChangeForm;
                    break;
                
                case InteractionValue.SlackTheme:
                    responseString += Info.SlackTheme;
                    break;
                
                case InteractionValue.SlackGuide:
                    responseString += Info.SlackGuide;
                    break;

                case InteractionValue.TravelBill:
                    responseString += Info.TravelBill;
                    break;
                
                default:
                    responseString += $"Kjenner ikke til \"{interactionValue}\", dessverre :sweat_smile:";
                    break;
            }

            await EphemeralSlackMessage.PostSimpleTextMessage(responseString, url);
        }
    }
}