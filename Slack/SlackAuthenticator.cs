using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VariantBot.Slack
{
    public static class SlackAuthenticator
    {
        public static async Task<bool> RequestHasValidSignature(HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);
            var requestBody = await reader.ReadToEndAsync();

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
    }
}