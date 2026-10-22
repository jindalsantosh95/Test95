using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Domain.GitHub;
using VAA.EAPI.Microservices.Insights.Interfaces;

namespace VAA.EAPI.Microservices.Insights.Application.GitHub
{
    public sealed class GitHubMergedPullRequestQueryHandler(IGitHubApiService gitHubApiService) : IGitHubMergedPullRequestQueryHandler
    {
        private readonly IGitHubApiService _gitHubApiService = gitHubApiService ?? throw new ArgumentNullException(nameof(gitHubApiService));

        public async Task<IReadOnlyCollection<GitHubRepositoryMergedPullRequests>> HandleAsync(GitHubMergedPullRequestQuery query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (string.IsNullOrWhiteSpace(query.PersonalAccessToken))
            {
                throw new ArgumentException("A GitHub personal access token is required.", nameof(query.PersonalAccessToken));
            }

            if (string.IsNullOrWhiteSpace(query.Owner))
            {
                throw new ArgumentException("A GitHub owner is required.", nameof(query.Owner));
            }

            if (query.EndDate < query.StartDate)
            {
                throw new ArgumentException("EndDate must be equal to or greater than StartDate.");
            }

            var startUtcInclusive = query.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endUtcExclusive = query.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            return await _gitHubApiService.GetMergedPullRequestsAsync(query.PersonalAccessToken, query.Owner, startUtcInclusive, endUtcExclusive, cancellationToken);
        }
    }
}
