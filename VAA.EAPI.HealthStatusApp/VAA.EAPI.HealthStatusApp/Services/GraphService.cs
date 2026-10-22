using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VAA.EAPI.HealthStatusApp.Models;

namespace VAA.EAPI.HealthStatusApp.Services
{
    public class GraphService : IGraphService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IConfidentialClientApplication _confidentialClientApp;

        public GraphService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            _confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(_configuration["AzureAD_ClientId"])
                .WithClientSecret(_configuration["AzureAD_ClientSecret"])
                .WithAuthority(new Uri($"{_configuration["AzureAD_Instance"]}{_configuration["AzureAD_TenantId"]}"))
                .Build();
        }

        public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
        {
            try
            {
                var authResult = await _confidentialClientApp
                    .AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                    .ExecuteAsync();

                var accessToken = authResult.AccessToken;

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var payload = new { groupIds = new[] { groupId }};

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://graph.microsoft.com/v1.0/users/{userId}/checkMemberGroups", content);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Application lacks permission to query this resource. Check API permissions.");
                }

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CheckMemberGroupsResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Value.Contains(groupId) ?? false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Authorization issue: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
    }
}
