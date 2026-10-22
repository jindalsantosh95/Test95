using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class PNRRetrieveRepoHandler : IRequestHandler<PNRRetrieveRepoQuery, RetrieveResponseType>
    {
        private readonly ILogger<PNRRetrieveRepoHandler> _logger;
        private readonly IPNRCommonV6 _air4PnrCommonV6;

        public PNRRetrieveRepoHandler(ILogger<PNRRetrieveRepoHandler> logger, IPNRCommonV6 air4PnrCommonV6)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _air4PnrCommonV6 = air4PnrCommonV6 ?? throw new ArgumentNullException(nameof(air4PnrCommonV6));
        }

        public Task<RetrieveResponseType> Handle(PNRRetrieveRepoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"PNR Retrieve delta service call start for PNR - {request.RetrieveRequestType.SetupContextRequest.RetrieveInfo.PassengerInfo.RecordLocator}");
                return Task.FromResult(_air4PnrCommonV6.Air4Retrieve(request.RetrieveRequestType));
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception, "Fault Exception from PNR Retrieve service");
                throw Air4FaultExceptionChecker.PNRRetrieveException(exception);
            }
            finally
            {
                _logger.LogInformation($"PNR Retrieve delta service call end for PNR - {request.RetrieveRequestType.SetupContextRequest.RetrieveInfo.PassengerInfo.RecordLocator}");
            }
        }
    }
}
