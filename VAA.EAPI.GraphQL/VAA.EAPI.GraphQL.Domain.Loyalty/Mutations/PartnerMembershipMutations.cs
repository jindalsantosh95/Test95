using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class PartnerMembershipMutations
    {
        private readonly ILogger<PartnerMembershipMutations> _logger;
        private readonly ICustomerTransactionMutationProvider<PartnerMembershipDocumentResponse> _partnerMembershipMutationProvider;
        private readonly IPartnersProfilesQueryProvider _partnersProfilesQueryProvider;
        private readonly ICustomerProvider _customerProvider;

        public PartnerMembershipMutations(ICustomerTransactionMutationProvider<PartnerMembershipDocumentResponse> partnerMembershipMutationProvider, ICustomerProvider customerProvider, IPartnersProfilesQueryProvider partnersProfilesQueryProvider, ILogger<PartnerMembershipMutations> logger)
        {
            _partnerMembershipMutationProvider = partnerMembershipMutationProvider ?? throw new ArgumentNullException(nameof(partnerMembershipMutationProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _partnersProfilesQueryProvider = partnersProfilesQueryProvider ?? throw new ArgumentNullException(nameof(partnersProfilesQueryProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = new[] { "Loyalty.PartnerMembership.Create" })]
        [GraphQLName("loyaltyCreatePartnerMembership")]
        [GraphQLDescription("Creates a Partner Membership")]
        public async Task<PartnerMembership> CreatePartnerMembership(PartnerMembershipMutationRequest partnerMembershipRequest)
        {
            if (partnerMembershipRequest is null)
                throw new RequestValidationException("Partner membership request parameters must be specified");

            if (!Enum.IsDefined(typeof(CardTypeEnum), partnerMembershipRequest.CardType))
                throw new RequestValidationException("Valid 'Card Type' must be specified");

            partnerMembershipRequest.RequestType = HttpMethod.Post;
            return await MutatePMDoc(partnerMembershipRequest);
        }

        [Authorize(Roles = new[] { "Loyalty.PartnerMembership.Update" })]
        [GraphQLName("loyaltyUpdatePartnerMembership")]
        [GraphQLDescription("Updates a Partner Membership")]
        public async Task<PartnerMembership> UpdatePartnerMembership(PartnerMembershipMutationRequest partnerMembershipRequest)
        {
            if (partnerMembershipRequest is null)
                throw new RequestValidationException("Partner membership request parameters must be specified");

            partnerMembershipRequest.RequestType = HttpMethod.Patch;
            return await MutatePMDoc(partnerMembershipRequest);
        }

        private async Task<PartnerMembership> MutatePMDoc(PartnerMembershipMutationRequest partnerMembershipRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(partnerMembershipRequest.CustomerId) && string.IsNullOrEmpty(partnerMembershipRequest.MemberNumber))
                    throw new RequestValidationException("Either 'Customer ID' or 'Member Number' must be specified");

                if (string.IsNullOrEmpty(partnerMembershipRequest.CustomerId))
                    partnerMembershipRequest.CustomerId = (await _customerProvider.GetCustomer(partnerMembershipRequest.CustomerId, partnerMembershipRequest.MemberNumber)).Customer.CustomerId;

                if (string.IsNullOrEmpty(partnerMembershipRequest.MemberNumber))
                    partnerMembershipRequest.MemberNumber = (await _customerProvider.GetCustomer(partnerMembershipRequest.CustomerId, partnerMembershipRequest.MemberNumber)).Customer.LoyaltyNumber;

                var validationResult = new PartnerMembershipValidator(partnerMembershipRequest.RequestType).Validate(partnerMembershipRequest);

                if (!validationResult.IsValid)
                    throw new RequestValidationException("Partner Membership mutation request validation failure.", validationResult.Errors);

                partnerMembershipRequest.PartnerId = partnerMembershipRequest.RequestType == HttpMethod.Post ? (await _partnersProfilesQueryProvider.GetPartnerProfileDetails()).FirstOrDefault(x => x.PartnerCode == partnerMembershipRequest.PartnerCode)?.RowId : null;

                // fire and forget;
                var response = await _partnerMembershipMutationProvider.Mutate((PartnerMembershipRequest)partnerMembershipRequest);

                // If there were any errors, then report them to consumer, 
                if (response.Errors is not null)
                {
                    var builder = ErrorBuilder.New().SetMessage("Partner Membership mutation request failed.");

                    foreach (var err in response.Errors)
                    {
                        builder.SetExtension("code", err.Code);
                        builder.SetExtension("message", err.Message);
                        builder.SetExtension("source", err.ErrorSource);
                    }

                    throw new GraphQLException(builder.Build());
                }

                return response.PartnerMembershipDocument;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke PartnerMembership");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in PartnerMembership mutation");
                throw;
            }

        }
    }
}
