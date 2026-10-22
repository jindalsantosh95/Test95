namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class Passenger
    {
        public string PassengerResourceId { get; set; }
        public TitleType Title { get; set; }
        public string GivenNames { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string Suffix { get; set; }
        public string FirstNameNum { get; set; }
        public string LastNameNum { get; set; }
        public string CustomerId { get; set; }
        public string BirthDate { get; set; }
        public string Gender { get; set; }
        public PassengerType PassengerType { get; set; }
        public Passenger Infant { get; set; }
        public IEnumerable<LoyaltyProgramAccount> LoyaltyProgramAccounts { get; set; }
        public IEnumerable<Affiliation> Affiliations { get; set; }
        public AdvancePassengerInformation AdvancePassengerInformation { get; set; }
        public ActionCodeType ActionType { get; set; }
        public bool SfpdExists { get; set; }
    }
}