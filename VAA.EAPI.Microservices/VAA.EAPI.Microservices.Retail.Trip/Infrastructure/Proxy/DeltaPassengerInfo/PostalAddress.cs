
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class PostalAddress
    {
        public string Street { get; set; }
        public string CityName { get; set; }
        public CountrySubdivision State { get; set; }
        public string PostalCode { get; set; }
        public bool ApplyToAllPassengers { get; set; }

        public static explicit operator PostalAddress(Domain.PassengerDetails.PostalAddress source)
        {
            if (source is null) return null;

            return new PostalAddress()
            {
                Street = source.Street,
                CityName = source.CityName,
                State = (CountrySubdivision)source.State,
                PostalCode = source.PostalCode,
                ApplyToAllPassengers = source.ApplyToAllPassengers
            };
        }
    }
}