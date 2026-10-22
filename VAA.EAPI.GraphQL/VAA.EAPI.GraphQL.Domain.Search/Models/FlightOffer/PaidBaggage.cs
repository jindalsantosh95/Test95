using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class PaidBaggage
    {
        public Weight Weight { get; set; }
        public IEnumerable<DimensionAllowance>? DimensionAllowance { get; set; }
        public IEnumerable<WeightAllowance>? WeightAllowance { get; set; }
        public Fee? Price { get; set; }

        public static explicit operator PaidBaggage(PaidBag source)
        {
            if (source is null)
                return null;

            return new()
            {
                Price = source.Fee,
                DimensionAllowance = source.Size is null ? null : new DimensionAllowance[] {
                    (DimensionAllowance)source.Size
                },
                WeightAllowance = source.Weight is null ? null : new WeightAllowance[] {
                    (WeightAllowance)source.Weight
                }
            };
        }
    }
}
