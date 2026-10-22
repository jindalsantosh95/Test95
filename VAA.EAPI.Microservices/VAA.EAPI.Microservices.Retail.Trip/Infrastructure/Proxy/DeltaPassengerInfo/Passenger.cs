
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
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
        public string BirthDt { get; set; }
        public string Gender { get; set; }
        public PassengerType PassengerType { get; set; }
        public Passenger Infant { get; set; }
        public IEnumerable<LoyaltyProgramAccount> LoyaltyProgramAccounts { get; set; }
        public IEnumerable<Affiliation> Affiliations { get; set; }
        public AdvancePassengerInformation AdvancePassengerInformation { get; set; }
        public ActionCodeType ActionType { get; set; }
        public bool SfpdExists { get; set; }

        public static explicit operator Passenger(Domain.PassengerDetails.Passenger source)
        {
            if (source is null) return null;

            return new Passenger()
            {
                PassengerResourceId = source.PassengerResourceId,
                Title = (TitleType)source.Title,
                GivenNames = source.GivenNames,
                MiddleName = source.MiddleName,
                Surname = source.Surname,
                Suffix = source.Suffix,
                FirstNameNum = source.FirstNameNum,
                LastNameNum = source.LastNameNum,
                CustomerId = source.CustomerId,
                BirthDt = source.BirthDate,
                Gender = source.Gender,
                PassengerType = (PassengerType)source.PassengerType,
                Infant = (Passenger)source.Infant,
                LoyaltyProgramAccounts = source.LoyaltyProgramAccounts?.Select(x => (LoyaltyProgramAccount)x),
                Affiliations = source.Affiliations?.Select(x => (Affiliation)x),
                AdvancePassengerInformation = (AdvancePassengerInformation)source.AdvancePassengerInformation,
                ActionType = (ActionCodeType)source.ActionType,
                SfpdExists = source.SfpdExists
            };


        }
    }
}