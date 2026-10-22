using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Microservices.Retail.Seat.Application;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Services.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability
{
    public class OrderSeatV8DeleteSeatCommandHandler: IRequestHandler<OrderSeatV8DeleteSeatCommand, SeatDeletionResponse>
    {
        private readonly IAir4SeatV8 _iAir4SeatV8;
        private readonly ICachedChannel _cachedChannel;
        private readonly ILogger<OrderSeatV8DeleteSeatCommandHandler> _logger;

        public OrderSeatV8DeleteSeatCommandHandler(IAir4SeatV8 iAir4SeatV8, ILogger<OrderSeatV8DeleteSeatCommandHandler> logger, ICachedChannel cachedChannel)
        {
            _iAir4SeatV8 = iAir4SeatV8 ?? throw new ArgumentNullException(nameof(iAir4SeatV8));
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SeatDeletionResponse> Handle(OrderSeatV8DeleteSeatCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var channelInfo = await _cachedChannel.GetChannelEntity(request.ChannelId);
                
                //get list of requests from request object basis different flight segments
                var requestedSegments = request.OrderAmendments.SelectMany(x => x.FlightItems).DistinctBy(x => x.Segments).ToList();
                List<DeleteResponseType> delSeatresponses = new List<DeleteResponseType>();
                foreach (var segment in requestedSegments)
                {
                    //map and call delete seat for each segment
                    var segmentWiseRequest = (OrderSeatV8DeleteSeatCommand)(request, segment.Segments);

                    var delSeatRequest = SeatV8ReqMapper.MapDeleteRequest(segmentWiseRequest, channelInfo);
                    var delSeatresponse = await _iAir4SeatV8.DeleteSeat(delSeatRequest);
                    delSeatresponses.Add(delSeatresponse);
                }

                var response = SeatDeletionResponseMapper.MapDeleteResponse(delSeatresponses, request);

                return response;
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {exception.Message}");
                throw Air4FaultExceptionChecker.SeatV8Exception(exception, Constants.ErrorCodes.DeltaSeatV8ErrorCode);
            }
        }
    }
}
