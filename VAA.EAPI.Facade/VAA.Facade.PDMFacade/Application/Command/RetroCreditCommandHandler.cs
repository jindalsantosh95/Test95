using MediatR;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.Facade.PDMFacade.Domain;
using VAA.Facade.PDMFacade.Helper;

namespace VAA.Facade.PDMFacade.Application.Command
{
    public class RetroCreditCommandHandler : IRequestHandler<RetroCreditCommand, Response<RetroCreditResponse>>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;

        public RetroCreditCommandHandler(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Response<RetroCreditResponse>> Handle(RetroCreditCommand request, CancellationToken cancellationToken)
        {
            return new Response<RetroCreditResponse>()
            {
                Status = Status.OK,
                Value = await _httpConnector.SendAsync<RetroCreditResponse>($"{ConfigReader.Get(Constants.Environment.PdmUrl)}{ConfigReader.Get(Constants.Application.OnlineRetroEndpoint)}", MediaTypeHelper.GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.PdmAppId)), HttpMethod.Post, Json.Serialize(request.RetroCreditRequest))
            };
        }
        
    }
}
