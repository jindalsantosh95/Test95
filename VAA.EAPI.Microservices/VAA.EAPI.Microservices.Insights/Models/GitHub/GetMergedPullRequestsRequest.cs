using System;
using System.Globalization;
using VAA.EAPI.Microservices.Insights.Application.GitHub;

namespace VAA.EAPI.Microservices.Insights.Models.GitHub
{
    public sealed class GetMergedPullRequestsRequest
    {
        public string PersonalAccessToken { get; set; } = string.Empty;

        public string Owner { get; set; } = string.Empty;

        public string StartDate { get; set; } = string.Empty;

        public string EndDate { get; set; } = string.Empty;

        public static explicit operator GitHubMergedPullRequestQuery(GetMergedPullRequestsRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!DateOnly.TryParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
            {
                throw new FormatException("StartDate must be a valid date string (expected format: yyyy-MM-dd).");
            }

            if (!DateOnly.TryParseExact(request.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            {
                throw new FormatException("EndDate must be a valid date string (expected format: yyyy-MM-dd).");
            }

            return new GitHubMergedPullRequestQuery
            {
                PersonalAccessToken = request.PersonalAccessToken,
                Owner = request.Owner,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}
