using System;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public sealed class Person
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CustomerId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public bool ProfilePassenger { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
        public string Type { get; set; }
        public int Age { get; set; }
        public FQTMembership MembershipDetails { get; set; } = new();
        public BookingClass? BookingClass { get; set; }
    }
}