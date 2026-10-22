
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class LoyaltyProgram
    {
        public string Name { get; set; }
        public IEnumerable<Carrier> Owner { get; set; }

        public static explicit operator LoyaltyProgram(Domain.PassengerDetails.LoyaltyProgram source)
        {
            if (source is null) return null;

            return new LoyaltyProgram()
            {
                Name = source.Name,
                Owner = source.Owner?.Select(x => (Carrier)x)
            };

        }
    }
}