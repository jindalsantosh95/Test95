
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class BookingReference
    {
        public string Id { get; set; }
        public Organization Organization { get; set; }

        public static explicit operator BookingReference(Infrastructure.Proxy.DeltaMessageCentre.BookingReference source)
        {
            if (source == null)
                return null;

            return new BookingReference()
            {
                Id = source.Id,
                Organization = (Organization)source.Organization
            };
        }
    }
}
