using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class DimensionAllowance
    {
        public string HeightMeasure { get; set; }
        public string WidthMeasure { get; set; }
        public string LinearMeasure { get; set; }
        public string LengthMeasure { get; set; }
        public LengthUnitCodeType LengthUnitOfMeasurement { get; set; }

        public static explicit operator DimensionAllowance(BagSize source)
        {
            if (source is null)
                return null;

            return new()
            {
                HeightMeasure = source.Size.Metric.ToString(),
                LengthMeasure = source.Size.Metric.ToString(),
                LinearMeasure = source.Size.Metric.ToString(),
                WidthMeasure = source.Size.Metric.ToString(),
                LengthUnitOfMeasurement = LengthUnitCodeType.CM
            };
        }
    }

    public enum LengthUnitCodeType
    {
        /// <remarks/>
        CM,
        /// <remarks/>
        FT,
        /// <remarks/>
        IN,
        /// <remarks/>
        MT,
    }
}
