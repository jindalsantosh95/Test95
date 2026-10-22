using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VAA.EAPI.Microservices.Retail.Seat.Constants;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Services.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability
{
    public class OrderSeatV8AddSeatQueryHandler : IRequestHandler<OrderSeatV8AddSeatQuery, AddResponseType>
    {

        private readonly ILogger<OrderSeatV8AddSeatQueryHandler> _logger;
        private readonly IAir4SeatV8 _iAir4SeatV8;
        private readonly ICachedChannel _cachedChannel;

        public OrderSeatV8AddSeatQueryHandler(ILogger<OrderSeatV8AddSeatQueryHandler> logger, IAir4SeatV8 iAir4SeatV8, ICachedChannel cachedChannel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iAir4SeatV8 = iAir4SeatV8 ?? throw new ArgumentNullException(nameof(iAir4SeatV8));
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
        }
        public async Task<AddResponseType> Handle(OrderSeatV8AddSeatQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var channelInfo = await _cachedChannel.GetChannelEntity(request.ChannelId);
                var addSeatRequest = SeatFulFilmentRequestMapper.MapOrderAddSeat(request, channelInfo);

                var stringWriter = new StringWriter();
                var requestSerializer = new XmlSerializer(typeof(AddRequestType));

                requestSerializer.Serialize(stringWriter, addSeatRequest);

                _logger.LogInformation($"ITA Request: {stringWriter}");

                var addSeatResponse = await _iAir4SeatV8.AddSeat(addSeatRequest);

                var stringWriter1 = new StringWriter();
                var requestSerializer1 = new XmlSerializer(typeof(AddResponseType));

                requestSerializer1.Serialize(stringWriter1, addSeatResponse);

                _logger.LogInformation($"ITA Request: {stringWriter1}");


                return addSeatResponse;
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception.Message, "Fault Exception from SeatV8 Add service");
                throw Air4FaultExceptionChecker.SeatV8Exception(exception, ErrorCodes.DeltaSeatV8ErrorCode);
            }
            finally
            {
                _logger.LogInformation($"SeatV8 Add Service Call - End");
            }
        }
    }
}
