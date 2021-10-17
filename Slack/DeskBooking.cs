using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;
using Newtonsoft.Json.Linq;

namespace VariantBot.Slack
{
    internal static class DeskBooking
    {
        private const string PostMessageUrl = "https://slack.com/api/chat.postMessage";
        private const string ChannelId = "C0139UY6M1D"; // #desk-booking-trh = C0139UY6M1D, #desbuk = C02DYPR0WJC
        private const string DateFormat = "dddd dd. MMM";
        private static readonly CultureInfo CultureInfo = new("nb-NO");
        private static int BaseDayOffset;

        public static async Task PostDeskBookingDays()
        {
            var today = DateTime.Now;

            BaseDayOffset = today.DayOfWeek switch
            {
                DayOfWeek.Sunday => 1,
                DayOfWeek.Monday => 0,
                DayOfWeek.Tuesday => 6,
                DayOfWeek.Wednesday => 5,
                DayOfWeek.Thursday => 4,
                DayOfWeek.Friday => 3,
                DayOfWeek.Saturday => 2,
                _ => throw new ArgumentOutOfRangeException()
            };

            
            var postUrls = new List<string>();
            for (var i = 0; i < 5; i++)
            {
                var postUrl = GetPostUrl(today, BaseDayOffset + i);
                postUrls.Add(postUrl);
            }

            var shouldPostNewMessages = true;
            
            var lastTenMessages = await SlackMessage.GetAllMessages(ChannelId, limit: 10);
            var parsedResult = JObject.Parse(lastTenMessages);
            var messages = parsedResult["messages"];
            if (messages is null)
            {
                Console.WriteLine("#ping message history result was null");
            }
            else
            {
                foreach (var message in messages)
                {
                    var msgText = message["text"].Value<string>();
                    if (string.IsNullOrEmpty(msgText)) 
                        return;
                    var messageAlreadyPosted = postUrls.Any(url => url.Contains(msgText));
                    if (!messageAlreadyPosted) continue;
                    shouldPostNewMessages = false;
                    break;
                } 
            }

            if (shouldPostNewMessages)
            {
                foreach (var url in postUrls)
                {
                    await SlackMessage.Post("", url);
                    Thread.Sleep(50);
                }
            }
            else
            {
                Console.WriteLine("Was told not to post new dates");
            }
        }

        private static string GetPostUrl(DateTime today, int offset)
        {
            var day = today.AddDays(offset);
            var text = day.Date.ToString(DateFormat, CultureInfo).FirstCharToUpper(); 
            return $"{PostMessageUrl}?channel={ChannelId}&text={text}"; 
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