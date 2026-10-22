using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class PartnerDataManagementQuery
    {
        private readonly IPartnerDataManagementQueryProvider _partnerDataManagementQueryProvider;

        public PartnerDataManagementQuery(IPartnerDataManagementQueryProvider partnerDataManagementQueryProvider)
        {
            _partnerDataManagementQueryProvider = partnerDataManagementQueryProvider ?? throw new ArgumentNullException(nameof(partnerDataManagementQueryProvider));
        }

        [Authorize(Roles = new[] { "Loyalty.PDMFlownData.Read" })]
        [GraphQLName("loyaltyGetVSRetroFlownData")]
        [GraphQLDescription("Retrieve VS retro flown data from Partner Data Management.")]
        public async Task<VSFlownDetailsResponse> GetVSRetroFlownData(GetVSRetroFlownDataRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var hasTicketNumber = !request.TicketNumber.IsNullOrWhiteSpace();
            var hasPnrAndFlightNumber = !request.Pnr.IsNullOrWhiteSpace() && !request.FlightNumber.IsNullOrWhiteSpace();

            if (!hasTicketNumber && !hasPnrAndFlightNumber)
            {
                throw new ArgumentException($"Either '{nameof(request.TicketNumber)}' must be specified, or both '{nameof(request.Pnr)}' and '{nameof(request.FlightNumber)}' must be specified.");
            }

            return await _partnerDataManagementQueryProvider.GetVSRetroFlownData(request);
        }
    }
}
