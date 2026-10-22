using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Helpers
{
    public static class AzureDevOpsWikiUploader
    {
        private static readonly string env = Config.Environment;
        private static readonly string organization = "virginatlantic";
        private static readonly string project = "Digital Distribution";
        private static readonly string wikiIdentifier = "Digital-Distribution.wiki";
        private static readonly string pat = Vault.Default.GetSecretValue("AzurePersonalAccessToken");

        public static async Task UploadMarkdownAsync(string markdownContent, string path)
        {
            string pagePath = $"Backend API Created Test Data/{path} - {env.ToUpper()}";
            string baseUrl = $"https://dev.azure.com/{organization}/{project}/_apis/wiki/wikis/{wikiIdentifier}/pages";
            string getUrl = $"{baseUrl}?path={Uri.EscapeDataString(pagePath)}&includeContent=true&api-version=7.1";
            string putUrl = $"{baseUrl}?path={Uri.EscapeDataString(pagePath)}&api-version=7.1";

            using (HttpClient client = new HttpClient())
            {
                // Set auth header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}")));

                // Step 1: Get current page to retrieve ETag
                System.Net.Http.HttpResponseMessage getResponse = await client.GetAsync(getUrl);

                getResponse.EnsureSuccessStatusCode();

                string etag = getResponse.Headers.ETag?.Tag?.Trim('"');

                // Step 2: Prepare PUT request
                object body = new { content = markdownContent };
                string json = JsonConvert.SerializeObject(body);
                System.Net.Http.HttpRequestMessage putRequest = new HttpRequestMessage(HttpMethod.Put, putUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                // Include ETag for update
                if (!etag.IsNullOrWhiteSpace())
                    putRequest.Headers.IfMatch.Add(new EntityTagHeaderValue($"\"{etag}\""));

                // Step 3: Send PUT request
                System.Net.Http.HttpResponseMessage putResponse = await client.SendAsync(putRequest);

                putResponse.EnsureSuccessStatusCode();
            }
        }
    }
}
