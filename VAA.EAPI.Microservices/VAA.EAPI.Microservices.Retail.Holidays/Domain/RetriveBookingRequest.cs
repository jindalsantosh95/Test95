using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain
{
    public sealed class RetrieveBookingRequest
    {
        public string BookingReference { get; set; }
        public string Surname { get; set; }
        public DateOnly DateOfBirth { get; set; }
    }
}
