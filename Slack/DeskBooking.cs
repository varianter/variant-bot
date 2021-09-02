using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VariantBot.Slack
{
    internal static class DeskBooking
    {
        private const string PostMessageUrl = "https://slack.com/api/chat.postMessage";
        private const string ChannelId = "C0139UY6M1D"; // #desk-booking-trh = C0139UY6M1D
        private const string DateFormat = "dddd dd. MMM";
        private static readonly CultureInfo CultureInfo = new("nb-NO");
        private const int BaseDayOffset = 3; // 3 if running on friday

        public static async Task PostDeskBookingDays()
        {
            var today = DateTime.Now;

            var monday = today.AddDays(BaseDayOffset);
            var text = monday.Date.ToString(DateFormat, CultureInfo).FirstCharToUpper(); 
            var mondayUrl = $"{PostMessageUrl}?channel={ChannelId}&text={text}";
            await SlackMessage.Post("", mondayUrl);
            
            Thread.Sleep(50);
            var tuesday = today.AddDays(BaseDayOffset + 1);
            text = tuesday.Date.ToString(DateFormat, CultureInfo).FirstCharToUpper();
            var tuesdayUrl = $"{PostMessageUrl}?channel={ChannelId}&text={text}";
            await SlackMessage.Post("", tuesdayUrl);

            Thread.Sleep(50);
            var wednesday = today.AddDays(BaseDayOffset + 2);
            text = wednesday.Date.ToString(DateFormat, CultureInfo).FirstCharToUpper();
            var wednesdayUrl = $"{PostMessageUrl}?channel={ChannelId}&text={text}";
            await SlackMessage.Post("", wednesdayUrl);

            Thread.Sleep(50);
            var thursday = today.AddDays(BaseDayOffset + 3);
            text = thursday.Date.ToString(DateFormat, CultureInfo).FirstCharToUpper();
            var thursdayUrl = $"{PostMessageUrl}?channel={ChannelId}&text={text}";
            await SlackMessage.Post("", thursdayUrl);
            
            Thread.Sleep(50);
            var friday = today.AddDays(BaseDayOffset + 4);
            text = friday.Date.ToString(DateFormat, CultureInfo).FirstCharToUpper();
            var fridayUrl = $"{PostMessageUrl}?channel={ChannelId}&text={text}";
            await SlackMessage.Post("", fridayUrl);
        }
        
        private static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input[1..]
            };
    }
}