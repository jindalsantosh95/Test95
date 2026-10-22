using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Offer.Validations.AirShopping
{
    public class AirShoppingRequestValidations<T> : IAirShoppingRequestValidations<T>
    {
        private readonly IHostEnvironment _hostEnvironment;

        public AirShoppingRequestValidations(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public Response<T> ValidateAirShoppingRequest(IATA_AirShoppingRQ airShoppingRQ)
        {
            var errorIds = new List<ErrorDetail>();
            var flightOriginDestCriteria = (FlightRequestOriginDestinationsCriteriaType)airShoppingRQ?.Request?.FlightRequest?.Item;

            errorIds.AddRange(RequestValidations.ValidateCabinCodes(flightOriginDestCriteria?.OriginDestCriteria?.First().CabinType));
            errorIds.AddRange(RequestValidations.ValidateTaxonomyCode(airShoppingRQ?.Request?.OfferCriteria?.ServiceCriteria));
            errorIds.AddRange(RequestValidations.ValidateRequestFareType(airShoppingRQ?.Request));
            errorIds.AddRange(RequestValidations.ValidateFareTypeCode(airShoppingRQ?.Request?.OfferCriteria?.FareCriteria));
            errorIds.AddRange(RequestValidations.ValidatePaxList(airShoppingRQ?.Request?.PaxList));
            errorIds.AddRange(RequestValidations.ValidateOriginDestinations(flightOriginDestCriteria?.OriginDestCriteria));
            errorIds.AddRange(RequestValidations.ValidateCalendarSearchRQ(airShoppingRQ));

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public Response<T> ValidateAgentInAirShoppingRequest(IATA_AirShoppingRQ airShoppingRQ)
        {
            var errorIds = RequestValidations.ValidateRequestAgentDetails(airShoppingRQ.DistributionChain).ToList();

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public Response<T> ValidateAgency(ValidationRS validationRS)
        {
            var errorIds = RequestValidations.ValidateAgencyDetails(validationRS).ToList();

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public IEnumerable<NDCWarning> ValidateCurrency(IATA_AirShoppingRQ airShoppingRQ, OfferResponse offers, BspMarketCurrencyData currencyData, ValidationRS validationRS, CalendarSearchResponse calendarSearchResponse)
        {
            var warningIds = RequestValidations.ValidateCurrencyDetails(airShoppingRQ, offers, currencyData, validationRS, calendarSearchResponse).ToList();

            return ErrorHelper<NDCWarning>.GetWarnings(warningIds, _hostEnvironment);
        }

        public Response<T> ValidateAccountCode(ValidationRS validationRS, IATA_AirShoppingRQ airShoppingRQ)
        {
            var errorIds = RequestValidations.ValidateAccountCodeDeatils(validationRS, airShoppingRQ);

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public Response<T> ValidatePaxBasedOnFareType(PaxType[] paxTypes, IEnumerable<ItaFareTypeCodeData> matchedPTCCode)
        {
            var errorIds = RequestValidations.ValidatePaxType(paxTypes, matchedPTCCode);

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }
    }
}
