using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class PartnerDataManagementMutation
    {
        private readonly IPartnerDataManagementQueryProvider _partnerDataManagementProvider;

        public PartnerDataManagementMutation(IPartnerDataManagementQueryProvider partnerDataManagementProvider)
        {
            _partnerDataManagementProvider = partnerDataManagementProvider ?? throw new ArgumentNullException(nameof(partnerDataManagementProvider));
        }

        [Authorize(Roles = ["Loyalty.PDMFlownData.Update"])]
        [GraphQLName("loyaltyEditVSRetroFlownData")]
        [GraphQLDescription("Updates VS retro flown data in Partner Data Management.")]
        public async Task<EditVSRetroFlownDataResponse> EditVSRetroFlownData(EditVSRetroFlownDataRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validationResult = new EditVSRetroFlownDataValidator().Validate(request);
            if (!validationResult.IsValid)
            {
                throw new RequestValidationException("VS retro flown-data update mutation mandatory input is missing.", validationResult.Errors);
            }

            return await _partnerDataManagementProvider.UpdateVSRetroFlownDetails(request);
        }
    }
}
