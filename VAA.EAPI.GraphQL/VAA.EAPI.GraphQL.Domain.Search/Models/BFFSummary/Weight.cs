using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    [GraphQLName("Weight_BFF")]
    public class Weight
    {
        public int UnitOfMeasureCnt { get; set; }
        public string? UnitOfMeasure { get; set; }

        public static explicit operator Weight(FlightOffer.WeightAllowance source)
        {
            if (source is null)
                return null;

            return new()
            {
                UnitOfMeasure = source.WeightUnitOfMeasurement.ToString(),
                UnitOfMeasureCnt = source.MaximumWeightMeasure.Parse<int>()
            };
        }

        public static explicit operator Weight(Brand.BagWeight? source)
        {
            if (source?.Weight?.Metric is null)
                return null;

            return new()
            {
                UnitOfMeasure = FlightOffer.WeightUnitCodeType.KG.ToString(),
                UnitOfMeasureCnt = source.Weight.Metric.Value
            };
        }
    }
}