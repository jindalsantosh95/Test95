using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Models.RefData;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Extensions;

namespace VAA.EAPI.Web.Book.Trigger.Http
{
    public class RefDataController
    {
        private readonly IMediator _mediator;

        public RefDataController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "Countries", tags: new[] { "Countries" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Retrieve list of countries and dialing codes",
            Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<Country>), Summary = "OK", Description = "OK")]
        [Function("Countries")]
        public async Task<HttpResponseData> Countries([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Countries")] HttpRequestData req)
        {
            var result = await _mediator.Send(new RetrieveCountriesRequest());

            return await ResultExtention<IEnumerable<Country>>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<IEnumerable<Country>>()
            {
                Status = Status.OK,
                Value = result,
            }));
        }

        [OpenApiOperation(operationId: "FQTProgrammes", tags: new[] { "FQTProgrammes" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Retrieve list of FQT Programmes",
            Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<FQTProgramme>), Summary = "OK", Description = "OK")]
        [Function("FQTProgrammes")]
        public async Task<HttpResponseData> FQTProgrammes([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "FQTProgrammes")] HttpRequestData req)
        {
            var result = await _mediator.Send(new RetrieveFQTProgrammesRequest());

            return await ResultExtention<IEnumerable<FQTProgramme>>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<IEnumerable<FQTProgramme>>()
            {
                Status = Status.OK,
                Value = result,
            }));
        }
    }
}
