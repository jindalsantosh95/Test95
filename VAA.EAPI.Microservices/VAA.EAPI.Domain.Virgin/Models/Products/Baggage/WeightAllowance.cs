namespace VAA.EAPI.Domain.Virgin.Models.Products.Baggage
{
    public class WeightAllowance
    {
        public string MaximumWeightMeasure;

        public WeightUnitCodeType WeightUnitOfMeasurement;

        public static bool operator ==(WeightAllowance a, WeightAllowance b)
        {
            if (ReferenceEquals(a, b)) return true;

            if (a is null || b is null) return false;

            return a.MaximumWeightMeasure == b.MaximumWeightMeasure && a.WeightUnitOfMeasurement == b.WeightUnitOfMeasurement;
        }
        public static bool operator !=(WeightAllowance a, WeightAllowance b) => !(a == b);

        public static bool Equals(WeightAllowance a, WeightAllowance b) => a == b;
    }

    public enum WeightUnitCodeType
    {

        KGM,

        LBR,
    }
}
