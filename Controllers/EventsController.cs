using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VariantBot.Slack;

namespace VariantBot.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;
        private readonly SlackMessageHandler _slackMessageHandler;

        public EventsController(ILogger<EventsController> logger, SlackMessageHandler slackMessageHandler)
        {
            _logger = logger;
            _slackMessageHandler = slackMessageHandler;
        }

        public async Task<IActionResult> PostAsync([FromBody] SlackEventBody eventBody)
        {
            _logger.LogDebug("New message from Slack");
            _slackMessageHandler.HandleMessage(eventBody); 
            return Ok();
        }

        public class SlackEventBody
        {
            [JsonProperty("event")]
            public Event Event { get; set; }
        }
        public class Event
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("user")]
            public string User { get; set; }

            [JsonProperty("blocks")]
            public Block[] Blocks { get; set; }

            [JsonProperty("channel")]
            public string Channel { get; set; }

        }
    }
}