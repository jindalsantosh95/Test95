using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Application.GitHub;
using VAA.EAPI.Microservices.Insights.Domain.GitHub;

namespace VAA.EAPI.Microservices.Insights.Interfaces
{
    public interface IGitHubMergedPullRequestQueryHandler
    {
        Task<IReadOnlyCollection<GitHubRepositoryMergedPullRequests>> HandleAsync(GitHubMergedPullRequestQuery query, CancellationToken cancellationToken = default);
    }
}
