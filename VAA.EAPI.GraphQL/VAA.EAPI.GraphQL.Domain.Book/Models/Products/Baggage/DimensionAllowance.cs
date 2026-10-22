namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Baggage
{
    public class DimensionAllowance
    {
        public string HeightMeasure { get; set; }

        public string WidthMeasure { get; set; }

        public string LinearMeasure { get; set; }

        public string LengthMeasure { get; set; }

        public LengthUnitCodeType LengthUnitOfMeasurement { get; set; }
    }

    public enum LengthUnitCodeType
    {

        /// <remarks/>
        CMT,

        /// <remarks/>
        FOT,

        /// <remarks/>
        INH,

        /// <remarks/>
        MTR,
    }
}
