using System;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Search.Models.OfferMS;
/**
* The flight search criteria slice
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class Criteria
    {

        public Airport origin { get; set; }

        public Airport destination { get; set; }

        public DateOnly departing { get; set; }

        public static explicit operator Criteria(OriginDestinationAssociation source)
        {
            return new()
            {
                origin = new()
                {
                    Code = source.OriginDestination.OriginAirport,
                    CityName = source.OriginDestination.OriginCityName,
                    CountryName = source.OriginDestination.OriginCountryName,
                    AirportName = source.OriginDestination.OriginAirportName
                },
                destination = new()
                {
                    Code = source.OriginDestination.DestinationAirport,
                    CityName = source.OriginDestination.DestinationCityName,
                    CountryName = source.OriginDestination.DestinationCountryName,
                    AirportName = source.OriginDestination.DestinationAirportName
                },
                departing = source.OriginDestination.DepartureDate.Parse<DateOnly>()
            };
        }

    }
}
