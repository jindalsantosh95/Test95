using System;
using System.Linq;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS
{
    public class TicketInfo
    {
        public string FFPCarrierCode { get; set; }
        public string FFPAccountNumber { get; set; }
        public string PassengerLastName { get; set; }
        public string PassengerFirstName { get; set; }
        public string FFPMemberDateOfBirth { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string ScheduledFlightSegmentDepartureDate { get; set; }
        public string FlightSegmentOrigin { get; set; }
        public string FlightSegmentDestination { get; set; }
        public string EticketNumber { get; set; }
        public string FFPMemberEmailAddress { get; set; }
        public string SeatNumber { get; set; }
        public string OperatingCarrierRecordLocator { get; set; }

        public static explicit operator TicketInfo((CustomerDTO customerDetails, SkyFlightsPointsClaimRequest flightDetails) input)
        {
            var custInfo = input.customerDetails;
            var flightInfo = input.flightDetails;

            return new TicketInfo()
            {
                FFPCarrierCode = Common.Constants.Application.PartnerAutoRetroFFPCarrierCode,
                FFPAccountNumber = custInfo.Customer.LoyaltyNumber,
                PassengerLastName = custInfo.Customer.LastName,
                PassengerFirstName = custInfo.Customer.FirstName,
                FFPMemberDateOfBirth = custInfo.Customer.DateOfBirth.ToString(Common.Constants.Application.ISODateFormat),
                FFPMemberEmailAddress = custInfo.Customer.ContactDetails?.EmailDetails?.First(x => x.PreferredFlag == "Y").emailId,
                OperatingFlightNumber = flightInfo.FlightNumber,
                OperatingCarrierCode = flightInfo.Airline,
                ScheduledFlightSegmentDepartureDate = Convert.ToDateTime(flightInfo.DepartureDate).Date.ToString(Common.Constants.Application.ISODateFormat).Replace("-", ""),
                FlightSegmentOrigin = flightInfo.DepartureFrom,
                FlightSegmentDestination = flightInfo.ArrivingAt,
                EticketNumber = flightInfo.TicketNumber,
                SeatNumber = flightInfo.SeatNumber,
                OperatingCarrierRecordLocator = flightInfo.PnrNumber

            };
        }
    }
}
