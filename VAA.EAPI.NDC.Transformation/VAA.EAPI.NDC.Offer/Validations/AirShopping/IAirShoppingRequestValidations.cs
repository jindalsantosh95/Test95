using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.Currency;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Offer.Validations.AirShopping
{
    public interface IAirShoppingRequestValidations<T>
    {
        Response<T> ValidateAirShoppingRequest(IATA_AirShoppingRQ airShoppingRQ);
        Response<T> ValidateAgentInAirShoppingRequest(IATA_AirShoppingRQ airShoppingRQ);
        Response<T> ValidateAgency(ValidationRS validationRS);
        Response<T> ValidateAccountCode(ValidationRS validationRS, IATA_AirShoppingRQ airShoppingRQ);
        Response<T> ValidatePaxBasedOnFareType(PaxType[] paxTypes, IEnumerable<ItaFareTypeCodeData> matchedPTCCode);
        IEnumerable<NDCWarning> ValidateCurrency(IATA_AirShoppingRQ airShoppingRQ, OfferResponse offers, BspMarketCurrencyData currencyData, ValidationRS validationRS, CalendarSearchResponse calendarSearchResponse);
    }
}
