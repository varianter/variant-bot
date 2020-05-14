using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VariantBot.Slack;

namespace VariantBot.Controllers
{
    [Route("api/actions")]
    [ApiController]
    public class ActionsController : ControllerBase
    {
        private readonly ILogger<ActionsController> _logger;

        public ActionsController(ILogger<ActionsController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromForm] SlackActionFormBody slackInteractionFormBody)
        {
            if (!await SlackAuthenticator.RequestHasValidSignature(Request))
            {
                _logger.LogError("Invalid or missing Slack signature");
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(slackInteractionFormBody.Payload))
                return BadRequest();

            var jsonPayload = JObject.Parse(slackInteractionFormBody.Payload);
            var interactionValue = jsonPayload["actions"][0]["value"].Value<string>();
            var userId = jsonPayload["user"]["id"].Value<string>();
            var channelId = jsonPayload["channel"]["id"].Value<string>();

            if (string.IsNullOrWhiteSpace(interactionValue))
                return BadRequest();

            await Info.SendInteractionResponse(interactionValue,
                "https://slack.com/api/chat.postEphemeral", channelId, userId);

            if (interactionValue.Equals(Info.InteractionValue.SlackTheme))
            {
                var slackTheme = Environment.GetEnvironmentVariable("VARIANT_SLACK_THEME");
                await SlackMessage.Post(
                    $"{{\"channel\": \"{userId}\",\"text\": \"{slackTheme}\"}}",
                    "https://slack.com/api/chat.postMessage");
            }

            return Ok();
        }


        public class SlackActionFormBody
        {
            public string Payload { get; set; }
        }
    }
}