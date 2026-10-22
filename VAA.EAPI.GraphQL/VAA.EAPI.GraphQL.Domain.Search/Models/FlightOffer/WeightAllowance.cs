using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class WeightAllowance
    {
        public string MaximumWeightMeasure { get; set; }
        public WeightUnitCodeType WeightUnitOfMeasurement { get; set; }

        public static explicit operator WeightAllowance(BagWeight source)
        {
            if (source is null)
                return null;

            return new()
            {
                MaximumWeightMeasure = source.Weight.Metric.ToString(),
                WeightUnitOfMeasurement = WeightUnitCodeType.KG
            };
        }
    }

    public enum WeightUnitCodeType
    {
        KG,
        LB,
    }
}
