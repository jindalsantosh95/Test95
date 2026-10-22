namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class LoyaltyProgram
    {
        public string Name { get; set; }
        public IEnumerable<Carrier> Owner { get; set; }

        public static explicit operator LoyaltyProgram(Infrastructure.Proxy.DeltaUpsellOffers.LoyaltyProgram source)
        {
            if (source == null) return null;

            return new LoyaltyProgram
            {
                Name = source.Name,
                Owner = source.Owner?.Select(carrier => (Carrier)carrier)
            };
        }
    }
}

