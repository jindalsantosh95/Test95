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
    public class RetroFeedbackCommandHandler : IRequestHandler<RetroFeedbackCommand, Response<RetroFeedbackResponse>>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;

        public RetroFeedbackCommandHandler(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Response<RetroFeedbackResponse>> Handle(RetroFeedbackCommand request, CancellationToken cancellationToken)
        {
            var Value = await _httpConnector.SendAsync<RetroFeedbackResponse>($"{ConfigReader.Get(Constants.Environment.PdmUrl)}{ConfigReader.Get(Constants.Application.RetroFeedbackEndpoint)}", MediaTypeHelper.GetHeaders(),await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.PdmAppId)),HttpMethod.Post,Json.Serialize(request.RetroFeedbackRequest));

            return new Response<RetroFeedbackResponse>()
            {
                Status = Status.OK,
                Value = Value
            };
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {  Constants.Application.OcpKeyHeader , ConfigReader.Get(Constants.Environment.PdmOcpKey) }
            };
        }
    }
}
