using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Triggers.Http
{
    public class AcquireTokenController
    {
        private readonly IMediator _mediator;

        public AcquireTokenController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("AcquireToken")]
        public async Task<HttpResponseData> AcquireToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "token")] HttpRequestData req, FunctionContext executionContext)
        {
            var queryOutput = await _mediator.Send(AcquireTokenReqMapper.Mapper(req));
            return await ResultExtention<string>.ExecuteResultAsync(req, queryOutput);
        }
    }
}
