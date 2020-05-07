﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;

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

        public ActionsController(ILogger<ActionsController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            using var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            if (!RequestHasValidSignature(requestBody))
            {
                _logger.LogError("Invalid or missing Slack signature");
                return;
            }
        }

        private bool RequestHasValidSignature(string requestBody)
        {
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

            return signature.Equals(slackSignature);
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