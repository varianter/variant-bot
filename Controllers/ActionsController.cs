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
            if (string.IsNullOrWhiteSpace(slackInteractionFormBody.Payload))
                return BadRequest();

            var jsonPayload = JObject.Parse(slackInteractionFormBody.Payload);
            var interactionValue = jsonPayload["actions"][0]["value"].Value<string>();
            var responseUrl = jsonPayload["response_url"].Value<string>();

            if (string.IsNullOrWhiteSpace(interactionValue))
                return BadRequest();

            if (interactionValue.StartsWith(InteractionValueConstants.NewsletterUrlConfirmation))
            {
                await SlackMessage.PostSimpleTextMessage($"Show dialog here, URL I got was {interactionValue}",
                    responseUrl, null, null);
                return Ok();
            }

            await Info.SendInteractionResponse(interactionValue,
                responseUrl);

            return Ok();
        }


        public class SlackActionFormBody
        {
            public string Payload { get; set; }
        }
    }
}