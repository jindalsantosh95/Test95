using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Domain.GitHub
{
    public sealed class GitHubRepositoryMergedPullRequests
    {
        public string RepositoryFullName { get; set; } = string.Empty;

        public string DefaultBranch { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;

        public IReadOnlyCollection<GitHubMergedPullRequest> PullRequests { get; set; } = [];
    }
}
