using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Domain.GitHub;

namespace VAA.EAPI.Microservices.Insights.Interfaces
{
    public interface IGitHubApiService
    {
        Task<IReadOnlyCollection<GitHubRepositoryMergedPullRequests>> GetMergedPullRequestsAsync(string personalAccessToken, string owner, DateTime startUtcInclusive, DateTime endUtcExclusive, CancellationToken cancellationToken = default);
    }
}
