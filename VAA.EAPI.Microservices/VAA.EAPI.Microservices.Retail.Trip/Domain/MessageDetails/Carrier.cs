
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class Carrier
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator Carrier(Infrastructure.Proxy.DeltaMessageCentre.Carrier source)
        {
            if (source == null)
                return null;

            return new Carrier
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}
