using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FutureFlights;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class FutureFlightsQuery
    {
        private readonly ILogger<FutureFlightsQuery> _logger;
        private readonly ICustomerProvider _customerServic;
        private readonly IFutureFlightsQueryProvider _futureFlightsrService;
        public FutureFlightsQuery(ILogger<FutureFlightsQuery> logger, ICustomerProvider customerService, IFutureFlightsQueryProvider futureFlightsrService, IContactHistoryQueryProvider contactHistoryQueryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerServic = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _futureFlightsrService = futureFlightsrService ?? throw new ArgumentNullException(nameof(_futureFlightsrService));

        }

        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.FutureFlights.Read" })]
        [GraphQLName("loyaltyFutureFlights")]
        [GraphQLDescription("Retreive a list of future flights assocaited to the ffp number or contact email ")]
        public async Task<FlightResponse> RetrieveFlights(FlightRequest flightRequest)
        {
            if (string.IsNullOrEmpty(flightRequest.MemberNumber) && string.IsNullOrEmpty(flightRequest.ContactEmail) && string.IsNullOrEmpty(flightRequest.PhoneNumber))
                throw new ArgumentException($"Either '{nameof(flightRequest.MemberNumber)}' or {nameof(flightRequest.PhoneNumber)} must be spefied.");

            return await _futureFlightsrService.Query(flightRequest);
        }
    }
}
