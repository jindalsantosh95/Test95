using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class Passenger
    {
        public string CustomerId { get; set; }
        public string FirstNameNum { get; set; }
        public string LastNameNum { get; set; }
        public string PassengerId { get; set; }
        public string PassengerTypeCode { get; set; }
        public PersonName PersonName { get; set; }
        public List<Phone> Phones { get; set; }
    }
}
