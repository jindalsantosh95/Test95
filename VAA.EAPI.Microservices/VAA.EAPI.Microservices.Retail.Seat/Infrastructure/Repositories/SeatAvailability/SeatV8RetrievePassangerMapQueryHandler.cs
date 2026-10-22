using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Seat.Constants;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Services.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability
{
    public class SeatV8RetrievePassengerMapQueryHandler : IRequestHandler<SeatV8RetrievePassengerMapQuery, RetrievePassengerMapResponseType>
    {
        private readonly ILogger<SeatV8RetrievePassengerMapQueryHandler> _logger;
        private readonly IAir4SeatV8 _iAir4SeatV8;

        public SeatV8RetrievePassengerMapQueryHandler(ILogger<SeatV8RetrievePassengerMapQueryHandler> logger, IAir4SeatV8 iAir4SeatV8)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iAir4SeatV8 = iAir4SeatV8 ?? throw new ArgumentNullException(nameof(iAir4SeatV8));
        }

        public async Task<RetrievePassengerMapResponseType> Handle(SeatV8RetrievePassengerMapQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _iAir4SeatV8.RetrievePassengerMap(SeatV8ReqMapper.MapRequest(request));
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception.Message, "Fault Exception from SeatV8 RetrievePassengerMap service");

                throw Air4FaultExceptionChecker.SeatV8Exception(exception, ErrorCodes.DeltaSeatV8ErrorCode);
            }
        }
    }
}
