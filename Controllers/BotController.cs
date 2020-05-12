using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VariantBot.Slack;

namespace VariantBot.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;

        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }

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

        public class SlackInteractionFormBody
        {
            public string Payload { get; set; }

            public string Text { get; set; }
            public string Command { get; set; }

            [FromForm(Name = "response_url")] public string ResponseUrl { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromForm] SlackInteractionFormBody slackInteractionFormBody)
        {
            if (!await RequestHasValidSignature())
                return Unauthorized();

            if (!string.IsNullOrWhiteSpace(slackInteractionFormBody.Command))
            {
                // Handle commands
                switch (slackInteractionFormBody.Command)
                {
                    case "/info":
                    {
                        EphemeralSlackMessageBody ephemeralMessage;
                        if (!string.IsNullOrWhiteSpace(slackInteractionFormBody.Text)
                            && slackInteractionFormBody.Text.Equals("wifi"))
                        {
                            ephemeralMessage = SlackHandler
                                .CreateSimpleTextEphemeralMessage(
                                    Environment.GetEnvironmentVariable("VARIANT_WIFI_SSID_AND_PASSWORD"));
                            await SlackHandler.PostEphemeralSlackMessage(_httpClientFactory.CreateClient(),
                                ephemeralMessage, slackInteractionFormBody.ResponseUrl);
                            return Ok();
                        }

                        ephemeralMessage = SlackHandler.CreateInfoCommandEphemeralMessage();
                        await SlackHandler.PostEphemeralSlackMessage(
                            _httpClientFactory.CreateClient(), ephemeralMessage, slackInteractionFormBody.ResponseUrl);
                        return Ok();
                    }
                }
            }

            else if (!string.IsNullOrWhiteSpace(slackInteractionFormBody.Payload))
            {
                // Handle message interactions
                var jsonPayload = JObject.Parse(slackInteractionFormBody.Payload);
                var interactionValue = jsonPayload["actions"][0]["value"].Value<string>();

                if (string.IsNullOrWhiteSpace(interactionValue))
                    return BadRequest();

                switch (interactionValue)
                {
                    case "wifi":
                    {
                        var ephemeralMessage = SlackHandler
                            .CreateSimpleTextEphemeralMessage(
                                Environment.GetEnvironmentVariable("VARIANT_WIFI_SSID_AND_PASSWORD"));
                        await SlackHandler.PostEphemeralSlackMessage(_httpClientFactory.CreateClient(),
                            ephemeralMessage, jsonPayload["response_url"].Value<string>());
                        return Ok();
                    }
                }
            }

            return BadRequest();
        }

        private async Task<bool> RequestHasValidSignature()
        {
            using var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();
            
            string slackSignatureHeader = Request.Headers["X-Slack-Signature"];
            if (string.IsNullOrWhiteSpace(slackSignatureHeader))
                return false;

            var (slackVersion, slackSignature) =
                GetSlackVersionAndSignature(slackSignatureHeader);

            string slackTimestamp = Request.Headers["X-Slack-Request-Timestamp"];
            if (string.IsNullOrWhiteSpace(slackTimestamp))
                return false;

            var signatureBase = $"{slackVersion}:{slackTimestamp}:{requestBody}";
            var signatureSecret = Environment.GetEnvironmentVariable("SLACK_SIGNATURE_SECRET");

            var signature = GetSignature(signatureBase, signatureSecret);

            var requestHasValidSignature = signature.Equals(slackSignature);
            if (requestHasValidSignature)
                return true;
            
            _logger.LogError("Invalid or missing Slack signature");
            return false;
        }

        private static string GetSignature(string signatureBase, string signatureSecret)
        {
            var encoding = new UTF8Encoding();

            var signatureBaseBytes = encoding.GetBytes(signatureBase);
            var signatureSecretBytes = encoding.GetBytes(signatureSecret);

            byte[] signature;

            using (var hash = new HMACSHA256(signatureSecretBytes))
                signature = hash.ComputeHash(signatureBaseBytes);

            return BitConverter.ToString(signature).Replace("-", "").ToLower();
        }

        private static (string slackVersion, string slackSignature)
            GetSlackVersionAndSignature(string slackSignatureHeader)
        {
            // Header has format 'version=signature', v0=2fb833...
            var splitString = slackSignatureHeader.Split("=");
            return splitString.Length < 2 ? (string.Empty, string.Empty) : (splitString[0], splitString[1]);
        }
    }
}