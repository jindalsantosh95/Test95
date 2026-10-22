using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.Facade.WalletFacade.Constants;

namespace VAA.Facade.WalletFacade.Application.Command
{
    public class RegisterPassCommandHandler : IRequestHandler<RegisterPassCommand, Response<int>>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;

        public RegisterPassCommandHandler(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Response<int>> Handle(RegisterPassCommand request, CancellationToken cancellationToken)
        {

            return new Response<int>()
            {
                Status = Status.OK,
                Value = await _httpConnector.SendAsync<int>($"{ConfigReader.Get(Constants.Environment.WalletUrl)}{Constants.Application.RegisterPassEndpoint}", GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.WalletAppId)), HttpMethod.Post, Json.Serialize(request)),
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
