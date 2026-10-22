using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Insurance;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public sealed class BasketServiceProvider : IBasketServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.BasketMSOcpKey) } };

        private readonly IHttpConnector _connector;

        public BasketServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<Basket> CreateBasketAsync(CreateBasketRequest createBasketRQ)
        {
            string createBasketUri = ConfigReader.Get(Constants.Environment.BasketMSUrl);

            return (await _connector.SendAsync<Basket>(createBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(createBasketRQ))).Response;
        }

        public async Task<Basket> RetrieveBasketAsync(string basketid)
        {
            string retrieveBasketUri = ConfigReader.Get(Constants.Environment.BasketMSRetrieveUrl).Replace("{basket}", basketid);

            return (await _connector.GetAsync<Basket>(retrieveBasketUri, _headers, string.Empty)).Response;
        }

        public async Task<Basket> UpdateContactAsync(ContactUpdateRequest contactDetail)
        {
            string updateContactBasketUri = ConfigReader.Get(Constants.Environment.BasketMSUpdateContactUrl).Replace("{basket}", contactDetail.BasketId);

            return (await _connector.SendAsync<Basket>(updateContactBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(contactDetail))).Response;
        }

        public async Task<Basket> UpdatePeopleAsync(PassengerUpdateRequest people)
        {
            string updatePeopleBasketUri = ConfigReader.Get(Constants.Environment.BasketMSUpdatePassengerUrl).Replace("{basket}", people.BasketId).Replace("{peopleid}", people.People.Id);

            return (await _connector.SendAsync<Basket>(updatePeopleBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(people.People))).Response;
        }

        public async Task<Basket> UpdateTnCAsync(TnCUpdateRequest termsAndConditions)
        {
            string updateTnCBasketUri = ConfigReader.Get(Constants.Environment.BasketMSUpdateTermsUrl).Replace("{basket}", termsAndConditions.BasketId);

            return (await _connector.SendAsync<Basket>(updateTnCBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(termsAndConditions.TermsAndConditions))).Response;
        }

        public async Task<Basket> UpdatePaymentDetailsAsync(BasketPaymentRequest basketPaymentRequest)
        {
            string updatePaymentBasketUri = ConfigReader.Get(Constants.Environment.BasketMSUpdatePaymentUrl).Replace("{basket}", basketPaymentRequest.BasketId);

            return (await _connector.SendAsync<Basket>(updatePaymentBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(basketPaymentRequest.PaymentSessionsBasketMS))).Response;
        }

        public async Task<bool> DeleteBasketAsync(string basketid)
        {
            string retrieveBasketUri = ConfigReader.Get(Constants.Environment.BasketMSDeleteUrl).Replace("{basket}", basketid);

            return (await _connector.DeleteAsync<bool>(retrieveBasketUri, _headers, string.Empty)).Response;
        }

        public async Task<Basket> UpdateBasketOrderStatusAsync(string basketId, string status)
        {
            string processBasketUri = ConfigReader.Get(Constants.Environment.BasketMSOrderStatusUpdateUrl).Replace("{basket}", basketId);

            return (await _connector.SendAsync<Basket>(processBasketUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(new { Status = status }))).Response;
        }

        public async Task<Basket> UpdateSeatDetailsAsync(SeatUpdateRequest seatUpdateRequest)
        {
            string updateBasketSeatOfferUrl = ConfigReader.Get(Constants.Environment.BasketMSUpdateSeatOfferUrl).Replace("{basket}", seatUpdateRequest.BasketId);

            return (await _connector.SendAsync<Basket>(updateBasketSeatOfferUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(seatUpdateRequest.FlightPassengerSeatMap))).Response;
        }

        public async Task<Basket> UpdateCustomerProfileAsync(string basketId, BasketLoyaltyMemberDetails loyaltyMemberDetails)
        {
            string updateMembershipDetailsUrl = ConfigReader.Try<string>(Constants.Environment.BasketMSUpdateMembershipUrl).Replace("{basket}", basketId);

            return (await _connector.SendAsync<Basket>(updateMembershipDetailsUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(loyaltyMemberDetails))).Response;
        }

        public async Task<Basket> UpdatePointsCurrencyDetailsAsync(string basketId, IEnumerable<PtcWisePointsEquivalentCurrencyDetails> ptcWisePointsEquivalentCurrencyDetails)
        {
            string updatePointsCurrencyDetailsUrl = $"{ConfigReader.Try<string>(Common.Constants.Environment.BasketMSUrl)}{basketId}{"/pointsEquivalentCurrency"}";

            return (await _connector.SendAsync<Basket>(updatePointsCurrencyDetailsUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(ptcWisePointsEquivalentCurrencyDetails))).Response;
        }

        public async Task<Basket> UpdateFCEarnPointsAsync(string basketId, FCEarnPoints fCEarnPoints)
        {
            string updateFCEarnPointsUrl = $"{ConfigReader.Try<string>(Common.Constants.Environment.BasketMSUrl).TrimEnd('/')}/{basketId}{Common.Constants.Application.UpdateFCEarnPointsEndPoint}";

            return (await _connector.SendAsync<Basket>(updateFCEarnPointsUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(fCEarnPoints))).Response;
        }

        public async Task UpdateVoucherRedemptionAsync(string basketId, VoucherRedemptionUpdateRequest request)
        {
            var updateVoucherRedemptionUrl = $"{ConfigReader.Get(Constants.Environment.BasketMSUrl).TrimEnd('/')}/{basketId}/voucherredemption";

            await _connector.SendAsync<Basket>(updateVoucherRedemptionUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request));
        }

        public async Task<Basket> SaveInsuranceQuoteAsync(string basketId, IEnumerable<QuoteResponse> quoteResponse)
        {
            string saveInsuranceQuoteUrl = $"{ConfigReader.Try<string>(Common.Constants.Environment.BasketMSUrl).TrimEnd('/')}/{basketId}{Common.Constants.Application.SaveInsuranceQuoteEndPoint}";

            return (await _connector.SendAsync<Basket>(saveInsuranceQuoteUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(quoteResponse))).Response;
        }

        public async Task<Basket> UpdateInsuranceDecisionAsync(string basketId, bool insuranceAdded)
        {
            string updateInsuranceDecisionUrl = $"{ConfigReader.Try<string>(Common.Constants.Environment.BasketMSUrl).TrimEnd('/')}/{basketId}{Common.Constants.Application.UpdateInsuranceDecisionEndPoint}";

            return (await _connector.SendAsync<Basket>(updateInsuranceDecisionUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(new BasketInsuranceDecisionRequest() { InsuranceSelected = insuranceAdded }))).Response;
        }

        public async Task<Basket> SetCobrandProgress(string basketId, string cobrandProgress)
        {
            string setCobrandProgressUrl = $"{ConfigReader.Try<string>(Common.Constants.Environment.BasketMSUrl).TrimEnd('/')}/{basketId}{Common.Constants.Application.SetCobrandProgressUrl}";

            return (await _connector.SendAsync<Basket>(setCobrandProgressUrl, _headers, string.Empty, HttpMethod.Post, Json.Serialize(new BasketCoBrandStatusRequest() { Status = cobrandProgress }))).Response;
        }

        public async Task<Basket> UpdateFlightSummaryAsync(string basketId, string flightSummary, string summaryLink)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(flightSummary);

            var url = ConfigReader.Get(Constants.Environment.BasketMSSetFlightSummaryUrl).Replace("{basket}", basketId);
            return (await _connector.SendAsync<Basket>(url, _headers, string.Empty, HttpMethod.Post, Json.Serialize(new { FlightSummary = flightSummary, SummaryLink = summaryLink }))).Response;
        }

    }
}
