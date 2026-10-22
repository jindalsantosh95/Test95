
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class Passenger
    {
        public string Id { get; set; }
        public TitleType Title { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string Suffix { get; set; }
        public Passenger Infant { get; set; }
        public IEnumerable<LoyaltyProgramAccount> LoyaltyProgramAccounts { get; set; }
        public IEnumerable<Affiliation> Affiliations { get; set; }

        public static explicit operator Passenger(Infrastructure.Proxy.DeltaMessageCentre.Passenger source)
        {
            if (source == null)
                return null;

            return new Passenger
            {
                Id = source.Id,
                Title = (TitleType)source.Title,
                GivenNames = source.GivenNames,
                Surname = source.Surname,
                Suffix = source.Suffix,
                Infant = (Passenger)source.Infant,
                LoyaltyProgramAccounts = source.LoyaltyProgramAccounts?.Select(l => (LoyaltyProgramAccount)l),
                Affiliations = source.Affiliations?.Select(a => (Affiliation)a)
            };
        }
    }
}
