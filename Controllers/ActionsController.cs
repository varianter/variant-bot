using System;
using System.Net.Http;
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
        private readonly IHttpClientFactory _httpClientFactory;

        public ActionsController(ILogger<ActionsController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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

            if (string.IsNullOrWhiteSpace(interactionValue))
                return BadRequest();

            switch (interactionValue)
            {
                case "wifi":
                {
                    var ephemeralMessage = EphemeralSlackMessage
                        .CreateSimpleTextMessage(
                            Environment.GetEnvironmentVariable("VARIANT_WIFI_SSID_AND_PASSWORD"));
                    await EphemeralSlackMessage.PostMessage(_httpClientFactory.CreateClient(),
                        ephemeralMessage, jsonPayload["response_url"].Value<string>());
                    return Ok();
                }
            }

            return BadRequest();
        }


        public class SlackActionFormBody
        {
            public string Payload { get; set; }
        }
    }
}