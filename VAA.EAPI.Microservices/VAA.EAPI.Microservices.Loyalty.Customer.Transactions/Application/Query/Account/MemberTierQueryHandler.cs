using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MembershipChange;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Account;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Account
{
    public class MemberTierQueryHandler : TransactionBaseQueryHandler<MemberTierResponse>, IRequestHandler<MemberTierQuery, Result<Response<MemberTierResponse>>>
    {
        public MemberTierQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<MemberTierResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<MemberTierResponse>>> Handle(MemberTierQuery request, CancellationToken cancellationToken)
        {
            // Acquire all the documents
            var flyingClubMemberDetails = _mediator.Send(new FlyingClubMemberQuery { CustomerId = request.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.FlyingClubMemberDocType) }, cancellationToken);
            var memberChangeDetails = _mediator.Send(new MembershipChangeQuery { CustomerId = request.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.MembershipChangeDocType) }, cancellationToken);
            var transactionDetails = _mediator.Send(new TransactionQuery { CustomerId = request.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.TransactionDocType) }, cancellationToken);
            var awardsDetails = _mediator.Send(new AwardsQuery { CustomerId = request.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.AwardDocType) }, cancellationToken);

            await Task.WhenAll(flyingClubMemberDetails, memberChangeDetails, transactionDetails, awardsDetails);

            // Calculate all the points           
            int totalFlownMiles = 0;
            int totalTierPoints = 0;
            int upgradePoints = 0;
            int totalRetainPoints = 0;
            int rollingTierPoints = 0;

            // Verify all memberstatus
            bool wingsMember;
            bool lifeTimeGoldMember;
            bool millionMiler;
            bool twoMillionMiler;

            IEnumerable<MemberAttribute> membershipAttributeFlown = flyingClubMemberDetails.Result.Value.Value.FlyingClubMemberDocuments.First().MemberAttributes.Where(w => w.AttributeName.Is(Constants.Application.FlownMiles));

            if (membershipAttributeFlown.Any())
            {
                totalFlownMiles = Convert.ToInt32(membershipAttributeFlown.First().Value);
            }

            DateTime tenYearBack = DateTime.UtcNow.AddYears(-10);

            var roundedDate = new DateTime(tenYearBack.Year, tenYearBack.Month, 1);
            DateTime lastYear = DateTime.UtcNow.AddYears(-1);
            DateTime tierStartDate = Convert.ToDateTime(memberChangeDetails.Result.Value.Value.MembershipChangeDocuments?.FirstOrDefault(x => x.Active == "Y").StartDate);
            DateTime tierEndDate = Convert.ToDateTime(memberChangeDetails.Result.Value.Value.MembershipChangeDocuments?.FirstOrDefault(x => x.Active == "Y").EndDate);

            DateTime validateDate = new(lastYear.Year, lastYear.Month, 1);

            if (memberChangeDetails.Result.Value.Value.MembershipChangeDocuments.FirstOrDefault(x => x.Active == "Y").TierCode == Constants.Application.RedTier)
            {
                DateTime oneyearback = DateTime.Now.AddDays(-365);
                DateTime roundDate = new DateTime(oneyearback.Year, oneyearback.Month, 1);

                IEnumerable<TransactionDocument> transactionEntityRetain = transactionDetails.Result.Value.Value.TransactionDocuments?.Where(t => t.StatusCode == Constants.Application.StatusCode &&
                 (!string.IsNullOrEmpty(t.ActivityDate) && Convert.ToDateTime(t.ActivityDate) >= roundDate ||
                 !string.IsNullOrEmpty(t.FlightDetails?.DepartureDate) && Convert.ToDateTime(t.FlightDetails?.DepartureDate) >= roundDate));

                var retainCreditDtls = transactionEntityRetain.SelectMany(transFinal => transFinal.CreditDetails)
                    .Where(transFinal => transFinal.PointType == Constants.Application.TierPoints).ToList();

                totalRetainPoints += retainCreditDtls.Sum(credDtls => (int)Convert.ToDecimal(credDtls.CreditValue));
            }
            else
            {

                IEnumerable<TransactionDocument> transactionEntityRetain = transactionDetails.Result.Value.Value.TransactionDocuments?.Where(t => t.StatusCode == Constants.Application.StatusCode &&
               (!string.IsNullOrEmpty(t.ActivityDate) && Convert.ToDateTime(t.ActivityDate) >= tierStartDate && Convert.ToDateTime(t.ActivityDate) <= tierEndDate ||
               !string.IsNullOrEmpty(t.FlightDetails?.DepartureDate) && Convert.ToDateTime(t.FlightDetails?.DepartureDate) >= tierStartDate && Convert.ToDateTime(t.FlightDetails?.DepartureDate) <= tierEndDate));

                var retainCreditDtls = transactionEntityRetain.SelectMany(transFinal => transFinal?.CreditDetails)
                    .Where(transFinal => transFinal?.PointType == Constants.Application.TierPoints).ToList();

                totalRetainPoints += retainCreditDtls.Sum(tierPoints => (int)Convert.ToDecimal(tierPoints?.CreditValue));

            }

            IEnumerable<TransactionDocument> transactionEntity = transactionDetails.Result.Value.Value.TransactionDocuments.Where(t => t.StatusCode == Constants.Application.StatusCode && (!string.IsNullOrEmpty(t.ActivityDate) && Convert.ToDateTime(t.ActivityDate) >= roundedDate || !string.IsNullOrEmpty(t.FlightDetails?.DepartureDate) && Convert.ToDateTime(t.FlightDetails.DepartureDate) >= roundedDate));
            var totalTierCreditDtls = transactionEntity.SelectMany(transFinal => transFinal?.CreditDetails)
                    .Where(transFinal => transFinal?.PointType == Constants.Application.VSTierPoints).ToList();

            totalTierPoints += totalTierCreditDtls.Sum(tierPoints => (int)Convert.ToDecimal(tierPoints?.CreditValue));

            IEnumerable<TransactionDocument> transactionEntityRolling = transactionDetails.Result.Value.Value.TransactionDocuments.Where(t => (!string.IsNullOrEmpty(t.ActivityDate) && Convert.ToDateTime(t.ActivityDate) >= validateDate || !string.IsNullOrEmpty(t.FlightDetails?.DepartureDate) && Convert.ToDateTime(t.FlightDetails?.DepartureDate) >= validateDate) && t.StatusCode == Constants.Application.StatusCode);

            var upgradeCreditDtls = transactionEntityRolling.SelectMany(transFinal => transFinal?.CreditDetails)
                    .Where(transFinal => transFinal?.PointType == Constants.Application.TierPoints).ToList();

            upgradePoints += upgradeCreditDtls.Sum(tierPoints => (int)Convert.ToDecimal(tierPoints?.CreditValue));


            int pointsRequiredForUpgrade = flyingClubMemberDetails.Result.Value.Value.FlyingClubMemberDocuments.FirstOrDefault()?.TierCode switch
            {
                "Red" => Constants.Application.RedUpgradeTierPoints - upgradePoints,
                "Silver" => Constants.Application.SilverUpgradeTierPoints - upgradePoints,

                _ => 0
            };

            int pointsRequiredForRetain = flyingClubMemberDetails.Result.Value.Value.FlyingClubMemberDocuments.FirstOrDefault()?.TierCode switch
            {
                "Silver" => Constants.Application.SilverRetainTierPoints - totalRetainPoints,
                "Gold" => Constants.Application.GoldRetainTierPoints - totalRetainPoints,
                _ => 0
            };

            //Rolling Tier Points and Upgrade by Date calculation
            DateTime rollingQualifyDate = new(DateTime.Now.Year - 1, DateTime.Now.Month, 1);
            DateTime currentPlusOneYearLastDay = DateTime.Now.AddYears(1).AddMonths(1).AddDays(-DateTime.Now.AddYears(1).AddMonths(1).Day).Date;
            DateTime byDate = new();

            IEnumerable<TransactionDocument> accrualTransactions = transactionDetails.Result.Value.Value.TransactionDocuments?.Where(t => t.StatusCode == Constants.Application.StatusCode && t.TransactionType == Constants.Application.AccrualTransactionType && t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= rollingQualifyDate);

            IEnumerable<TransactionDocument> accrualTransactionSorted = accrualTransactions.OrderBy(t => Convert.ToDateTime(t.ActivityDate));

            IEnumerable<Creditdetail> rollingTierPointsCreditDtls = accrualTransactions.SelectMany(transFinal => transFinal?.CreditDetails).Where(transFinal => transFinal?.PointType == Constants.Application.TierPoints).ToList();

            TransactionDocument firstAccuralTransaction = accrualTransactionSorted.FirstOrDefault(c => c.CreditDetails.Where(_ => _.PointType == Constants.Application.TierPoints && Convert.ToDouble(_.CreditValue) > 0).FirstOrDefault() != null);

            DateTime activityDatePlusOneYear = firstAccuralTransaction != null ? Convert.ToDateTime(Convert.ToDateTime(firstAccuralTransaction?.ActivityDate).AddDays(365)) : default;

            byDate = activityDatePlusOneYear != default ? new DateTime(activityDatePlusOneYear.Year, activityDatePlusOneYear.Month, 1).AddMonths(1).AddDays(-1) : default;

            rollingTierPoints += rollingTierPointsCreditDtls.Sum(tierPoints => (int)Convert.ToDecimal(tierPoints?.CreditValue));

            //member status verification
            wingsMember = awardsDetails.Result.Value.Value.Awards.Any(x => x.ProductId == Constants.Application.WingsMemProductID && x.ExpiryDate == null);
            lifeTimeGoldMember = awardsDetails.Result.Value.Value.Awards.Any(x => x.ProductId == Constants.Application.LTGProductID && x.ExpiryDate == null);
            millionMiler = awardsDetails.Result.Value.Value.Awards.Any(x => x.ProductId == Constants.Application.MillionMilerProductID && x.ExpiryDate == null);
            twoMillionMiler = totalFlownMiles >= Constants.Application.TwoMillionMilerFlownMiles;

            return Result.Success(new Response<MemberTierResponse>
            {
                Status = Status.OK,
                Value = new()
                {
                    FlownMiles = totalFlownMiles,
                    DateTierPointsReset = memberChangeDetails.Result.Value.Value.MembershipChangeDocuments.FirstOrDefault(x => x.Active == "Y")?.EndDate ?? DateTime.MaxValue,
                    TierPointsNeedToRetain = pointsRequiredForRetain < 0 ? 0 : pointsRequiredForRetain,
                    TierPointsNeedToReachNextTier = pointsRequiredForUpgrade,
                    TierPoints = totalTierPoints,
                    RetainTierPoints = totalRetainPoints,
                    TierCode = flyingClubMemberDetails.Result.Value.Value.FlyingClubMemberDocuments.FirstOrDefault()?.TierCode,
                    RollingTierPoints = rollingTierPoints,
                    UpgradeByDate = byDate.Equals(default) ? currentPlusOneYearLastDay : byDate,
                    MemberStatus = new MemberStatus()
                    {
                        Is2MillionMiler = twoMillionMiler,
                        IsWingsMemeber = wingsMember,
                        IsLifeTimeGoldMember = lifeTimeGoldMember,
                        IsMillionMiler = millionMiler
                    }
                }
            });
        }
    }
}
