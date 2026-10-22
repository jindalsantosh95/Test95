using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Helper;
using VAA.EAPI.Web.Book.Models.YourDetails;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Exceptions;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Trigger.Http
{
    public class YourDetailsController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<YourDetailsController> _logger;

        public YourDetailsController(IMediator mediator, ILogger<YourDetailsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "UpdatePassenger", tags: new[] { "UpdatePassenger" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Update a passenger's details",
            Description = "")]
        [OpenApiParameter(name: "basketId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(YourDetailsResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter("passengerId", In = ParameterLocation.Path, Required = true)]
        [Function("UpdatePassenger")]
        public async Task<HttpResponseData> UpdatePassenger([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "passenger/{passengerId}")] HttpRequestData req, string passengerId)
        {
            try
            {
                var basketId = req?.Headers.GetValues("basketId").FirstOrDefault();
                var authToken = Utility.TryGetAuthorizationToken(req.Headers);

                var result = await _mediator.Send(new UpdatePassengerRequest(basketId, int.Parse(passengerId), authToken, req.Body));

                return await ResultExtention<YourDetailsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<YourDetailsResponse>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            catch (GraphQLException ex)
            {
                var status = GraphQLExceptionStatus.GetStatusCode(ex);

                var errors = ex.Errors.First().Extensions.Response is not null ?
                    Json.Deserialize<IEnumerable<ErrorDetails>>(ex.Errors.First().Extensions.Response) :
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ex.Errors?.First()?.Extensions?.Code), AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath);

                return await ResultExtention<YourDetailsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<YourDetailsResponse>()
                {
                    Status = status,
                    Errors = errors.ToList(),
                    IgnoreNullValues = true
                }));
            }
        }

        [OpenApiOperation(operationId: "UpdateContact", tags: new[] { "UpdateContact" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Update contact information.",
            Description = "")]
        [OpenApiParameter(name: "basketId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(YourDetailsResponse), Summary = "OK", Description = "OK")]
        [Function("UpdateContact")]
        public async Task<HttpResponseData> UpdateContact([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "contact")] HttpRequestData req)
        {
            try
            {
                var basketId = req?.Headers.GetValues("basketId").FirstOrDefault();
                var authToken = Utility.TryGetAuthorizationToken(req.Headers);

                var result = await _mediator.Send(new UpdateContactRequest(basketId, authToken, req.Body));

                return await ResultExtention<YourDetailsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<YourDetailsResponse>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            catch (GraphQLException ex)
            {
                var status = GraphQLExceptionStatus.GetStatusCode(ex);

                var errors = ex.Errors.First().Extensions.Response is not null ?
                    Json.Deserialize<IEnumerable<ErrorDetails>>(ex.Errors.First().Extensions.Response) :
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ex.Errors?.First()?.Extensions?.Code), AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath);

                return await ResultExtention<YourDetailsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<YourDetailsResponse>()
                {
                    Status = status,
                    Errors = errors.ToList(),
                    IgnoreNullValues = true
                }));
            }
        }

        [OpenApiOperation(operationId: "UpdateTerms", tags: new[] { "UpdateTerms" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Update Terms & Conditions acceptance.",
            Description = "")]
        [OpenApiParameter(name: "basketId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(YourDetailsResponse), Summary = "OK", Description = "OK")]
        [Function("UpdateTerms")]
        public async Task<HttpResponseData> UpdateTerms([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "terms")] HttpRequestData req)
        {
            try
            {
                var basketId = req?.Headers.GetValues("basketId").FirstOrDefault();
                var authToken = Utility.TryGetAuthorizationToken(req.Headers);

                var result = await _mediator.Send(new UpdateTermsRequest(basketId, authToken, req.Body));

                return await ResultExtention<YourDetailsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<YourDetailsResponse>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            catch (GraphQLException ex)
            {
                var status = GraphQLExceptionStatus.GetStatusCode(ex);

                var errors = ex.Errors.First().Extensions.Response is not null ?
                    Json.Deserialize<IEnumerable<ErrorDetails>>(ex.Errors.First().Extensions.Response) :
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ex.Errors?.First()?.Extensions?.Code), AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath);

                return await ResultExtention<YourDetailsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<YourDetailsResponse>()
                {
                    Status = status,
                    Errors = errors.ToList(),
                    IgnoreNullValues = true
                }));
            }
        }

        [OpenApiOperation(operationId: "Basket", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Retrieve Basket",
            Description = "")]
        [OpenApiParameter(name: "basketId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(YourDetailsResponse), Summary = "OK", Description = "OK")]
        [Function("Basket")]
        public async Task<HttpResponseData> Basket([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "basket")] HttpRequestData req)
        {
            try
            {
                var basketId = req?.Headers.GetValues("basketId").FirstOrDefault();
                var authToken = Utility.TryGetAuthorizationToken(req.Headers);

                var result = await _mediator.Send(new RetrieveBasketRequest(basketId, authToken));
                return await ResultExtention<YourDetailsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<YourDetailsResponse>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            catch (GraphQLException qex)
            {
                var status = GraphQLExceptionStatus.GetStatusCode(qex);

                var errors = qex.Errors.First().Extensions.Response is not null ?
                    Json.Deserialize<IEnumerable<ErrorDetails>>(qex.Errors.First().Extensions.Response) :
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(qex.Errors?.First()?.Extensions?.Code), AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath);

                return await ResultExtention<YourDetailsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<YourDetailsResponse>()
                {
                    Status = status,
                    Errors = errors.ToList(),
                    IgnoreNullValues = true
                }));
            }
        }
    }
}
