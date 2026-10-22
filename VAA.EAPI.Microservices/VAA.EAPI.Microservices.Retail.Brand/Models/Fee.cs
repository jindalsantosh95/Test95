namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Fee
    {
        public double? Amount { get; set; }
        public string Currency { get; set; }
        public double? Percentage { get; set; }
        public string Higher_Lower { get; set; }

        public static explicit operator Fee(RouteHappy.Fee source)
        {
            if (source is null)
                return null;

            return new()
            {
                Amount = source.Amount,
                Currency = source.Currency,
                Percentage = source.Percentage,
                Higher_Lower = source.HigherLower?.ToString()
            };
        }

        /*
        public static explicit operator Fee(Infrastructure.ATPCO.Fee1 source)
        {
            if (source is null)
                return null;

            return new()
            {
                Amount = source.amount,
                Currency = source.currency,
                Percentage = source.percentage,
                Higher_Lower = source.higher_lower
            };
        }

        public static explicit operator Fee(Infrastructure.ATPCO.Fee2 source)
        {
            if (source is null)
                return null;

            return new()
            {
                Currency = source.currency,
                Amount = (decimal)source.amount,
            };
        }
        */
    }
}