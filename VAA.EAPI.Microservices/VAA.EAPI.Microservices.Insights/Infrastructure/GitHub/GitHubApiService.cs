using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Insights.Domain.GitHub;
using VAA.EAPI.Microservices.Insights.Infrastructure.GitHub.Models;
using VAA.EAPI.Microservices.Insights.Interfaces;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.GitHub
{
    public sealed class GitHubApiService(IHttpConnector httpConnector, ILogger<GitHubApiService> logger) : IGitHubApiService
    {
        private const string GitHubApiBaseUrl = "https://api.github.com";
        private const int PageSize = 100;
        private const string DefaultBranchFallback = "main";

        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly ILogger<GitHubApiService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<IReadOnlyCollection<GitHubRepositoryMergedPullRequests>> GetMergedPullRequestsAsync(string personalAccessToken, string owner, DateTime startUtcInclusive, DateTime endUtcExclusive, CancellationToken cancellationToken = default)
        {
            var repositories = await GetRepositoriesAsync(personalAccessToken, owner);
            var repositoryPullRequests = new List<GitHubRepositoryMergedPullRequests>();
            var tasks = new List<Task<GitHubRepositoryMergedPullRequests>>();
            using (var semaphore = new SemaphoreSlim(10)) // Limit to 10 concurrent tasks
            {
                foreach (var repository in repositories)
                {
                    tasks.Add(ProcessRepositoryAsync(repository, personalAccessToken, startUtcInclusive, endUtcExclusive, cancellationToken, semaphore));
                }
                var results = await Task.WhenAll(tasks);
                repositoryPullRequests.AddRange(results);
            }

            return repositoryPullRequests
                .OrderBy(repository => repository.RepositoryFullName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<GitHubRepositoryMergedPullRequests> ProcessRepositoryAsync(GitHubRepositoryResponse repository, string personalAccessToken, DateTime startUtcInclusive, DateTime endUtcExclusive, CancellationToken cancellationToken, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var defaultBranch = string.IsNullOrWhiteSpace(repository.DefaultBranch)
                    ? DefaultBranchFallback
                    : repository.DefaultBranch;

                var pullRequests = await GetRepositoryPullRequestsAsync(repository.FullName, defaultBranch, personalAccessToken, startUtcInclusive, endUtcExclusive, cancellationToken);

                return new GitHubRepositoryMergedPullRequests
                {
                    RepositoryFullName = repository.FullName,
                    DefaultBranch = defaultBranch,
                    PullRequests = pullRequests
                        .OrderByDescending(pullRequest => pullRequest.MergedAtUtc)
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing repository {FullName}.", repository.FullName);
                return new GitHubRepositoryMergedPullRequests
                {
                    RepositoryFullName = repository.FullName,
                    DefaultBranch = string.IsNullOrWhiteSpace(repository.DefaultBranch) ? DefaultBranchFallback : repository.DefaultBranch,
                    Error = ex.Message
                };
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<IReadOnlyCollection<GitHubRepositoryResponse>> GetRepositoriesAsync(string personalAccessToken, string owner)
        {
            var repositories = new List<GitHubRepositoryResponse>();
            var page = 1;

            while (true)
            {
                var uri = $"{GitHubApiBaseUrl}/user/repos?visibility=all&affiliation=owner,collaborator,organization_member&per_page={PageSize}&page={page}";

                var pageResponse = await _httpConnector.GetAsync<List<GitHubRepositoryResponse>>(uri, BuildHeaders(), personalAccessToken) ?? new List<GitHubRepositoryResponse>();

                if (pageResponse.Count == 0)
                {
                    break;
                }

                repositories.AddRange(pageResponse
                    .Where(repo => !string.IsNullOrWhiteSpace(repo.FullName))
                    .Where(repo => repo.FullName.StartsWith($"{owner}/", StringComparison.OrdinalIgnoreCase)));
                page++;
            }

            return repositories;
        }

        private async Task<IReadOnlyCollection<GitHubMergedPullRequest>> GetRepositoryPullRequestsAsync(string repositoryFullName, string defaultBranch, string personalAccessToken, DateTime startUtcInclusive, DateTime endUtcExclusive, CancellationToken cancellationToken)
        {
            var results = new List<GitHubMergedPullRequest>();
            var page = 1;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var uri = $"{GitHubApiBaseUrl}/repos/{repositoryFullName}/pulls?state=closed&base={Uri.EscapeDataString(defaultBranch)}&sort=updated&direction=desc&per_page={PageSize}&page={page}";
                var pullRequests = await _httpConnector.GetAsync<List<GitHubPullRequestResponse>>(uri, BuildHeaders(), personalAccessToken) ?? new List<GitHubPullRequestResponse>();

                if (pullRequests.Count == 0)
                {
                    break;
                }

                var mergedPullRequests = pullRequests
                    .Where(pr => pr.MergedAt.HasValue)
                    .Where(pr => string.Equals(pr.Base?.Ref, defaultBranch, StringComparison.OrdinalIgnoreCase))
                    .Where(pr => pr.MergedAt!.Value >= startUtcInclusive && pr.MergedAt.Value < endUtcExclusive)
                    .Select(pr =>
                    {
                        pr.RepositoryFullName = repositoryFullName;
                        return (GitHubMergedPullRequest)pr;
                    });

                results.AddRange(mergedPullRequests);
                page++;
            }

            return results;
        }

        private static Dictionary<string, string> BuildHeaders()
        {
            return new Dictionary<string, string>
            {
                { "Accept", "application/vnd.github+json" },
                { "X-GitHub-Api-Version", "2022-11-28" },
                { "User-Agent", "VAA.EAPI.Microservices.Insights" }
            };
        }
    }
}
