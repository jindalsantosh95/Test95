using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Mapper
{
    public class ResponseMapper
    {
        public static CurrencyData MapCurrencyDetails(CurrencyDbEntity currencyDbEntity)
        {
            if (currencyDbEntity is null) return null;

            return new CurrencyData()
            {
                Country = currencyDbEntity.Country,
                CountryCode = currencyDbEntity.CountryCode,
                Currency = currencyDbEntity.Currency,
                LocalCurrency = currencyDbEntity.LocalCurrency,
                OverrideCurrency = currencyDbEntity.OverrideCurrency,
                PointOfSale = currencyDbEntity.POS,
                OverridePointOfSale = currencyDbEntity.OverridePos
            };
        }

        public static IEnumerable<UpsellOffersData> MapUpsellOffers(IEnumerable<UpsellOffersDbEntity> upsellOfferDbEntityList)
        {
            return upsellOfferDbEntityList?.Select(x => new UpsellOffersData()
            {
                ChosenBrand = x.ChosenBrand,
                EligibleUpsell = (x.EligibleUpsell != null) ? Json.Deserialize<EligibleUpsellOffer[]>(x.EligibleUpsell) : null
            });
        }

        public static PricingLocation MapPricingLocation(List<AirportListDBEntity> airportDetails, List<LocationMapDbEntity> locationDetails)
        {
            return new PricingLocation
            {
                AirportListDetails = airportDetails.Select(x => (AirportList)x),
                LocationMappingListDetails = locationDetails.Select(x => (LocationMapping)x)
            };
        }
    }

}
