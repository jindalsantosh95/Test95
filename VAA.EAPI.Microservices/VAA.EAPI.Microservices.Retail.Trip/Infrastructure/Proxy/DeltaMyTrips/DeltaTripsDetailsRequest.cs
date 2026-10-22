using VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails;
using VAA.EAPI.Microservices.Retail.Trip.Mapper;
using System.Text.RegularExpressions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class DeltaTripsDetailsRequest
    {
        public RequestOptionCodeType Using { get; set; }
        public string ConfirmationNum { get; set; }
        public string EncryptedConfirmationNum { get; set; }
        public string TicketNum { get; set; }
        public string CreditCardNum { get; set; }
        public string LoyaltyNum { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string DepartureAirportCode { get; set; }

        public static explicit operator DeltaTripsDetailsRequest(TripsDetailsRequest source)
        {
            if (source == null)
            {
                return null;
            }

            return new DeltaTripsDetailsRequest
            {
                ConfirmationNum = source.ConfirmationNum,
                GivenNames = RegexCompiled.Replace(source.GivenNames ?? string.Empty, "['&]", string.Empty),
                Surname = RegexCompiled.Replace(source.Surname ?? string.Empty, "['&]", string.Empty),
                TicketNum = source.TicketNum,
                Using = EnumMapper.MapToDLRequestOptionCodeType(source.Using),
                DepartureAirportCode = source.DepartureAirportCode
            };
        }
    }
}