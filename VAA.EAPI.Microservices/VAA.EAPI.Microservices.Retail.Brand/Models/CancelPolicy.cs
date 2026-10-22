namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class CancelPolicy : AmendmentPolicy
    {
        public static explicit operator CancelPolicy(RouteHappy.CancellationInner source)
        {
            return new()
            {
                Assessment = source.Assessment.ToString(),
                Description = source.Description,
                Headline = source.Headline,
                Fee = (Fee)source.Fee
            };
        }
    }
}