using System;

namespace VAA.EAPI.Microservices.Insights.Application.GitHub
{
    public sealed class GitHubMergedPullRequestQuery
    {
        public string PersonalAccessToken { get; set; } = string.Empty;

        public string Owner { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }
    }
}
