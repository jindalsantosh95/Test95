using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public sealed class HolidayRequest
    {
        public string BookingReference { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Surname { get; set; }
    }
}