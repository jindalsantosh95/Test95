using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.StampFCNumToPNR;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class StampFCNumToPNRMutations
    {
        private readonly IStampFCNumToPNRMutationsProvider _stampFCNumToPNRMutationsProvider;

        public StampFCNumToPNRMutations(IStampFCNumToPNRMutationsProvider stampFCNumToPNRMutationsProvider)
        {
            _stampFCNumToPNRMutationsProvider = stampFCNumToPNRMutationsProvider ?? throw new ArgumentNullException(nameof(stampFCNumToPNRMutationsProvider));
        }

        [Authorize(Roles = new[] { "Loyalty.FCToPNR.Add" })]
        [GraphQLName("loyaltyStampFCNumToPNR")]
        [GraphQLDescription("Create stamping of PNR and FC Number")]
        public async Task<StampFCNumToPNRResponse> StampFCNumToPNR(StampFCNumToPNRRequest request)
        {
            var validationResult = new StampingFCNumToPNRValidation(HttpMethod.Post).Validate(request);

            if (!validationResult.IsValid)
                throw new RequestValidationException("StampSync mutation request validation failure.", validationResult.Errors);

            return await _stampFCNumToPNRMutationsProvider.StampingFCNumberToPNR(request);

        }


    }
}
