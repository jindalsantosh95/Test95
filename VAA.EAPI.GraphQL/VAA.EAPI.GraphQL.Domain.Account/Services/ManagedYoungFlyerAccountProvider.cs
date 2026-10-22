using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManagedYoungFlyer;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public sealed class ManagedYoungFlyerAccountProvider : IManagedYoungFlyerAccountProvider
    {
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _customerTransactionProvider;
        private readonly ICustomerTransactionQueryProvider<CustomerTransactionDocument> _tierDetailsProvider;
        private readonly IManagedYoungFlyerRelationshipProvider _relationshipProvider;
        private readonly ILogger<ManagedYoungFlyerAccountProvider> _logger;

        public ManagedYoungFlyerAccountProvider(IAccountCustomerService customerProvider, IManagedYoungFlyerRelationshipProvider relationshipProvider, ICustomerTransactionQueryProvider<FcMemberDetails> customerTransactionProvider, ICustomerTransactionQueryProvider<CustomerTransactionDocument> tierDetailsProvider, ILogger<ManagedYoungFlyerAccountProvider> logger)
        {
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _relationshipProvider = relationshipProvider ?? throw new ArgumentNullException(nameof(relationshipProvider));
            _customerTransactionProvider = customerTransactionProvider ?? throw new ArgumentNullException(nameof(customerTransactionProvider));
            _tierDetailsProvider = tierDetailsProvider ?? throw new ArgumentNullException(nameof(tierDetailsProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ManagedYoungFlyerAccountsResponse> GetAsync(string parentCustomerId, string parentFcNumber, int activitiesCount, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (parentCustomerId.IsNullOrWhiteSpace())
                throw new ArgumentException("The authenticated parent customer ID is required.");

            var youngFlyerIds = await _relationshipProvider.GetManagedYoungFlyersCustomerIdsAsync(parentCustomerId);

            var youngFlyers = new ConcurrentBag<ManagedYoungFlyerAccount>();

            await Parallel.ForEachAsync(youngFlyerIds, new ParallelOptions
            {
                CancellationToken = cancellationToken
            }, async (youngFlyerId, childCancellationToken) =>
            {
                childCancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var child = await _customerProvider.GetCustomer(youngFlyerId, string.Empty);

                    if (child?.Customer is null)
                    {
                        return;
                    }

                    FcMemberDetails memberDetails = null;
                    try
                    {
                        memberDetails = await _customerTransactionProvider.CustomerTransactionQuery($"{ConfigReader.Get(Environment.CustTransactionMsUrl)}{Application.GetFcMemberDetails}", child.Customer.CustomerId);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Unable to retrieve Flying Club details for a managed young flyer. CorrelationId: {CorrelationId}", AsyncContext.Current?.CorrelationId ?? Guid.Empty);
                    }

                    CustomerTransactionDocument tierDetails = null;
                    try
                    {
                        tierDetails = await _tierDetailsProvider.CustomerTransactionQuery($"{ConfigReader.Get(Environment.CustTransactionMsUrl).TrimEnd('/')}/{child.Customer.CustomerId}/tierDetails", child.Customer.CustomerId);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Unable to retrieve tier details for a managed young flyer. CorrelationId: {CorrelationId}", AsyncContext.Current?.CorrelationId ?? Guid.Empty);
                    }

                    childCancellationToken.ThrowIfCancellationRequested();
                    youngFlyers.Add(Map(child.Customer, memberDetails, tierDetails, activitiesCount));
                }
                catch (OperationCanceledException) when (childCancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to retrieve a managed young flyer. CorrelationId: {CorrelationId}", AsyncContext.Current?.CorrelationId ?? Guid.Empty);
                }
            });

            return new ManagedYoungFlyerAccountsResponse { ParentCustomerId = parentCustomerId, ParentFcNumber = parentFcNumber, YoungFlyers = youngFlyers };
        }

        private ManagedYoungFlyerAccount Map(CustomerCustMS child, FcMemberDetails memberDetails, CustomerTransactionDocument tierDetails, int activitiesCount)
        {
            var email = child?.ContactDetails?.EmailDetails?.FirstOrDefault(x => x.PreferredFlag == "Y")?.emailId;
            DateTime? dob = child?.DateOfBirth == default ? null : DateTime.SpecifyKind(child.DateOfBirth, DateTimeKind.Utc);
            var flyingClubMember = memberDetails?.FlyingClubMemberDocuments?.FirstOrDefault();

            return new ManagedYoungFlyerAccount
            {
                CustomerId = child?.CustomerId,
                FcNumber = child?.LoyaltyNumber,
                FirstName = child?.FirstName,
                LastName = child?.LastName,
                DateOfBirth = dob,
                FCJoinDate = flyingClubMember?.EnrollmentDate == default ? null : DateTime.SpecifyKind(flyingClubMember.EnrollmentDate, DateTimeKind.Utc),
                TierPoints = tierDetails?.TierPoints,
                TierCode = tierDetails?.TierCode,
                DateTierPointsReset = tierDetails?.DateTierPointsReset == default ? null : DateTime.SpecifyKind(tierDetails.DateTierPointsReset, DateTimeKind.Utc),
                UpgradeByDate = tierDetails?.UpgradeByDate == default ? null : DateTime.SpecifyKind(tierDetails.UpgradeByDate, DateTimeKind.Utc),
                TierPointsNeedToReachNextTier = tierDetails?.TierPointsNeedToReachNextTier,
                RetainTierPoints = tierDetails?.RetainTierPoints,
                TierPointsNeedToRetain = tierDetails?.TierPointsNeedToRetain,
                RollingTierPoints = tierDetails?.RollingTierPoints,
                Status = child?.Status,
                EmailAddress = email,
                HoHManaged = true,
                ActivitiesCount = activitiesCount,
                Customer = child,
                MemberDetails = memberDetails
            };
        }
    }
}
