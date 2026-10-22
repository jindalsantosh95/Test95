namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    [GraphQLName("DimensionAllowance_BFF")]
    public class DimensionAllowance
    {
        public string? Height { get; set; }
        public string? Width { get; set; }
        public string? Linear { get; set; }
        public string? Length { get; set; }
        public string? UnitOfMeasure { get; set; }

        public static explicit operator DimensionAllowance(FlightOffer.DimensionAllowance source)
        {
            if (source is null)
                return null;

            return new()
            {
                Height = source.HeightMeasure,
                Length = source.LengthMeasure,
                Linear = source.LinearMeasure,
                Width = source.WidthMeasure,
                UnitOfMeasure = source.LengthUnitOfMeasurement.ToString(),
            };
        }

        public static explicit operator DimensionAllowance(Brand.BagSize? source)
        {
            if (source?.Size?.Metric is null)
                return null;

            return new DimensionAllowance
            {
                UnitOfMeasure = FlightOffer.LengthUnitCodeType.CM.ToString(),
                Linear = source.Size?.Metric.ToString(),
                Height = source.Size?.Metric.ToString(),
                Length = source.Size?.Metric.ToString(),
                Width = source.Size?.Metric.ToString()
            };
        }
    }
}
