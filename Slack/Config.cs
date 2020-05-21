using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ExcelDataReader;
using Newtonsoft.Json;

namespace VariantBot.Slack
{
    public static class Config
    {
        private static readonly string DocumentFolderUrl = Environment.GetEnvironmentVariable("VARIANT_BOT_SHAREPOINT_FOLDER_URL");
        private static readonly string AuthUri = Environment.GetEnvironmentVariable("VARIANT_BOT_SHAREPOINT_AUTH_URL");
        private static readonly ArrayList _infoItems = ArrayList.Synchronized(new ArrayList());
        private static readonly HttpClient HttpClient = new HttpClient();

        public static IEnumerable<Info.InfoItem> InfoItems => _infoItems.Cast<Info.InfoItem>();

        public static async Task LoadConfigFromSharePoint(byte[] documentData = null)
        {
            if (documentData != null)
            {
                ReloadInfoItems(documentData);
                return;
            }

            var formParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id",
                    Environment.GetEnvironmentVariable("VARIANT_BOT_CLIENT_ID")),
                new KeyValuePair<string, string>("client_secret",
                    Environment.GetEnvironmentVariable("VARIANT_BOT_CLIENT_SECRET")),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("resource", "https://graph.microsoft.com")
            };

            string authToken;

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, AuthUri)
            {
                Content = new FormUrlEncodedContent(formParameters)
            };

            using var authResult = await HttpClient.SendAsync(httpRequest);
            if (authResult.IsSuccessStatusCode)
            {
                var resultString = await authResult.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<SharePointAccessToken>(resultString);
                authToken = token.Token;
            }
            else
            {
                throw new Exception("Failed to get auth token for config loading");
            }

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var documentFolderRequest = new HttpRequestMessage(HttpMethod.Get, DocumentFolderUrl);
            var documentFolderResult = await HttpClient.SendAsync(documentFolderRequest);
            var documentFolderString = await documentFolderResult.Content.ReadAsStringAsync();
            var documentFolder = JsonConvert.DeserializeObject<SharePointFolder>(documentFolderString);

            var documentRequest =
                new HttpRequestMessage(HttpMethod.Get, documentFolder.Items[0].MicrosoftGraphDownloadUrl);


            using var documentResult = await HttpClient.SendAsync(documentRequest);
            documentData = await documentResult.Content.ReadAsByteArrayAsync();

            ReloadInfoItems(documentData);
        }

        private static void ReloadInfoItems(byte[] documentData)
        {
            using var excelReader = ExcelReaderFactory.CreateReader(new MemoryStream(documentData));
            var document = excelReader.AsDataSet();

            _infoItems.Clear();

            foreach (DataTable table in document.Tables)
            {
                foreach (DataRow dataRow in table.Rows)
                {
                    _infoItems.Add(new Info.InfoItem
                    {
                        InteractionValue = dataRow.ItemArray[0].ToString(),
                        ResponseText = dataRow.ItemArray[1].ToString()
                    });
                }
            }
        }
    }

    public class SharePointAccessToken
    {
        [JsonProperty("access_token")] public string Token { get; set; }
    }

    public class SharePointFolder
    {
        [JsonProperty("value")] public Item[] Items { get; set; }
    }

    public class Item
    {
        [JsonProperty("@microsoft.graph.downloadUrl")]
        public Uri MicrosoftGraphDownloadUrl { get; set; }
    }
}