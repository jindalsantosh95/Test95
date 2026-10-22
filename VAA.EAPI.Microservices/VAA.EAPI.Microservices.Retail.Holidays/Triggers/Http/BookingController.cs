using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.Retail.Holidays.Application;
using VAA.EAPI.Microservices.Retail.Holidays.Domain;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;

namespace VAA.EAPI.Microservices.Retail.Holidays.Triggers.Http
{
    public sealed class BookingController(IRetrieveBookingService retrieveBookingService, IRetrieveDocumentService retrieveDocumentService)
    {
        private readonly IRetrieveBookingService _retrieveBookingService = retrieveBookingService ?? throw new ArgumentNullException(nameof(retrieveBookingService));
        private readonly IRetrieveDocumentService _retrieveDocumentService = retrieveDocumentService ?? throw new ArgumentNullException(nameof(retrieveDocumentService));

        [OpenApiOperation(operationId: "retrieveBooking", tags: new[] { "retrieveBooking" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(TripsDetailsRequest), Description = "Retrieve booking request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TripsDetailsResponse), Summary = "OK", Description = "OK")]
        [Function("RetrieveBooking")]
        public async Task<HttpResponseData> RetrieveBooking([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "retrieveBooking")] HttpRequestData req)
        {
            var request = Json.Deserialize<TripsDetailsRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            var response = await _retrieveBookingService.Handle(request, CancellationToken.None);

            return await ResultExtention<TripsDetailsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<TripsDetailsResponse>()
            {
                Status = response is null ? Status.NotFound : (response.Booking is null ? Status.BadRequest : Status.OK),
                Value = response,
                Errors = response?.Errors?.ToList(),
            }));
        }

        [OpenApiOperation(operationId: "retrieveBookingList", tags: new[] { "retrieveBookingList" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(IEnumerable<TripsDetailsRequest>), Description = "Retrieve list of booking requests")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TripListResponseStore), Summary = "OK", Description = "OK")]
        [Function("RetrieveBookingList")]
        public async Task<HttpResponseData> RetrieveBookingList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "retrieveBookingList")] HttpRequestData req)
        {
            var request = Json.Deserialize<IEnumerable<TripsDetailsRequest>>(await new StreamReader(req.Body).ReadToEndAsync());

            var response = await _retrieveBookingService.HandleList(request, CancellationToken.None);

            return await ResultExtention<TripListResponseStore>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<TripListResponseStore>()
            {
                Status = response is null ? Status.NotFound : Status.OK,
                Value = response,
            }));
        }

        [OpenApiOperation(operationId: "retrieveDocumentList", tags: new[] { "retrieveDocumentList" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RetrieveDocumentRequest), Description = "Retrieve document list request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HolidaysDocumentListResponse), Summary = "OK", Description = "OK")]
        [Function("RetrieveDocumentList")]
        public async Task<HttpResponseData> RetrieveDocumentList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "retrieveDocumentList")] HttpRequestData req)
        {
            var request = Json.Deserialize<RetrieveDocumentRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            var response = await _retrieveDocumentService.Handle(request, CancellationToken.None);

            return await ResultExtention<HolidaysDocumentListResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<HolidaysDocumentListResponse>()
            {
                Status = response is null ? Status.NotFound : Status.OK,
                Value = response,
            }));
        }

        [OpenApiOperation(operationId: "retrieveDocumentDownloadLink", tags: new[] { "retrieveDocumentDownloadLink" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "docId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Document Id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HolidaysDocumentLinkItem), Summary = "OK", Description = "OK")]
        [Function("RetrieveDocumentDownloadLink")]
        public async Task<HttpResponseData> RetrieveDocumentDownloadLink([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "retrieveDocumentDownloadLink/{docId}")] HttpRequestData req, int docId)
        {
            var response = await _retrieveDocumentService.RetrieveDocumentDownloadLink(new DocumentLinkRequest { DocumentId = docId }, CancellationToken.None);

            var statusCode = response?.Response?.ResponseStatus?.HttpStatusCode;
            var status = statusCode switch
            {
                200 => Status.OK,
                404 => Status.NotFound,
                _ => response is null ? Status.NotFound : Status.Error
            };

            return await ResultExtention<HolidaysDocumentLinkItem>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<HolidaysDocumentLinkItem>()
            {
                Status = status,
                Value = response,
                Errors = status == Status.OK
                    ? null
                    : response?.Response?.ResponseStatus?.Errors?.Select(e => new ErrorDetails
                    {
                        Code = e.Code,
                        Message = e.Message,
                        DeveloperMessage = e.DeveloperMessage,
                        ErrorSource = e.ErrorSource
                    }).ToList(),
            }));
        }

        [OpenApiOperation(operationId: "retrievePendingDocumentList", tags: new[] { "retrievePendingDocumentList" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RetrieveDocumentRequest), Description = "Retrieve pending document list request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HolidaysPendingDocumentListResponse), Summary = "OK", Description = "OK")]
        [Function("RetrievePendingDocumentList")]
        public async Task<HttpResponseData> RetrievePendingDocumentList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "retrievePendingDocumentList")] HttpRequestData req)
        {
            var request = Json.Deserialize<RetrieveDocumentRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            var response = await _retrieveDocumentService.RetrievePendingDocumentList(request, CancellationToken.None);
            return await ResultExtention<HolidaysPendingDocumentListResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<HolidaysPendingDocumentListResponse>()
            {
                Status = response is null ? Status.NotFound : Status.OK,
                Value = response,
            }));
        }

        [OpenApiOperation(operationId: "retrieveDocumentDownloadLinkList", tags: new[] { "retrieveDocumentDownloadLinkList" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RetrieveDocumentLinkListRequest), Description = "List of document link requests")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HolidaysDocumentLinkListResponse), Summary = "OK", Description = "OK")]
        [Function("RetrieveDocumentDownloadLinkList")]
        public async Task<HttpResponseData> RetrieveDocumentDownloadLinkList(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "retrieveDocumentDownloadLinkList")] HttpRequestData req)
        {
            var request = Json.Deserialize<RetrieveDocumentLinkListRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            var response = await _retrieveDocumentService.RetrieveDocumentDownloadLinkList(request, CancellationToken.None);
            return await ResultExtention<HolidaysDocumentLinkListResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<HolidaysDocumentLinkListResponse>()
            {
                Status = response is null ? Status.NotFound : Status.OK,
                Value = response,
            }));
        }

        [OpenApiOperation(operationId: "retrieveDocument", tags: new[] { "retrieveDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RetrieveDocumentDownloadRequest), Description = "Validated document download context")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RetrieveDocumentDownloadResponse), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Bad request", Description = "The document request is invalid.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "Not found", Description = "The document is no longer available.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Summary = "Technical error", Description = "The document could not be retrieved safely.")]
        [Function("RetrieveDocument")]
        public async Task<HttpResponseData> RetrieveDocument([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "retrieveDocument")] HttpRequestData req, CancellationToken cancellationToken)
        {
            RetrieveDocumentDownloadRequest request;
            try
            {
                using var reader = new StreamReader(req.Body);
                var requestBody = await reader.ReadToEndAsync(cancellationToken);
                request = requestBody.IsNullOrWhiteSpace()
                    ? null
                    : Json.Deserialize<RetrieveDocumentDownloadRequest>(requestBody);
            }
            catch (Newtonsoft.Json.JsonException)
            {
                request = null;
            }

            if (request is null)
            {
                return await ResultExtention<RetrieveDocumentDownloadResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<RetrieveDocumentDownloadResponse>
                {
                    Status = Status.BadRequest
                }));
            }

#nullable enable annotations
            RetrieveDocumentDownloadResponse? response;
#nullable restore annotations
            try
            {
                response = await _retrieveDocumentService.RetrieveDocument(request, cancellationToken);
            }
            catch (ArgumentException)
            {
                return await CreateRetrieveDocumentErrorResponse(req, Status.BadRequest);
            }
            catch (InvalidOperationException)
            {
                return await CreateRetrieveDocumentErrorResponse(req, Status.Error);
            }

            return await ResultExtention<RetrieveDocumentDownloadResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<RetrieveDocumentDownloadResponse>
            {
                Status = response is null ? Status.NotFound : Status.OK,
                Value = response
            }));
        }

        private static Task<HttpResponseData> CreateRetrieveDocumentErrorResponse(HttpRequestData request, Status status) =>
            ResultExtention<RetrieveDocumentDownloadResponse>.ExecuteResultAsync(request, CSharpFunctionalExtensions.Result.Success(new Response<RetrieveDocumentDownloadResponse>
            {
                Status = status
            }));
    }
}
