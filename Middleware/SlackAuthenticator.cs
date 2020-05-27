using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace VariantBot.Middleware
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class SlackAuthenticator : IMiddleware
    {
        private static async Task<bool> RequestHasValidSignature(HttpRequest request, string requestBody)
        {
            string slackSignatureHeader = request.Headers["X-Slack-Signature"];
            if (string.IsNullOrWhiteSpace(slackSignatureHeader))
                return false;

            var (slackVersion, slackSignature) =
                GetSlackVersionAndSignature(slackSignatureHeader);

            string slackTimestamp = request.Headers["X-Slack-Request-Timestamp"];
            if (string.IsNullOrWhiteSpace(slackTimestamp))
                return false;

            var signatureBase = $"{slackVersion}:{slackTimestamp}:{requestBody}";
            var signatureSecret = Environment.GetEnvironmentVariable("VARIANT_SLACK_SIGNATURE_SECRET");

            var signature = GetSignature(signatureBase, signatureSecret);

            var requestHasValidSignature = signature.Equals(slackSignature);
            return requestHasValidSignature;
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

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body);
            var requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(requestBody) && requestBody.Contains("AppServiceClientSecret"))
            {
                var parsedRequestBody = JObject.Parse(requestBody);
                var slackSigningSecret = parsedRequestBody["AppServiceClientSecret"].Value<string>();
                if (slackSigningSecret.Equals(
                    Environment.GetEnvironmentVariable("VARIANT_BOT_CLIENT_SECRET")))
                {
                    await next(context);
                    return;
                }

                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            if (!await RequestHasValidSignature(context.Request, requestBody))
            {
                context.Response.Clear();
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            if (requestBody.Contains("challenge"))
            {
                var parsedRequestBody = JObject.Parse(requestBody);
                var challenge = parsedRequestBody["challenge"].Value<string>();
                context.Response.Clear();
                if (string.IsNullOrWhiteSpace(challenge))
                {
                    context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync("Could not parse challenge");
                }

                context.Response.StatusCode = (int) HttpStatusCode.OK;
                await context.Response.WriteAsync(challenge);
                return;
            }

            await next(context);
        }
    }
}