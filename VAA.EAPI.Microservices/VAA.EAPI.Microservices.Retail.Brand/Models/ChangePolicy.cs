namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class ChangePolicy : AmendmentPolicy
    {
        public static explicit operator ChangePolicy(RouteHappy.AdvanceChangeInner source)
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