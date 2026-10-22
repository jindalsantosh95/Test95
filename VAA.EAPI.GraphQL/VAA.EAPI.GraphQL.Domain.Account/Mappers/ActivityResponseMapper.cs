using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Domain.Account.Services;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class ActivityResponseMapper
    {
        public static IEnumerable<T> Mapper<T>(ILogger<ActivityQueryProvider> logger, IEnumerable<Statement> vredStatements, IEnumerable<ActivityStatementDetail> loopsStatements, int page, int pageSize, TimeProvider timeProvider)
        {

            List<ActivityDetail> activities = new();

            if (loopsStatements != null && loopsStatements.Any())
            {
                activities.AddRange(loopsStatements.Select(activityDetail => new ActivityDetail
                {
                    ActivityDate = DateTime.Parse(Convert.ToString(activityDetail?.ActivityDate)),
                    TransactionProcessedDate = DateTime.Parse(Convert.ToString(activityDetail?.TransactionProcessedDate)),
                    PartnerName = activityDetail?.PartnerName,
                    Statement = activityDetail?.StatementDescription,
                    VirginPoints = "0",
                    TierPoints = activityDetail.Point.TierPoints ?? "0",
                    PartnerCode = activityDetail?.PartnerCode,
                    TransactionId = activityDetail?.TransactionId,
                    TransactionType = activityDetail.TransactionType
                }));
            }

            if (vredStatements != null && vredStatements.Any())
            {
                foreach (Statement stmt in vredStatements)
                {
                    var transactionType = String.Equals(stmt.Type, Common.Constants.Application.VredDebitTransactionType, StringComparison.OrdinalIgnoreCase) ? Common.Constants.Application.RedemptionTransactionType : Common.Constants.Application.AccrualTransactionType;
                    if (!activities.Exists(item => item.TransactionId == stmt.VredDetail.TransactionId))
                    {
                        activities.Add(new ActivityDetail
                        {
                            ActivityDate = DateTime.Parse(Convert.ToString(stmt?.ActivityDate)),
                            TransactionProcessedDate = DateTime.Parse(Convert.ToString(stmt?.TransactionDate)),
                            PartnerName = stmt?.VredDetail.PartnerName,
                            Statement = stmt?.Description,
                            VirginPoints = GetVirginPoint(stmt?.PointsInt, transactionType),
                            TierPoints = "0",
                            PartnerCode = stmt?.VredDetail?.PartnerCode,
                            TransactionId = stmt?.VredDetail?.TransactionId,
                            TransactionType = transactionType
                        });
                    }
                    else
                    {
                        activities.FirstOrDefault(x => x.TransactionId == stmt.VredDetail.TransactionId).VirginPoints = GetVirginPoint(stmt?.PointsInt, transactionType);
                    }
                }
            }

            if (activities.Count == 0)
                return null;

            var filterActivities = activities.Any() ? activities.Where(x => !string.IsNullOrEmpty(x.PartnerCode) &&
                !string.IsNullOrEmpty(x.PartnerName) && !string.IsNullOrEmpty(x.Statement) &&
                !string.IsNullOrEmpty(x.TransactionId) && !string.IsNullOrEmpty(x.TransactionType) &&
                x.TransactionProcessedDate != DateTime.MinValue && x.ActivityDate != DateTime.MinValue) : null;

            if (activities.Count() != filterActivities.Count())
            {
                logger.LogInformation("{SuppressedActivityCount} activity records were suppressed due to incomplete data", activities.Count() - filterActivities.Count());
            }
            var cutoff = timeProvider.GetUtcNow().UtcDateTime.AddYears(-3);
            return (IEnumerable<T>)(filterActivities.Any() ? filterActivities.Where(x => x.TransactionProcessedDate >= cutoff).OrderByDescending(x => x.TransactionProcessedDate).Skip((page - 1) * pageSize).Take(pageSize) : null);
        }

        private static string GetVirginPoint(string totalPoints, string transactionType)
        {
            if (!string.IsNullOrEmpty(totalPoints))
            {
                if (transactionType == "Redemption" && !totalPoints.Contains('-'))
                {
                    return $"-{totalPoints}";
                }
                else if (transactionType == "Accrual" && !totalPoints.Contains('+') && (Convert.ToInt64(double.Parse(totalPoints)) > 0))
                {
                    return $"+{totalPoints}";
                }
                return totalPoints;
            }
            return "0";
        }
    }
}
