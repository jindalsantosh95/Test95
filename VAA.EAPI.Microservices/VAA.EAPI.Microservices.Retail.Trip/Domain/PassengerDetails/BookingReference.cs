using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class BookingReference
    {
        public string Id { get; set; }

        public static explicit operator BookingReference(DeltaPaxInfo.BookingReference source)
        {
            if (source is null) return null;

            return new BookingReference()
            {
                Id = source.Id
            };
        }
    }
}