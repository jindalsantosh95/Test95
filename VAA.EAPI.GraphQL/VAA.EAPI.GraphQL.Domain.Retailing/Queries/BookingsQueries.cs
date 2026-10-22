using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Mapper;
using VAA.EAPI.GraphQL.Domain.Retailing.Models;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class BookingsQueries(IOrderDataProvider orderDataProvider, IOrderSearchProvider orderSearchProvider, ICateringProvider cateringProvider, ILogger<BookingsQueries> logger)
    {
        private readonly IOrderDataProvider _orderDataProvider = orderDataProvider ?? throw new ArgumentNullException(nameof(orderDataProvider));
        private readonly IOrderSearchProvider _orderSearchProvider = orderSearchProvider ?? throw new ArgumentNullException(nameof(orderSearchProvider));
        private readonly ICateringProvider _cateringProvider = cateringProvider ?? throw new ArgumentNullException(nameof(cateringProvider));
        private readonly ILogger<BookingsQueries> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
       
        [GraphQLName("retrieveBookings")]
        [GraphQLDescription("Retrieves a list of bookings for provided FC number ")]
        public async Task<FlyingClubBookings> RetrieveBookings()
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.OtherToken.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var jwtSecurityToken = jwtPrincipalFeature.AccessToken.OtherToken.ToJwtSecurityToken();

                var fcNumber = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type.Is("flyingclubNumber"))?.Value;

                if (fcNumber.IsNullOrWhiteSpace())
                {
                    var error = ErrorBuilder.New()
                                            .SetMessage("FC Number is required")
                                            .SetExtension("code", 400)
                                            .Build();

                    throw new GraphQLException(error);
                }

                var flyingClubBookings = await _orderDataProvider.RetrieveBookings(fcNumber);

                var bookingList = (flyingClubBookings?.FlightBookingsDetails?.Any() == true) ? OrderDataMapper.PnrMapper(fcNumber, flyingClubBookings) : null;

                if (bookingList == null || bookingList.Bookings.IsNullOrEmpty())
                {
                    var error = ErrorBuilder.New()
                                            .SetMessage("No bookings found for the provided FC Number.")
                                            .SetExtension("code", "NO_BOOKINGS_FOUND")
                                            .Build();

                    throw new GraphQLException(error);
                }

                return bookingList;
            }
            catch (Exception ex)
            {                
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId}");
                
                throw;                                
            }
        }

        [GraphQLName("retrieveBooking")]
        [GraphQLDescription("Retrieves details of a specific booking")]
        public async Task<PNR> RetrieveBooking(string bookingRef, string firstName, string lastName)
        {
            try
            {
                if (bookingRef.IsNullOrWhiteSpace() || firstName.IsNullOrWhiteSpace() || lastName.IsNullOrWhiteSpace())
                {
                    var error = ErrorBuilder.New()
                                            .SetMessage("Booking Reference, First Name and Last Name are required fields")
                                            .SetExtension("code", 400)
                                            .Build();

                    throw new GraphQLException(error);
                }

                var response = await _orderSearchProvider.RetrieveBooking(bookingRef, firstName, lastName);

                var booking = OrderSearchMapper.PnrMapper(response);

                if (booking == null)
                {
                    var error = ErrorBuilder.New().SetMessage("Booking not found for the provided details.").SetExtension("code", "PNR_NOT_FOUND").Build();

                    throw new GraphQLException(error);
                }
                else
                {
                    return await _cateringProvider.IsEligible(booking);
                }
            }          
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId}");

                throw;               
            }
        }
    }
}
