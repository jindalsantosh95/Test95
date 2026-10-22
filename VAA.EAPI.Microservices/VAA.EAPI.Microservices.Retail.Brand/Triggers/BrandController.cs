using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand;
using VAA.EAPI.Microservices.Retail.Brand.Helper;
using VAA.EAPI.Microservices.Retail.Brand.Infrastructure.BrandMS;

namespace VAA.EAPI.Microservices.Retail.Brand.Triggers
{
    public class BrandController
    {
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IMediator _mediator;
        private readonly ILogger<BrandController> _logger;
        private readonly IAtpcoCommand _atpcoCommand;

        public BrandController(IMediator mediator, ILogger<BrandController> logger, IBrandMSCommand brandMSCommand, IAtpcoCommand atpcoCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(mediator));
            _brandMSCommand = brandMSCommand ?? throw new System.ArgumentNullException(nameof(brandMSCommand));
            _atpcoCommand = atpcoCommand ?? throw new ArgumentNullException(nameof(atpcoCommand));
        }

        [OpenApiOperation(operationId: "Brand", tags: new[] { "Brand" }, Summary = "Return the various policies and ammenities of an flight offer", Description = "Return the various policies and ammenities of an flight offer", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(BrandQueryRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(Models.Brand), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Not Found", Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Brand")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "brand")] HttpRequestData req,CancellationToken cancellationToken)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var brandReq = Json.Deserialize<BrandQueryRequest>(requestBody);

            _logger.LogInformation($"Correlation Id - {AsyncContext.Current.CorrelationId} \n Brand MS Request:\n {requestBody}");

            var brandRes = await _atpcoCommand.Send(brandReq,cancellationToken);

            if (brandRes.Itineraries == null || brandRes.Itineraries.Any() == false || brandRes.Itineraries.Count == 1 && (brandRes.Itineraries.First().Legs == null || brandRes.Itineraries.First().Legs.Count == 0))
            {
                var guid = Guid.NewGuid().ToString();

                var result = Result.Success(new Response<Models.Brand>
                {
                    Status = Status.NotFound,
                    Value = brandRes,
                    Errors = brandRes.Errors.Any() ? brandRes.Errors.Select(e => ErrorMapper.MapAtpcoErrorToEapiErrorDetails(e, guid)).ToList() : null
                }); ;
                return await ResultExtention<Models.Brand>.ExecuteResultAsync(req, result);

            }
            else
            {
                var result = Result.Success(new Response<Models.Brand>
                {
                    Status = Status.OK,
                    Value = brandRes
                });
                return await ResultExtention<Models.Brand>.ExecuteResultAsync(req, result);
            }
        }

        [OpenApiOperation(operationId: "BrandPTC", tags: new[] { "BrandPTC" }, Summary = "Return the various policies and ammenities of an flight offer PTC wise", Description = "Return the various policies and ammenities of an flight offer PTC wise", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(BrandQueryRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(Models.Brand), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Not Found", Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("BrandPTC")]
        public async Task<HttpResponseData> Call([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "brand/PTC")] HttpRequestData req,CancellationToken cancellationToken)
        {
            var brandRequest = Json.Deserialize<BrandQueryRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            IEnumerable<string> totalPTC = [.. brandRequest.Itinerary.First().PassengerTypeCodes.GroupBy(x=>x[..3]).Select(selector => selector.First())];
            totalPTC = totalPTC.Select(x => x.Replace(Constants.Environment.PTCYNG, Constants.Environment.PTCYTH));

            var brandReqForAllPtc = BrandRequestPTCMapper.MapBrandMSRequest(brandRequest, totalPTC);
            
            var brandMsResponse = await _brandMSCommand.Handle(brandReqForAllPtc, totalPTC, cancellationToken);

            var result = Result.Success(new Response<IEnumerable<Models.Brand>>
            {
                Status = Status.OK,
                Value = brandMsResponse
            });

            return await ResultExtention<IEnumerable<Models.Brand>>.ExecuteResultAsync(req, result.Value);

        }
    }
}
