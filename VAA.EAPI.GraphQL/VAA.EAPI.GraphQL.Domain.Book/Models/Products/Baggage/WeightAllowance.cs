namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Baggage
{
    public class WeightAllowance
    {
        public string MaximumWeightMeasure;

        public WeightUnitCodeType WeightUnitOfMeasurement;
    }

    public enum WeightUnitCodeType
    {

        KGM,

        LBR,
    }
}
