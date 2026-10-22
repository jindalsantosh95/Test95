using System;

namespace VAA.EAPI.Microservices.Insights.Domain.GitHub
{
    public sealed class GitHubMergedPullRequest
    {
        public string RepositoryFullName { get; set; } = string.Empty;

        public int Number { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public DateTime MergedAtUtc { get; set; }

        public string MergedBy { get; set; } = string.Empty;
    }
}
