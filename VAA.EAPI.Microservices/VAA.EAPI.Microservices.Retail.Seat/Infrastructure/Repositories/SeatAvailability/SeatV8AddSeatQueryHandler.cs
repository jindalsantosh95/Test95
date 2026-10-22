using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Seat.Constants;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Services.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability
{
    public class SeatV8AddSeatQueryHandler : IRequestHandler<SeatV8AddSeatQuery, IEnumerable<AddResponseType>>
    {
        private readonly ILogger<SeatV8AddSeatQueryHandler> _logger;
        private readonly IAir4SeatV8 _iAir4SeatV8;
        private readonly ICachedChannel _cachedChannel;

        public SeatV8AddSeatQueryHandler(ILogger<SeatV8AddSeatQueryHandler> logger, IAir4SeatV8 iAir4SeatV8, ICachedChannel cachedChannel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iAir4SeatV8 = iAir4SeatV8 ?? throw new ArgumentNullException(nameof(iAir4SeatV8));
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
        }

        public async Task<IEnumerable<AddResponseType>> Handle(SeatV8AddSeatQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var channelInfo = await _cachedChannel.GetChannelEntity(request.ChannelId);

                //get list of requests from request object basis different flight segments
                var requestedSegments = request.SelectedSeatInfo.Select(x => x.FlightIdRef).Distinct().ToList();
                var currentSegmentCounter = requestedSegments.Count;
                List<AddResponseType> addSeatresponses = new List<AddResponseType>();
                foreach (var segment in requestedSegments)
                {
                    //map and call delete seat for each segment
                    var segmentWiseRequest = (SeatV8AddSeatQuery)(request, segment);

                    var addSeatRequest = SeatFulFilmentRequestMapper.MapAddSeat(segmentWiseRequest, channelInfo);

                    try
                    {
                        var addResponseresponse = await _iAir4SeatV8.AddSeat(addSeatRequest);
                        addSeatresponses.Add(addResponseresponse);
                        currentSegmentCounter--;
                    }
                    catch (FaultException exception)
                    {
                        var logException = Air4FaultExceptionChecker.SeatV8Exception(exception, ErrorCodes.DeltaSeatV8ErrorCode);
                        _logger.LogError(exception, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {logException}");
                        if (currentSegmentCounter > 1)
                        {
                            addSeatresponses.Add(new() { SeatError = [new() { Error = new() { ErrorText = exception.Message } }]});
                            continue;
                        }
                        else throw Air4FaultExceptionChecker.SeatV8Exception(exception, ErrorCodes.DeltaSeatV8ErrorCode);
                    }

                }
               
                return addSeatresponses;
            }
            catch (FaultException exception)
            {
                var logException = Air4FaultExceptionChecker.SeatV8Exception(exception, ErrorCodes.DeltaSeatV8ErrorCode);
                _logger.LogError(exception, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {logException}");

                throw Air4FaultExceptionChecker.SeatV8Exception(exception, ErrorCodes.DeltaSeatV8ErrorCode);
            }            
        }
    }
}
