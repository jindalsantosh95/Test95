using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("UpsellCabinClass")]
    public class CabinClass
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public IEnumerable<Amenity> Amenities { get; set; }

        public static explicit operator CabinClass((Infrastructure.Models.UpsellOffer.CabinClass source, IEnumerable<CabinRefData> cabinRefData) data)
        {
            var (source, cabinRefData) = data;
            if (source == null) return null;

            return new CabinClass
            {
                Code = source.Code,
                Name = source.Name,
                ProductCode = source.ProductCode,
                ProductName = cabinRefData?.FirstOrDefault(c => c.BrandID == source.ProductCode)?.BrandName ?? source.ProductName,
                Amenities = source.Amenities?.Select(a => (Amenity)a)
            };
        }
    }
}

