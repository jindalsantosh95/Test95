using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability
{
    public class SeatAvailabilityQueryHandler : IRequestHandler<SeatAvailabilityQuery, Result<Response<SeatAvailabilityResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SeatAvailabilityQueryHandler> _logger;
        private readonly ICachedChannel _cachedChannel;
        private readonly IDistributedCacheService _distributedCacheService;

        public SeatAvailabilityQueryHandler(IMediator mediator, ILogger<SeatAvailabilityQueryHandler> logger, ICachedChannel cachedChannel, IDistributedCacheService distributedCacheService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<Result<Response<SeatAvailabilityResponse>>> Handle(SeatAvailabilityQuery requestData, CancellationToken cancellationToken)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            try
            {
                if (requestData.ChannelId.IsNullOrEmpty())
                {
                    requestData.ChannelId = Constants.Application.NdcChannel;
                }

                var channelInfo = await _cachedChannel.GetChannelEntity(requestData.ChannelId);
                var docType = Constants.Application.SeatDocType;

                List<RetrievePassengerMapResponseType> retrievePassengerMapResponseTypeList = new();

                SeatAvailabilityResponse seatAvailabilityResponse = new()
                {
                    SeatOfferId = Guid.NewGuid().ToString(),
                    FlightSegmentSeatMap = new(),
                    OrderId = requestData.OrderId,
                    Parties = requestData.Parties,
                    Pos = requestData.Pos
                };

                var tasks = requestData.FlightSegments.Select(async segment =>
                {
                    var seatRequest = SeatAvailabilityRequestMapper.MapSeatRequest(requestData, segment.SegmentId, channelInfo);
                    var retrievePassengerMapResponseType = await _mediator.Send(seatRequest, cancellationToken);
                    
                    if (retrievePassengerMapResponseType != null)
                    {
                        retrievePassengerMapResponseTypeList.Add(retrievePassengerMapResponseType);
                    }
                });
                await Task.WhenAll(tasks);

                foreach (var requestSegment in requestData.FlightSegments)
                {
                    retrievePassengerMapResponseTypeList.ForEach(retrievePassengerMapResponseType =>
                    {
                        if (requestSegment.SegmentId == retrievePassengerMapResponseType.FlightInfo.SegmentNumber)
                        {
                            var response = SeatAvailabitlityResponseMapper.MapSeatResponse(retrievePassengerMapResponseType, requestData);
                            seatAvailabilityResponse.FlightSegmentSeatMap.AddRange(response.FlightSegmentSeatMap);
                        }
                    });
                }

                seatAvailabilityResponse.OfferExpirationTimeLimit = DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)).ToString(Constants.Application.ZuluDateTimeFormat);

                var response = SeatAvailabilityResponseToRedisMapper.RedisCacheMapper(seatAvailabilityResponse, seatAvailabilityResponse.SeatOfferId, docType, channelInfo.ChannelId);

                var redisAbsoluteExpiration = requestData.ChannelId == Constants.Application.NdcChannel ? TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)) : TimeSpan.FromHours(2);
                await _distributedCacheService.Set(seatAvailabilityResponse.SeatOfferId, response, false, TimeSpan.FromSeconds(0), redisAbsoluteExpiration);

                return new Response<SeatAvailabilityResponse>
                {
                    Status = Status.OK,
                    Value = seatAvailabilityResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");
                return Result.Failure<Response<SeatAvailabilityResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
