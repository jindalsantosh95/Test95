using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class PNRSetupContextRepoHandler : IRequestHandler<PNRSetupContextRepoQuery, string>
    {
        private readonly ILogger<PNRSetupContextRepoHandler> _logger;
        private readonly IPNRCommonV6 _air4PnrCommonV6;

        public PNRSetupContextRepoHandler(ILogger<PNRSetupContextRepoHandler> logger, IPNRCommonV6 air4PnrCommonV6)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _air4PnrCommonV6 = air4PnrCommonV6 ?? throw new ArgumentNullException(nameof(air4PnrCommonV6));
        }

        public Task<string> Handle(PNRSetupContextRepoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"PNR SetupContext delta service call start for PNR - {request}");
                var response = _air4PnrCommonV6.Air4SetupContext(SetupContextRequestMapper.Mapper(request.RecordLocator, request.AssignPrinter));
                return Task.FromResult(response.SessionId);
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception, "Fault Exception from PNR SetupContext service");
                throw Air4FaultExceptionChecker.PNRRetrieveException(exception);
            }
            finally
            {
                _logger.LogInformation($"PNR SetupContext delta service call end for PNR - {request}");
            }
        }
    }
}
