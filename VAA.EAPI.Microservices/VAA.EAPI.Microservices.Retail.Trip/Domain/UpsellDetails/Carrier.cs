namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class Carrier
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator Carrier(Infrastructure.Proxy.DeltaUpsellOffers.Carrier source)
        {
            if (source == null) return null;

            return new Carrier
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}

