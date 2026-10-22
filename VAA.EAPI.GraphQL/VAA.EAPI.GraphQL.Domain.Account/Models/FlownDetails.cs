using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class FlownDetails
    {
        public string OriginalBookedClass { get; set; }
        public string FlownUpgradedClass { get; set; }
        public string OperatingAirlineCode { get; set; }
        public DateTime DepartureDate { get; set; }
        public string FareBasis { get; set; }
        public string TicketDesignator { get; set; }
        public string PassengerList { get; set; }
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }

        public static explicit operator FlownDetails(VSRetroFlownDetail pdmResponse)
        {
            return new FlownDetails()
            {
                OriginalBookedClass = pdmResponse.OriginalBookedClass,
                FlownUpgradedClass = pdmResponse.FlownCabinClass,
                OperatingAirlineCode = pdmResponse.OperatingAirlineCode,
                DepartureDate = pdmResponse.DepartureDate,
                FareBasis = pdmResponse.FareBasis,
                TicketDesignator = pdmResponse.TicketDesignator,
                PassengerList = pdmResponse.PassengerType,
                OriginCountryCode = pdmResponse.DepartureAirport,
                DestinationCountryCode = pdmResponse.ArrivalAirport
            };
        }
    }

}
