using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsAddress")]
    public class Address
    {
        public string? AddressType { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? PreferredFlag { get; set; }

        internal Address Clone()
        {
            return new Address
            {
                AddressType = this.AddressType,
                AddressLine1 = this.AddressLine1,
                AddressLine2 = this.AddressLine2,
                AddressLine3 = this.AddressLine3,
                City = this.City,
                PostalCode = this.PostalCode,
                Country = this.Country,
                PreferredFlag = this.PreferredFlag
            };
        }

        public static explicit operator Address(Infrastructure.Models.TripMS.Address source)
        {
            if (source == null)
                return null;

            return new Address()
            {
                AddressLine1 = source.AddressLine1,
                AddressLine2 = source.AddressLine2,
                AddressLine3 = source.AddressLine3,
                City = source.City,
                PostalCode = source.PostalCode,
                Country = source.Country,
                PreferredFlag = source.PreferredFlag,
                AddressType = source.AddressType,
            };
        }
    }
}
