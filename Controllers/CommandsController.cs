using System;
using System.Net.Http;
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
        private readonly IHttpClientFactory _httpClientFactory;

        public CommandsController(ILogger<CommandsController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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

            // Handle commands
            switch (slackCommandFormBody.Command)
            {
                case "/info":
                {
                    EphemeralSlackMessage ephemeralMessage;
                    if (!string.IsNullOrWhiteSpace(slackCommandFormBody.Text)
                        && slackCommandFormBody.Text.Equals("wifi"))
                    {
                        ephemeralMessage = EphemeralSlackMessage
                            .CreateSimpleTextMessage(
                                Environment.GetEnvironmentVariable("VARIANT_WIFI_SSID_AND_PASSWORD"));
                        await EphemeralSlackMessage.PostMessage(_httpClientFactory.CreateClient(),
                            ephemeralMessage, slackCommandFormBody.ResponseUrl);
                        return Ok();
                    }

                    ephemeralMessage = EphemeralSlackMessage.CreateInfoCommandMessage();
                    await EphemeralSlackMessage.PostMessage(
                        _httpClientFactory.CreateClient(), ephemeralMessage, slackCommandFormBody.ResponseUrl);
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