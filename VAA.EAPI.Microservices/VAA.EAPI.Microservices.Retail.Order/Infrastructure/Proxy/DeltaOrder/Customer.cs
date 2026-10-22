using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class Customer
    {
        public string CustomerId { get; set; }
        public string BirthDate { get; set; }
        public string EmailAdr { get; set; }
        public string GenderCode { get; set; }
        public string PassengerId { get; set; }
        public string PassengerTypeCode { get; set; }
        public PersonName PersonName { get; set; }
        public List<Phone> Phones { get; set; }
        public string AdultWithInfantPassengerId { get; set; }
        public LoyaltyMember LoyaltyMember { get; set; }

    }
}
