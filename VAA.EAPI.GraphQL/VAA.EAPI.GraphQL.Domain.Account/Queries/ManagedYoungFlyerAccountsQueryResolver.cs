using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManagedYoungFlyer;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;
using AccountApplication = VAA.EAPI.GraphQL.Domain.Account.Constants.Application;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    public sealed class ManagedYoungFlyerAccountsQueryResolver : ObjectType<ManagedYoungFlyerAccount>
    {
        private readonly IActivityQueryProvider _activityProvider;
        private readonly IMemberCustomAttributeProvider _memberCustomAttributeProvider;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<ManagedYoungFlyerAccountsQueryResolver> _logger;

        public ManagedYoungFlyerAccountsQueryResolver(IActivityQueryProvider activityProvider, IMemberCustomAttributeProvider memberCustomAttributeProvider, TimeProvider timeProvider, ILogger<ManagedYoungFlyerAccountsQueryResolver> logger)
        {
            _activityProvider = activityProvider ?? throw new ArgumentNullException(nameof(activityProvider));
            _memberCustomAttributeProvider = memberCustomAttributeProvider ?? throw new ArgumentNullException(nameof(memberCustomAttributeProvider));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override void Configure(IObjectTypeDescriptor<ManagedYoungFlyerAccount> descriptor)
        {
            descriptor.Field(field => field.ActivityResponse)
                .Description("Eligible activities from the last three years.")
                .Resolve(async context =>
                    await GetActivityResponse(context.Parent<ManagedYoungFlyerAccount>()));

            descriptor.Field(field => field.TenureResponse)
                .Resolve(async context =>
                    await GetTenureResponse(context.Parent<ManagedYoungFlyerAccount>()));
        }

        private async Task<ActivityResponse> GetActivityResponse(ManagedYoungFlyerAccount account)
        {
            try
            {
                if (account.Customer is null || account.MemberDetails is null)
                    throw new InvalidOperationException("Managed young flyer activity context is unavailable.");

                var activities = await _activityProvider.GetActivitiesQueryData(new TokenB2CResponse { CustomerId = account.CustomerId, FcNumber = account.FcNumber }, account.Customer, account.MemberDetails, Application.PageSizeResolver, account.ActivitiesCount);
                return new ActivityResponse { ActivityDetails = activities?.Select(activity => new ActivityDetail(activity)).ToList() ?? Enumerable.Empty<ActivityDetail>() };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to retrieve activities for a managed young flyer. CorrelationId: {CorrelationId}", AsyncContext.Current?.CorrelationId ?? Guid.Empty);
                return new ActivityResponse { ActivityDetails = Enumerable.Empty<ActivityDetail>(), ErrorResponse = CreateErrorResponse("Unable to retrieve Activities", AccountApplication.ActivityError) };
            }
        }

        private async Task<MemberCustomAttributesResponseAccount> GetTenureResponse(ManagedYoungFlyerAccount account)
        {
            try
            {
                var documents = await _memberCustomAttributeProvider.GetMemberCustomerAttributeData(account.CustomerId);
                return new MemberCustomAttributesResponseAccount { MemberCustomAttributesDocuments = MapMemberCustomAttributesDocuments(MapTenureDetails(documents)) };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to retrieve High Five details for a managed young flyer. CorrelationId: {CorrelationId}", AsyncContext.Current?.CorrelationId ?? Guid.Empty);
                return new MemberCustomAttributesResponseAccount { MemberCustomAttributesDocuments = Enumerable.Empty<MemberCustomAttributesAccount>(), ErrorResponse = CreateErrorResponse("Unable to retrieve High Five details", AccountApplication.MemberCustomAttributesError) };
            }
        }

        private IEnumerable<MemberCustomAttributesAccount> MapTenureDetails(IEnumerable<MemberCustomAttributesAccount> documents)
        {
            var currentYear = _timeProvider.GetUtcNow().Year.ToString();
            var mappedDocuments = (documents ?? Enumerable.Empty<MemberCustomAttributesAccount>()).ToList();

            foreach (var document in mappedDocuments)
            {
                document.MemberLastFiveVSActivityYear = (document.MemberLastFiveVSActivityYear ?? Enumerable.Empty<MemberLastFiveVSActivityYearAccount>())
                    .OrderBy(year => int.TryParse(year.ActivityYear, out var value) ? value : int.MaxValue)
                    .ToList();
                document.IsAnActiveYear = document.MemberLastFiveVSActivityYear
                    .OrderByDescending(year => year.ActivityYear)
                    .FirstOrDefault() is { } last
                    && last.ActivityYear == currentYear
                    && !last.BenefitGiven.IsNullOrWhiteSpace()
                    && last.BenefitGiven.Is("true");
            }

            return mappedDocuments;
        }

        private static ErrorResponse CreateErrorResponse(string message, string code) => new()
        {
            message = message,
            extentions = new Extentions { exception = "An internal dependency error occurred.", code = code }
        };

        private static IEnumerable<MemberCustomAttributesAccount> MapMemberCustomAttributesDocuments(IEnumerable<MemberCustomAttributesAccount> documents) =>
            (documents ?? Enumerable.Empty<MemberCustomAttributesAccount>())
                .Select(document => new MemberCustomAttributesAccount
                {
                    ActiveYearCounterForTenureBenefits = document.ActiveYearCounterForTenureBenefits,
                    TotalActiveYear = document.TotalActiveYear,
                    IsAnActiveYear = document.IsAnActiveYear,
                    MemberLastFiveVSActivityYear = (document.MemberLastFiveVSActivityYear ?? Enumerable.Empty<MemberLastFiveVSActivityYearAccount>())
                        .Select(year => new MemberLastFiveVSActivityYearAccount
                        {
                            ActivityYear = year.ActivityYear,
                            BenefitGiven = year.BenefitGiven
                        })
                        .ToList()
                })
                .ToList();

    }
}
