using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class PNRRetrieveHistoryRepoHandler : IRequestHandler<PNRRetrieveHistoryRepoQuery, PNRCommonV6RetrieveResponse>
    {
        private readonly ILogger<PNRRetrieveRepoHandler> _logger;
        private readonly IPNRCommonV6 _air4PnrCommonV6;

        public PNRRetrieveHistoryRepoHandler(ILogger<PNRRetrieveRepoHandler> logger, IPNRCommonV6 air4PnrCommonV6)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _air4PnrCommonV6 = air4PnrCommonV6 ?? throw new ArgumentNullException(nameof(air4PnrCommonV6));
        }

        public Task<PNRCommonV6RetrieveResponse> Handle(PNRRetrieveHistoryRepoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"PNR Retrieve History delta service call start for PNR - {request.RLOC}");
                var pnrRetrieveResponseType = _air4PnrCommonV6.Air4Retrieve(PNRRetrieveHistoryRQMapper.MapToPnrRetrieveRequest(request.RLOC));
                return Task.FromResult(PNRRetrieveHistoryRSMapper.MapToPnrRetrieveResponse(pnrRetrieveResponseType));
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception, "Fault Exception from PNR Retrieve History service");
                throw Air4FaultExceptionChecker.PNRRetrieveException(exception);
            }
            finally
            {
                _logger.LogInformation($"PNR Retrieve History delta service call end for PNR - {request.RLOC}");
            }
        }
    }
}
