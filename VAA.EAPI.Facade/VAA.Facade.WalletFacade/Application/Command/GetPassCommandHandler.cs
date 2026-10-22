using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.Facade.WalletFacade.Domain;

namespace VAA.Facade.WalletFacade.Application.Command
{
    public class GetPassCommandHandler : IRequestHandler<GetPassCommand, Response<AssociatedPasses>>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;

        public GetPassCommandHandler(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Response<AssociatedPasses>> Handle(GetPassCommand request, CancellationToken cancellationToken)
        {
            return new Response<AssociatedPasses>()
            {
                Status = Status.OK,
                Value = await _httpConnector.GetAsync<AssociatedPasses>($"{ConfigReader.Get(Constants.Environment.WalletUrl)}{Constants.Application.GetPassEndpoint}".Replace("{deviceId}", request.DeviceId).Replace("{passTypeId}", request.PassTypeId), GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.WalletAppId))),
                Errors = null
            };
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {  Constants.Application.OcpKeyHeader , ConfigReader.Get(Constants.Environment.WalletOcpKey) }
            };
        }       
    }
}
