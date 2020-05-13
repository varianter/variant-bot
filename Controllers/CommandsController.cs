using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VariantBot.Slack;

namespace VariantBot.Controllers
{
    [Route("api/commands")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ILogger<CommandsController> _logger;

        public CommandsController(ILogger<CommandsController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromForm] SlackCommandFormBody slackCommandFormBody)
        {
            if (!await SlackAuthenticator.RequestHasValidSignature(Request))
            {
                _logger.LogError("Invalid or missing Slack signature");
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(slackCommandFormBody.Command))
                return BadRequest();

            switch (slackCommandFormBody.Command)
            {
                case "/info":
                {
                    if (!string.IsNullOrWhiteSpace(slackCommandFormBody.Text))
                    {
                        Info.SendInteractionResponse(slackCommandFormBody.Text, slackCommandFormBody.ResponseUrl);
                        return Ok();
                    }

                    var ephemeralMessage = SlackMessage.CreateInfoCommandMessage();
                    await SlackMessage.PostMessage(ephemeralMessage, slackCommandFormBody.ResponseUrl);
                    return Ok();
                }
            }

            return BadRequest();
        }

        public class SlackCommandFormBody
        {
            public string Text { get; set; }
            public string Command { get; set; }

            [FromForm(Name = "response_url")] public string ResponseUrl { get; set; }
        }
    }
}