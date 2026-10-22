using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Passenger
    {
        public string AdultWithInfantPassengerId { get; set; }
        public string BirthDate { get; set; }
        public int CurrentAgeNum { get; set; }
        public string CustomerId { get; set; }
        public string EmailAdr { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameNum { get; set; }
        public string LastNameNum { get; set; }
        public int FirstTravelDateAgeNum { get; set; }
        public int LastTravelDateAgeNum { get; set; }
        public LoyaltyMember LoyaltyMember { get; set; }
        public string PassengerId { get; set; }
        public string PersistentCustomerId { get; set; }
        public string PassengerTypeCode { get; set; }
        public int? PassengerCnt { get; set; }
        public IEnumerable<string> AffiliationCodes { get; set; }
        public bool DominantPassenger { get; set; }
        public IEnumerable<string> DominantPassengerReasonCodes { get; set; }
    }
}
