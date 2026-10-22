using MediatR;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.Facade.WalletFacade.Application.Command
{
    public class SendUpdatedPassCommandHandler : IRequestHandler<SendUpdatedPassCommand, byte[]>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;

        public SendUpdatedPassCommandHandler(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<byte[]> Handle(SendUpdatedPassCommand request, CancellationToken cancellationToken)
        {
            return await _httpConnector.GetAsync<byte[]>($"{ConfigReader.Get(Constants.Environment.WalletUrl)}{Constants.Application.SendUpdatedPassEndpoint}".Replace("{serialNumber}", request.FcNumber), GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.WalletAppId)));
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
