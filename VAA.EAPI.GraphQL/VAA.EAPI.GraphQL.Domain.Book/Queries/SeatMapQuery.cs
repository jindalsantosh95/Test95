using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat;

namespace VAA.EAPI.GraphQL.Domain.Book.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class SeatMapQuery
    {


        private readonly ISeatServiceProvider _seatServiceProvider;
        private readonly ILOPAServiceProvider _lopaServiceProvider;
        private readonly ILogger<SeatMapQuery> _logger;
        private readonly IRefServiceProvider _referenceServiceProvider;

        public SeatMapQuery(ILogger<SeatMapQuery> logger, ISeatServiceProvider seatServiceProvider, ILOPAServiceProvider lopaServiceProvider, IRefServiceProvider referenceServiceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _seatServiceProvider = seatServiceProvider ?? throw new ArgumentNullException(nameof(seatServiceProvider));
            _lopaServiceProvider = lopaServiceProvider ?? throw new ArgumentNullException(nameof(lopaServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
        }

        [UseSorting]
        [UseFiltering]
        [GraphQLName("retrieveSeat")]
        public async Task<SeatMapResponse> RetrieveSeat(FlightSeatMapRequest request)
        {

            try
            {
                await Validation.ValidateSeatMap.ValidateSeatMapRequest(request);

                string channelId = AsyncContext.Current.Context.GetHeader(Constants.Application.ChannelId);

                var retrieveSeatMapRequest = SeatMapper.SeatMSRequestMapper(request, channelId);
                var retrieveSeatMapResponse = await _seatServiceProvider.RetrieveSeatMapAsync(retrieveSeatMapRequest);
                var equipmentCode = retrieveSeatMapResponse.FlightSegmentSeatMap.First().FlightSegment.EquipmentCode;

                var refCabinClassDetails = _referenceServiceProvider.RetrieveCabinClassAsync().GetAwaiter().GetResult();
                var brandId = string.IsNullOrEmpty(request.FlightSegment.BrandId) ? refCabinClassDetails.First(x => x.CabinClass == request.FlightSegment.ClassOfService).BrandID : request.FlightSegment.BrandId;

                var aircraftTemplate = await _lopaServiceProvider.RetrieveAircraftDetailsAsync(request.FlightSegment.Aircraft, equipmentCode);

                return SeatMapper.FlightSeatResponseMapper(retrieveSeatMapResponse, aircraftTemplate, brandId, request.CustomerDetails.Any(c => c.Ptc == "CNN" || c.Ptc == "INF"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: Exception - {ex} with Request: {Json.Serialize(request)}");

                throw new GraphQLException(ErrorBuilder.New()
                                    .SetMessage("Unhandled Exception in RetrieveSeat query.")
                                    .SetExtension("Exception", ex.Message)
                                    .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                                    .Build());
            }
        }
    }
}