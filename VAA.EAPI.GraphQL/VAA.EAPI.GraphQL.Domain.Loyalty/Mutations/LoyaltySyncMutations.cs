using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.SFPD;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using SFPDSyncResponse = VAA.EAPI.GraphQL.Domain.Loyalty.Models.SFPD.SFPDSyncResponse;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class LoyaltySyncMutations
    {
        private readonly ILogger<LoyaltySyncMutations> _logger;
        private readonly ILoyaltySyncMutationProvider<SFPDSyncResponse> _loyaltySyncMutationProvider;
        private readonly ICustomerProvider _customerProvider;

        public LoyaltySyncMutations(ILoyaltySyncMutationProvider<SFPDSyncResponse> loyaltySyncMutationProvider, ICustomerProvider customerProvider, ILogger<LoyaltySyncMutations> logger)
        {
            _loyaltySyncMutationProvider = loyaltySyncMutationProvider ?? throw new ArgumentNullException(nameof(loyaltySyncMutationProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = new[] { "Loyalty.SFPD.Sync" })]
        [GraphQLName("loyaltySyncSFPD")]
        [GraphQLDescription("Updates a Secure Flight Passenger Detail")]
        public async Task<SFPDSyncResponse> SyncSFPD(SFPDSyncMutationRequest passengerDetailRequest)
        {
            try
            {
                var validationResult = new LoyaltySyncSFPDValidator().Validate(passengerDetailRequest);

                if (!validationResult.IsValid)
                    throw new RequestValidationException("Loyalty Sync SFPD mutation request validation failure.", validationResult.Errors);

                if (string.IsNullOrEmpty(passengerDetailRequest.CustomerId))
                    passengerDetailRequest.CustomerId = (await _customerProvider.GetCustomer(passengerDetailRequest.CustomerId, passengerDetailRequest.FcNumber)).Customer.CustomerId;

                if (string.IsNullOrEmpty(passengerDetailRequest.FcNumber))
                    passengerDetailRequest.FcNumber = (await _customerProvider.GetCustomer(passengerDetailRequest.CustomerId, passengerDetailRequest.FcNumber)).Customer.LoyaltyNumber;

                var response = await _loyaltySyncMutationProvider.Mutate((SFPDSyncRequest)passengerDetailRequest);

                // If there were any errors, then report them to consumer, 
                if (response.Errors is not null)
                {
                    var builder = ErrorBuilder.New().SetMessage("SFPD sync mutation request failed.");

                    foreach (var err in response.Errors)
                    {
                        builder.SetExtension("code", err.Code);
                        builder.SetExtension("message", err.Message);
                        builder.SetExtension("source", err.ErrorSource);
                    }

                    throw new GraphQLException(builder.Build());
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke SFPD sync");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in Loyalty Sync mutation");
                throw;
            }
        }

    }
}
