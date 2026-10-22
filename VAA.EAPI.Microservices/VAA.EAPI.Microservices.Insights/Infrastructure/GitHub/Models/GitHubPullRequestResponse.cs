using Newtonsoft.Json;
using System;
using VAA.EAPI.Microservices.Insights.Domain.GitHub;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.GitHub.Models
{
    public sealed class GitHubPullRequestResponse
    {
        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("html_url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("merged_at")]
        public DateTime? MergedAt { get; set; }

        [JsonProperty("base")]
        public GitHubPullRequestBaseResponse Base { get; set; } = new();

        [JsonProperty("merged_by")]
        public GitHubUserResponse? MergedBy { get; set; }

        [JsonIgnore]
        public string RepositoryFullName { get; set; } = string.Empty;

        public static explicit operator GitHubMergedPullRequest(GitHubPullRequestResponse source)
        {
            return new GitHubMergedPullRequest
            {
                RepositoryFullName = source.RepositoryFullName,
                Number = source.Number,
                Title = source.Title,
                Url = source.Url,
                MergedAtUtc = source.MergedAt ?? DateTime.MinValue,
                MergedBy = source.MergedBy?.Login ?? string.Empty
            };
        }
    }

    public sealed class GitHubPullRequestBaseResponse
    {
        [JsonProperty("ref")]
        public string Ref { get; set; } = string.Empty;
    }

    public sealed class GitHubUserResponse
    {
        [JsonProperty("login")]
        public string Login { get; set; } = string.Empty;
    }
}
