using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Mappers;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.StampFCNumToPNR;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class UpgradeVACMemberMutation
    {
        private readonly ILogger<UpgradeVACMemberMutation> _logger;
        private readonly ICustomerProvider _customerProvider;
        private readonly ICustomerMutationProvider _customerMutationProvider;
        private readonly IReferenceDataMutationProvider _referenceDataMutationProvider;
        private readonly IEligiblePNRsQueryProvider _eligiblePNRsQueryProvider;
        private readonly IStampFCNumToPNRMutationsProvider _stampFCNumToPNRMutationsProvider;

        public UpgradeVACMemberMutation(ICustomerProvider customerProvider, ICustomerMutationProvider customerMutationProvider, ILogger<UpgradeVACMemberMutation> logger, IReferenceDataMutationProvider referenceDataMutationProvider, IEligiblePNRsQueryProvider eligiblePNRsQueryProvider, IStampFCNumToPNRMutationsProvider stampFCNumToPNRMutationsProvider)
        {
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerMutationProvider = customerMutationProvider ?? throw new ArgumentNullException(nameof(customerMutationProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _referenceDataMutationProvider = referenceDataMutationProvider ?? throw new ArgumentNullException(nameof(referenceDataMutationProvider));
            _eligiblePNRsQueryProvider = eligiblePNRsQueryProvider ?? throw new ArgumentNullException(nameof(eligiblePNRsQueryProvider));
            _stampFCNumToPNRMutationsProvider = stampFCNumToPNRMutationsProvider ?? throw new ArgumentNullException(nameof(stampFCNumToPNRMutationsProvider));
        }

        [Authorize(Roles = new[] { "Loyalty.VACMember.Upgrade" })]
        [GraphQLName("loyaltyUpgradeVACMember")]
        [GraphQLDescription("Upgrade VAC Member")]
        public async Task<string> UpgradeVACMember(UpgradeVACMemberRequest upgradeVACMemberRequest)
        {
            try
            {
                var validationResult = new UpgradeVACMemberValidator(HttpMethod.Post).Validate(upgradeVACMemberRequest);

                if (!validationResult.IsValid)
                    throw new RequestValidationException("Upgrade VAC members mutation request validation failure.", validationResult.Errors);

                if (string.IsNullOrEmpty(upgradeVACMemberRequest.CustomerId) && string.IsNullOrEmpty(upgradeVACMemberRequest.MemberNumber))
                    throw new RequestValidationException("Either 'Customer ID' or 'Member Number' must be specified");

                if (string.IsNullOrEmpty(upgradeVACMemberRequest.CustomerId))
                    upgradeVACMemberRequest.CustomerId = (await _customerProvider.GetCustomer(upgradeVACMemberRequest.CustomerId, upgradeVACMemberRequest.MemberNumber)).Customer.CustomerId;

                if (string.IsNullOrEmpty(upgradeVACMemberRequest.MemberNumber))
                    upgradeVACMemberRequest.MemberNumber = (await _customerProvider.GetCustomer(upgradeVACMemberRequest.CustomerId, upgradeVACMemberRequest.MemberNumber)).Customer.LoyaltyNumber;

                foreach (var address in upgradeVACMemberRequest.CustomerDetails.AddressDetails)
                {
                    CountryDetails countryDetails = _referenceDataMutationProvider.GetCountryDetails()?.Result.FirstOrDefault(x => x.Name == address.Country);
                    address.MetroArea = countryDetails?.MetroArea ?? string.Empty;
                    address.CountryName = countryDetails?.Value ?? string.Empty;
                }

                UpgradeMemberMSRequest customer = UpgradeMemberMapper.MapToUpgradeVACMemberRequest(upgradeVACMemberRequest);

                string upgradeResponse = await _customerMutationProvider.UpgradeVACMember(customer);

                upgradeVACMemberRequest.FCNumAutoStampingConsent = true;
                string autostampResponse = Constants.Application.AutostampPnrNotAvailable;

                if (upgradeResponse.Is(Constants.Application.UpgradeSuccessMessage))
                {
                    try
                    {
                        if ((bool)upgradeVACMemberRequest.FCNumAutoStampingConsent)
                        {
                            IEnumerable<FCNumToPNRDetails> eligiblePnrs = await _eligiblePNRsQueryProvider.GetEligiblePNRs(new GetEligiblePNRsRequest { MemberNumber = upgradeVACMemberRequest.MemberNumber, CustomerId = upgradeVACMemberRequest.CustomerId, CustomerDetails = upgradeVACMemberRequest.CustomerDetails });
                            if (eligiblePnrs.Any())
                                autostampResponse = (await _stampFCNumToPNRMutationsProvider.StampingFCNumberToPNR(new StampFCNumToPNRRequest { FCStampingDetails = eligiblePnrs })).Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception in FC number autostamping process");

                        return upgradeResponse + " & " + Constants.Application.AutostampFailureMessage;
                    }

                    return upgradeResponse + " & " + autostampResponse;
                }

                return upgradeResponse;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in Upgrade VAC members mutation");

                throw;
            }
        }
    }
}

