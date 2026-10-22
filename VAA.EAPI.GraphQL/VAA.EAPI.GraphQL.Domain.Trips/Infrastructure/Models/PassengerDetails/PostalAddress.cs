namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class PostalAddress
    {
        public string Street { get; set; }
        public string CityName { get; set; }
        public CountrySubdivision State { get; set; }
        public string PostalCode { get; set; }
        public bool ApplyToAllPassengers { get; set; }
    }
}