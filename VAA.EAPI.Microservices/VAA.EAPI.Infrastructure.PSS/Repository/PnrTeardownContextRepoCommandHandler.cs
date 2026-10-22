using MediatR;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class PnrTeardownContextRepoCommandHandler(IPNRCommonV6 air4PnrCommonV6) : IRequestHandler<PnrTeardownContextRepoCommand, TeardownContextResponseType>
    {
        private readonly IPNRCommonV6 _air4PnrCommonV6 = air4PnrCommonV6 ?? throw new ArgumentNullException(nameof(air4PnrCommonV6));

        public Task<TeardownContextResponseType> Handle(PnrTeardownContextRepoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var response = _air4PnrCommonV6.Air4TeardownContext(TearDownContextRequestMapper.Mapper(request.SessionId, request.IsOrderEmail));
                return Task.FromResult(response);
            }
            catch (FaultException exception)
            {
                throw Air4FaultExceptionChecker.PNRRetrieveException(exception);
            }
        }
    }
}
