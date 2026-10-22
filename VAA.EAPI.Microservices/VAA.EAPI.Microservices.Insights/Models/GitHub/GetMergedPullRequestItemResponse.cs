using System;
using VAA.EAPI.Microservices.Insights.Domain.GitHub;

namespace VAA.EAPI.Microservices.Insights.Models.GitHub
{
    public sealed class GetMergedPullRequestItemResponse
    {
        public int PullRequestNumber { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public DateTime MergedAtUtc { get; set; }

        public string MergedBy { get; set; } = string.Empty;

        public static explicit operator GetMergedPullRequestItemResponse(GitHubMergedPullRequest source)
        {
            return new GetMergedPullRequestItemResponse
            {
                PullRequestNumber = source.Number,
                Title = source.Title,
                Url = source.Url,
                MergedAtUtc = source.MergedAtUtc,
                MergedBy = source.MergedBy
            };
        }
    }
}
