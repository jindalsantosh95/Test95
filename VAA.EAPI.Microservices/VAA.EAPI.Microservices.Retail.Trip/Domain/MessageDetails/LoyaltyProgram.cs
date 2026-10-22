
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class LoyaltyProgram
    {
        public string Name { get; set; }
        public IEnumerable<Carrier> Owner { get; set; }

        public static explicit operator LoyaltyProgram(Infrastructure.Proxy.DeltaMessageCentre.LoyaltyProgram source)
        {
            if (source == null)
                return null;

            return new LoyaltyProgram()
            {
                Name = source.Name
            };
        }
    }
}
