using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails
{
    public class Passenger
    {
        public string? CustomerId { get; set; }
        public string? NameNumber { get; set; }
        public string? PassengerId { get; set; }
        public string? AssociatedInfantId { get; set; }
        public LoyaltyMembership? LoyaltyMembership { get; set; }
        public PassengerType? PassengerType { get; set; }
        public string? Title { get; set; }
        public string? GivenNames { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Email { get; set; }
        public IEnumerable<Slice>? Slices { get; set; }
        public string? FirstName { get; set; }
        public string? FirstNameNumber { get; set; }
        public string? LastNameNumber { get; set; }

        public static explicit operator Passenger(CustomerDetail customerDetail)
        {
            return new()
            {
                CustomerId = customerDetail?.CustID,
                FirstName = customerDetail?.FirstName,
                FirstNameNumber = customerDetail?.FirstNameNumber,
                LastName = customerDetail?.LastName,
                LastNameNumber = customerDetail?.LastNameNumber,
                Gender = customerDetail?.GenderOnDocument,
                PassengerType = new() { Code = customerDetail?.PTC },
                LoyaltyMembership = new() { Number = customerDetail?.FrequentFlyer?.AccountNumber, ProgrammeCode = customerDetail?.FrequentFlyer?.AirlineCode, TierCode = customerDetail?.FrequentFlyer?.TierCode }
            };
        }
    }
}