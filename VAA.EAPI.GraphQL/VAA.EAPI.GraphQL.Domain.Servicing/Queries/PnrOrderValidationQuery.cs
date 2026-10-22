using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Servicing.Interfaces;
using VAA.EAPI.GraphQL.Domain.Servicing.Models.PnrOrderValidation;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class PnrOrderValidationQuery(IPnrOrderValidationProvider provider)
    {
        private readonly IPnrOrderValidationProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        [GraphQLName("servicingValidatePnrOrder")]
        [GraphQLDescription("Validates that a PNR exists and checks supplied passenger names against the order document.")]
        [AllowAnonymous]
        public async Task<PnrOrderValidationResponse> Validate(PnrOrderValidationRequest request)
        {
            return await _provider.ValidateAsync(request);
        }
    }
}
