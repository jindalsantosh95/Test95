using MediatR;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails;
using VAA.EAPI.Microservices.Loyalty.Vred.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class VredCardDetailsServiceHandler : IRequestHandler<VredCardDetailsServiceQuery, VredCardDetailsResponse>
    {
        private readonly IHttpConnector _httpConnector;

        public VredCardDetailsServiceHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<VredCardDetailsResponse> Handle(VredCardDetailsServiceQuery request, CancellationToken cancellationToken)
        {
            var result = await _httpConnector.GetAsync<string>(
                            $"{ConfigReader.Get(Constants.Environment.UpdateVredurlSettingName)}{Constants.Application.ParterUserId + request.MemberNumber + Constants.Application.RequestId + Guid.NewGuid()}",
                            headers: new Dictionary<string, string>(),
                            bearerToken: await TokenHelper.GetToken(_httpConnector));

            if (result is not null)
                return Json.Deserialize<VredCardDetailsResponse>(result);
            else
                return new VredCardDetailsResponse()
                {
                    Errors = EAPI.Infrastructure.Helper.ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
        }
    }
}
