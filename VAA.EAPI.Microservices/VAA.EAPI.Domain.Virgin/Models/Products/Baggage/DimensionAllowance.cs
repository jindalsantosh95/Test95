namespace VAA.EAPI.Domain.Virgin.Models.Products.Baggage
{
    public class DimensionAllowance
    {
        public string HeightMeasure { get; set; }

        public string WidthMeasure { get; set; }

        public string LinearMeasure { get; set; }

        public string LengthMeasure { get; set; }

        public LengthUnitCodeType LengthUnitOfMeasurement { get; set; }

        public static bool operator ==(DimensionAllowance a, DimensionAllowance b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;

            return a.LinearMeasure == b.LinearMeasure &&
                   a.LengthMeasure == b.LengthMeasure &&
                   a.HeightMeasure == b.HeightMeasure &&
                   a.WidthMeasure == b.WidthMeasure &&
                   a.LengthUnitOfMeasurement == b.LengthUnitOfMeasurement;
        }

        public static bool operator !=(DimensionAllowance a, DimensionAllowance b) => !(a == b);

        public static bool Equals(DimensionAllowance a, DimensionAllowance b) => a == b;
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
