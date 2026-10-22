using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange
{
    public class Air4TicketingProcessV3PerRevenueCommandHandler : IRequestHandler<Air4TicketingProcessV3PerRevenueCommand, PerformRevenueResponseType>
    {
        private readonly IAir4TicketingProcessV3 _iAir4TicketingProcessV3;
        private readonly ILogger<Air4TicketingProcessV3PerRevenueCommandHandler> _logger;
        public Air4TicketingProcessV3PerRevenueCommandHandler(IAir4TicketingProcessV3 iAir4TicketingProcessV3, ILogger<Air4TicketingProcessV3PerRevenueCommandHandler> logger)
        {
            _iAir4TicketingProcessV3 = iAir4TicketingProcessV3 ?? throw new ArgumentNullException(nameof(iAir4TicketingProcessV3));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<PerformRevenueResponseType> Handle(Air4TicketingProcessV3PerRevenueCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var ticketingProcessV3Request = TicketingProcessV3ReqResMapper.PerformRevenueReqMapper(request.SessionId, request.TransactionId, request.Location, request.EmailAddress, request.ChannelInfo);
                var serviceResponse = await _iAir4TicketingProcessV3.PerformRevenue(ticketingProcessV3Request);
                return serviceResponse;
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.TicketingProcessV3Exception(e, ErrorCodes.DeltaTicketingProcessV3ErrorCode);
            }
        }
    }
}
