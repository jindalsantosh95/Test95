using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Mapper
{
    public class DbaAccrualMapper
    {
        public static DistanceBasedAccrualPayload Map(AutoAccrualFlownDetails request)
        {
            return new DistanceBasedAccrualPayload
            {


                Producer = request.OperatingAirlineCode,
                Consumer = request.FFPCarrierCode,
                SchemaId = Constants.Application.AccrualDbaSchemaId,
                SchemaVersion = Constants.Application.AccrualDbaSchemaVersion,
                RequestType = Constants.Application.AccrualDbaRequestType,
                AccrualType = Constants.Application.AccrualDbaAccrualType,

                UniqueIdentification = new UniqueIdentification
                {
                    UniqueIdentificationNumber = request.OperatingAirlineCode + request.OperatingAirAuthNumber,
                    AirlineCode = request.OperatingAirlineCode,
                },

                PassengerName = new NameDetails
                {
                    GivenName = request.FirstName,
                    SurName = request.LastName
                },

                FfpDetails = new FfpDetails
                {
                    FfpCarrierCode = request.FFPCarrierCode,
                    FfpAccountNumber = request.FFPAccountNumber
                },

                FlightDetails = new FlightDetails
                {
                    MarketingFlight = new MarketingFlight
                    {
                        MarketingCarrierCode = request.MarketingAirlineCode,
                        MarketingFlightNumber = request.MarketingFlightNum
                    },

                    OperatingFlight = new OperatingFlight
                    {
                        OperatingCarrierCode = request.OperatingAirlineCode,
                        OperatingFlightNumber = request.OperatingFlightNum
                    }
                },

                ScheduledDepartureDate = request.DepartureDate,

                FlightTypeIndicator = "N",

                FlightSegment = new FlightSegment
                {
                    Origin = request.OriginPort,
                    Destination = request.DestinationPort
                },

                BookingDetails = new BookingClassDetails
                {
                    CabinClassCode = request.FlownCabinClass,

                    OperatingCarrierBookingClass = request.OperatingCarrierBookingClass,

                    OperatingCarrierFareType = request.OperatingCarrierBookingClass,

                    MarketingCarrierBookingClass = request.MarketingCarrierBookingClass,

                    MarketingCarrierFareType = request.MarketingCarrierBookingClass
                },

                DocumentDetails = new DocumentDetails
                {
                    DocumentNumber = request.TicketNumber,

                    DocumentCouponNumber = int.TryParse(request.CouponNumber, out var couponNumber) ? couponNumber : null
                },

                AdditionalPassengerInformation = request.SeatNumber,

                CreditCode = null,

                ProductCode = Constants.Application.AccrualDbaProductCode,

                AdditionalInformation = Constants.Application.AccrualDbaAdditionalInformation
            };
        }
    }
}
