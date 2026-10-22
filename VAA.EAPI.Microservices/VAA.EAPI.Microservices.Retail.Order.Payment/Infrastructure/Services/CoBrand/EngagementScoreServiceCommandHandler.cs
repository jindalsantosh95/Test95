using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CoBrand
{
    public class EngagementScoreCommandHandler : IRequestHandler<EngagementScoreServiceCommand, Result<Response<EngagementResponse>>>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IVredToken _vredToken;

        public EngagementScoreCommandHandler(IHttpConnector httpConnector, IVredToken vredToken)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _vredToken = vredToken ?? throw new ArgumentNullException(nameof(vredToken));
        }

        public async Task<Result<Response<EngagementResponse>>> Handle(EngagementScoreServiceCommand request, CancellationToken cancellationToken)
        {
            var token= await _vredToken.GetToken();

            var engagementUrl = ConfigReader.Get(Constants.Environment.VredEngagementUrl);
            var engagementScore = await _httpConnector.SendAsync<EngagementScoreResponse>(engagementUrl, null, token, verb: HttpMethod.Post, content: Json.Serialize(new EngagementRequest() { Email = request.Email}));

            return Result.Success(new Response<EngagementResponse>
            {
                Status = Status.OK,
                Value = new EngagementResponse { AccountInfo = engagementScore.AccountInfo },
            });
        }
       
    }
}
