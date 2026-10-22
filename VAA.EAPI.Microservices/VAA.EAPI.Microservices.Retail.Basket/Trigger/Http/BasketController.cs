using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Basket.Application.Command;
using VAA.EAPI.Microservices.Retail.Basket.Application.Query;
using VAA.EAPI.Microservices.Retail.Basket.Domain;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Seat;
using Environment = VAA.EAPI.Microservices.Retail.Basket.Constants.Environment;
using VAA.EAPI.Microservices.Retail.Basket.Domain.PointsCurrencyConverter;
using VAA.EAPI.Microservices.Retail.Basket.Domain.FCEarnPoints;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Insurance;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Basket.Trigger.Http
{
    public sealed class BasketController
    {
        private readonly IMediator _mediator;

        public BasketController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "CreateBasket", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(Domain.Basket), Description = "Create Basket request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CreateBasket")]
        public async Task<HttpResponseData> CreateBasket([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket")] HttpRequestData req)
        {
            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();

            var basketInput = Json.Deserialize<Domain.Basket>(await requestData.ReadAsStringAsync());

            var result = await _mediator.Send(new CreateBasketRequest { Basket = basketInput });

            result.Value.HeaderParameters.Add(new ResponseHeader { Key = "Location", Value = $"{ConfigReader.Get(Environment.BasketApimUrl)}/{result.Value.Value.BasketId}" });

            result.Value.Status = Status.RedirectMethod;

            var response = await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);

            return response;
        }

        [OpenApiOperation(operationId: "LoadBasket", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("LoadBasket")]
        public async Task<HttpResponseData> LoadBasket([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "basket/{basket:guid}")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var response = await _mediator.Send(new LoadBasketRequest
            {
                Id = basket,
            });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "DeleteBasket", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("DeleteBasket")]
        public async Task<HttpResponseData> DeleteBasket([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "basket/{basket:guid}")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var response = await _mediator.Send(new DeleteBasketRequest
            {
                Id = basket,
            });

            return await ResultExtention<bool>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "UpdatePassenger", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "peopleid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "people id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(Person), Description = "Update Passenger request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdatePassenger")]
        public async Task<HttpResponseData> UpdatePassenger([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/People/{peopleid}")] HttpRequestData req, Guid basket, string peopleid)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();
            var result = await _mediator.Send(new UpdatePassengerCommand { Id = basket, PeopleId = peopleid, People = Json.Deserialize<Person>(await requestData.ReadAsStringAsync()) });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdateContact", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(Domain.Basket), Description = "Update contact details request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateContact")]
        public async Task<HttpResponseData> UpdateContact([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/contact")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var httpRequest = await req.FunctionContext.GetHttpRequestDataAsync();
            var requestData = Json.Deserialize<Domain.Basket>(await httpRequest.ReadAsStringAsync());
            var result = await _mediator.Send(new UpdateContactDetailCommand { Id = basket, ContactDetails = requestData.ContactDetails, FcAutoMatched = requestData.FcAutoMatched });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdateTerms", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(TermsAndConditions), Description = "Update T&C request")]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateTerms")]
        public async Task<HttpResponseData> UpdateTerms([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/terms")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();
            var result = await _mediator.Send(new UpdateTnCCommand { Id = basket, TermsAndConditions = Json.Deserialize<TermsAndConditions>(await requestData.ReadAsStringAsync()) });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdateMembership", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(LoyaltyMemberDetails), Description = "Update loyalty member details request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateMembership")]
        public async Task<HttpResponseData> UpdateMembership([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/membership")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();
            var result = await _mediator.Send(new UpdateMembershipCommand { Id = basket, LoyaltyDetails = Json.Deserialize<LoyaltyMemberDetails>(await requestData.ReadAsStringAsync()) });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdateVoucher", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Voucher), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [Function("UpdateVoucher")]
        public async Task<HttpResponseData> UpdateVoucher([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/voucher")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            throw new NotImplementedException(); //TBD
        }

        [OpenApiOperation(operationId: "UpdateOfferId", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "offerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "offer id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateOfferId")]
        public async Task<HttpResponseData> UpdateOfferID([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/offer/{offerId}")] HttpRequestData req, Guid basket, string offerId)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var result = await _mediator.Send(new UpdateOfferIDCommand { Id = basket, OfferIDs = new[] { offerId } });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdateSelectedOffer", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(BookingJourneyOfferDetails), Required = true, Description = "Update offer details in basket" )]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateSelectedOffer")]
        public async Task<HttpResponseData> UpdateSelectedOffer([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/offer")] HttpRequestData req, Guid basket, BookingJourneyOfferDetails selectedOffer)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var request = Json.Deserialize<BookingJourneyOfferDetails>(await new StreamReader(req.Body).ReadToEndAsync());

            var result = await _mediator.Send(new UpdateSelectedOfferCommand { Id = basket, BookingJourneyOfferDetails = request });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdatePaymentDetails", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<PaymentSession>), Required = true, Description = "Update payment details in basket")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdatePaymentDetails")]
        public async Task<HttpResponseData> UpdatePaymentDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/payment")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();

            var result = await _mediator.Send(new UpdatePaymentCommand { Id = basket, PaymentSessions = Json.Deserialize<IEnumerable<PaymentSession>>(await requestData.ReadAsStringAsync()) });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "SetBasketOrderStatus", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SetBasketOrderStatus")]
        public async Task<HttpResponseData> SetBasketOrderStatus([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/order")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var updateOrderStatus = Json.Deserialize<UpdateOrderStatusCommand>(await new StreamReader(req.Body).ReadToEndAsync());

            updateOrderStatus.Id = basket;

            var result = await _mediator.Send(updateOrderStatus);

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdateSeatDetails", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<FlightPassengerSeatMap>), Required = true, Description = "Update seat details in basket")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateSeatDetails")]
        public async Task<HttpResponseData> UpdateSeatDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/seat")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();

            var result = await _mediator.Send(new UpdateSeatCommand { Id = basket, FlightPassengerSeatMap = Json.Deserialize<IEnumerable<FlightPassengerSeatMap>>(await requestData.ReadAsStringAsync()) });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "SetBasketFlightSummary", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SetBasketFlightSummary")]
        public async Task<HttpResponseData> SetBasketFlightSummary([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/summaryupdate")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var updateflightSummary = Json.Deserialize<UpdateFlightSummaryCommand>(await new StreamReader(req.Body).ReadToEndAsync());

            updateflightSummary.Id = basket;

            var result = await _mediator.Send(updateflightSummary);

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdateVoucherRedemption", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(UpdateVoucherRedemptionCommand), Required = true, Description = "Update voucher redemption state in basket")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [Function("UpdateVoucherRedemption")]
        public async Task<HttpResponseData> UpdateVoucherRedemption([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/voucherredemption")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var command = Json.Deserialize<UpdateVoucherRedemptionCommand>(await new StreamReader(req.Body).ReadToEndAsync());
            command.Id = basket;
            var result = await _mediator.Send(command);
            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdatePointsCurrencyDetails", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<PtcWisePointsEquivalentCurrencyDetails>), Required = true, Description = "Update points equivalent currency details in basket")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateLoyaltyPointsEquivalentCurrency")]
        public async Task<HttpResponseData> UpdateLoyaltyPointsEquivalentCurrency([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/pointsEquivalentCurrency")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();

            var result = await _mediator.Send(new UpdatePointsEquivalentCurrencyCommand { Id = basket, PtcWisePointsEquivalentCurrencyDetails = Json.Deserialize<IEnumerable<PtcWisePointsEquivalentCurrencyDetails>>(await requestData.ReadAsStringAsync()) });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdateFCEarnPoints", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(EarnPoints), Required = true, Description = "Update earn points details in basket")]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateFCEarnPoints")]
        public async Task<HttpResponseData> UpdateFCEarnPoints([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/updatefcearnpoints")] HttpRequestData req, Guid basket)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();

            var result = await _mediator.Send(new UpdateEarnPointsCommand { Id = basket, EarnPoints = Json.Deserialize<EarnPoints>(await requestData.ReadAsStringAsync()) });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "SaveInsuranceQuote", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<InsuranceQuote>), Required = true, Description = "Save insurance quote details in basket")]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SaveInsuranceQuote")]
        public async Task<HttpResponseData> SaveInsuranceQuote([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/insurancequote")] HttpRequestData req, Guid basket, string typename)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();

            var result = await _mediator.Send(new SaveInsuranceQuoteCommand
            {
                Id = basket,
                InsuranceQuotes = Json.Deserialize<IEnumerable<InsuranceQuote>>(await requestData.ReadAsStringAsync())
            });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpdateInsuranceDecision", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(InsuranceDecision), Required = true, Description = "Update insurance decision details in basket")]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateInsuranceDecision")]
        public async Task<HttpResponseData> UpdateInsuranceDecision([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/insurancedecision")] HttpRequestData req, Guid basket, string typename)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();

            var result = await _mediator.Send(new UpdateInsuranceDecisionCommand
            {
                Id = basket,
                IsInsured = Json.Deserialize<InsuranceDecision>(await requestData.ReadAsStringAsync()).InsuranceSelected
            });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "SetCoBrandStatus", tags: new[] { "Basket" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "basket", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "basket id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(CoBrandStatusRequest), Required = true, Description = "Update Co Brand application status details in basket")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Domain.Basket), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SetCoBrandStatus")]
        public async Task<HttpResponseData> SetCoBrandStatus([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basket/{basket:guid}/cobrandprogress")] HttpRequestData req, Guid basket, string typename)
        {
            if (basket.Equals(Guid.Empty)) throw new ArgumentException("Value must be a valid guid.", nameof(basket));

            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();

            var result = await _mediator.Send(new SetCoBrandStatusCommand
            {
                Id = basket,
                CoBrandProgressStatus = (CoBrandStatusEnum)Enum.Parse(typeof(CoBrandStatusEnum), Json.Deserialize<CoBrandStatusRequest>(await requestData.ReadAsStringAsync()).Status, true) 
            });

            return await ResultExtention<Domain.Basket>.ExecuteResultAsync(req, result);
        }
    }
}
