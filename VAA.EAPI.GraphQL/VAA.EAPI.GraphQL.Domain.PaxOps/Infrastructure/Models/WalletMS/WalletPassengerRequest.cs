using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.WalletMS
{
    public class WalletPassengerRequest
    {
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string PassengerFirstNIN { get; set; }
        public string PassengerLastNIN { get; set; }

        public static explicit operator WalletPassengerRequest(Passenger passenger)
        {
            return new WalletPassengerRequest
            {
                GivenNames = passenger?.FirstName,
                Surname = passenger?.LastName,
                PassengerFirstNIN = passenger?.FirstNameNumber,
                PassengerLastNIN = passenger?.LastNameNumber
            };
        }
    }
}
