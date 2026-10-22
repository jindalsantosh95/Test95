using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.GitHub.Models
{
    public sealed class GitHubRepositoryResponse
    {
        [JsonProperty("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonProperty("default_branch")]
        public string DefaultBranch { get; set; } = string.Empty;
    }
}
