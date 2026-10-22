using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class TargetedMembershipMutation
    {
        private readonly ITargetedMembershipMutationProvider<TargetedMembershipMutationResponse> _tmDocProvider;
        private readonly ICustomerProvider _customerService;
        private readonly ILogger<TargetedMembershipMutation> _logger;

        public TargetedMembershipMutation(ITargetedMembershipMutationProvider<TargetedMembershipMutationResponse> tmDocProvider, ICustomerProvider customerService, ILogger<TargetedMembershipMutation> logger)
        {
            _tmDocProvider = tmDocProvider ?? throw new ArgumentNullException(nameof(tmDocProvider));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger;
        }

        [GraphQLName("loyaltyCreateTargetedMembershipDocument")]
        [Authorize(Roles = new[] { "TargetedMembership.Create" })]
        [GraphQLDescription("Create an new Targeted Membership document for the customer")]

        public async Task<TargetedMembership> CreateTargetedMembershipDocument(string customerId, string memberId, TargetedMembership tmDoc)
        {
            return await MutateTMDoc(customerId, memberId, tmDoc, HttpMethod.Post);

        }

        [GraphQLName("loyaltyUpdateTargetedMembershipDocument")]
        [Authorize(Roles = new[] { "TargetedMembership.Update" })]
        [GraphQLDescription("Update an existing Targeted Membership document for the customer")]

        public async Task<TargetedMembership> UpdateTargetedMembershipDocument(string customerId, string memberId, TargetedMembership tmDoc)
        {
            return await MutateTMDoc(customerId, memberId, tmDoc, HttpMethod.Patch);
        }

        [GraphQLName("loyaltyDeleteTargetedMembershipDocument")]
        [Authorize(Roles = new[] { "TargetedMembership.Delete" })]
        [GraphQLDescription("Delete an existing Targeted Membership document for the customer")]

        public async Task<TargetedMembership> DeleteTargetedMembershipDocument(string customerId, string memberId, TargetedMembership tmDoc)
        {
            return await MutateTMDoc(customerId, memberId, tmDoc, HttpMethod.Delete);
        }

        private async Task<TargetedMembership> MutateTMDoc(string customerId, string memberId, TargetedMembership tmDoc, HttpMethod verb)
        {
            if (customerId is null && memberId is null)
                throw new RequestValidationException($"Either '{nameof(customerId)}' or {nameof(memberId)} must be specified.");

            if (tmDoc is null)
                throw new ArgumentNullException(nameof(tmDoc));


            var validationResult = new TargetedMembershipValidator(verb).Validate(tmDoc);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Targeted Membership mutation request validation failure.", validationResult.Errors);

            // If I don't have a customer ID, go get the member based on member ID
            if (string.IsNullOrEmpty(customerId))
                customerId = (await _customerService.GetCustomer(customerId, memberId)).Customer.CustomerId;

            TargetedMembershipMutationRequest req = new()
            {
                CustomerId = customerId,
                TargetedMembershipDocument = tmDoc,
                RequestType = verb
            };

            var outcome = await _tmDocProvider.Mutate(req);

            // If there were any errors, then report them to consumer, 
            if (outcome.Errors is not null)
            {
                var builder = ErrorBuilder.New()
                                          .SetMessage("Targeted Membership mutation request failed.");
                outcome.Errors.ForEach(err =>
                {
                    builder.SetExtension("code", err.Code);
                    builder.SetExtension("message", err.Message);
                    builder.SetExtension("source", err.ErrorSource);
                });


                throw new GraphQLException(builder.Build());
            }

            return outcome.TargetedMembershipDocument;
        }

    }
}
