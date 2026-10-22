using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange
{
    public class PNRCommonV6TearDownCommandHandler : IRequestHandler<PNRCommonV6TearDownCommand, TeardownContextResponseType>
    {
        private readonly IAir4PnrCommonV6 _iAir4PnrCommonV6;
        private readonly ILogger<PNRCommonV6TearDownCommandHandler> _logger;
        private readonly IMediator _mediator;
        public PNRCommonV6TearDownCommandHandler(IAir4PnrCommonV6 iAir4PnrCommonV6, ILogger<PNRCommonV6TearDownCommandHandler> logger, IMediator mediator)
        {
            _iAir4PnrCommonV6 = iAir4PnrCommonV6 ?? throw new ArgumentNullException(nameof(iAir4PnrCommonV6));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<TeardownContextResponseType> Handle(PNRCommonV6TearDownCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("PNR Common V6 Tear Down Handler - Start");
                var tearDownRequest = PNRCommonV6ReqResMapper.TeardownContextRequest(request.SessionId, request.TransactionId, request.Location, request.ChannelInfo);
                var serviceResponse = _iAir4PnrCommonV6.Air4TearDownContext(tearDownRequest);
                _logger.LogDebug("PNR Common V6 Tear Down Handler - End");
                return serviceResponse;
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.PNRCommonV6Exception(e, ErrorCodes.DeltaTearDownErrorCode);
            }
        }
    }
}
