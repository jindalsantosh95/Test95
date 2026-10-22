using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Insights.Domain.GitHub;

namespace VAA.EAPI.Microservices.Insights.Models.GitHub
{
    public sealed class GetMergedPullRequestsResponse
    {
        public string Repository { get; set; } = string.Empty;

        public string DefaultBranch { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;

        public IReadOnlyCollection<GetMergedPullRequestItemResponse> PullRequests { get; set; } = [];

        public static explicit operator GetMergedPullRequestsResponse(GitHubRepositoryMergedPullRequests source)
        {
            return new GetMergedPullRequestsResponse
            {
                Repository = source.RepositoryFullName,
                DefaultBranch = source.DefaultBranch,
                Error = source.Error,
                PullRequests = source.PullRequests
                    .Select(pullRequest => (GetMergedPullRequestItemResponse)pullRequest)
                    .ToList()
            };
        }
    }
}
