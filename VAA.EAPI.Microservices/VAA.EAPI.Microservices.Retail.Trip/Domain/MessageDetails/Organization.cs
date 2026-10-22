
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class Organization
    {
        public string Id { get; set; }

        public static explicit operator Organization(Infrastructure.Proxy.DeltaMessageCentre.Organization source)
        {
            if (source == null)
                return null;

            return new Organization()
            {
                Id = source.Id
            };
        }
    }
}
