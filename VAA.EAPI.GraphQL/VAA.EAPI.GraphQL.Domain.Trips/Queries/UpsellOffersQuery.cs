using HotChocolate;
using HotChocolate.Types;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Web.Http;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell;
using VAA.EAPI.GraphQL.Domain.Trips.Mutations;

namespace VAA.EAPI.GraphQL.Domain.Trips.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class UpsellOffersQuery
    {
        private readonly IUpsellOfferDetailsProvider _upsellOfferDetailsProvider;
        private readonly IReferenceServiceProvider _referenceServiceProvider;
        private readonly ILogger<UpsellOffersQuery> _logger;

        public UpsellOffersQuery(IUpsellOfferDetailsProvider upsellOfferDetailsProvider, IReferenceServiceProvider referenceServiceProvider, ILogger<UpsellOffersQuery> logger)
        {
            _upsellOfferDetailsProvider = upsellOfferDetailsProvider ?? throw new ArgumentNullException(nameof(upsellOfferDetailsProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("UpsellOffersQuery")]
        [GraphQLDescription("Retrieve upsell offer details using tripReservationId")]
        public async Task<UpsellOfferResponse> GetUpsellOfferDetails(string tripReservationID)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(tripReservationID);
                IEnumerable<Models.ReferenceData.AirportRefData> airportRefdata = [];
                IEnumerable<Models.ReferenceData.CabinRefData> cabinRefData = [];
                IEnumerable<Models.ReferenceData.AirlineRefData> airlineRefData = [];

                Parallel.Invoke(
                           new Action(() => airportRefdata = _referenceServiceProvider.RetrieveAirportDataAsync().GetAwaiter().GetResult()),
                           new Action(() => cabinRefData = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                           new Action(() => airlineRefData = _referenceServiceProvider.RetrieveAirlineDetails().GetAwaiter().GetResult())
                         );

                return (UpsellOfferResponse)(await _upsellOfferDetailsProvider.QueryUpsellOfferDetails(await BookingIdHelper.GetMSBookingId(tripReservationID)), tripReservationID, airportRefdata, cabinRefData, airlineRefData);

            }
            catch (Exception ex)
            {
                var msError = !ex.InnerException.IsNullOrDefault() ? await (ex.InnerException as HttpResponseException)?.Response?.Content?.ReadAsStringAsync() : ex.Message;
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {tripReservationID} | Error message - {msError}");

                throw new Exception(Constants.ErrorCodes.SelectSeatErrorCode, ex); 
            }
            
        }
    }
}