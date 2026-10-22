
namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class RateInfo
    {
        public decimal? Price { get; set; }
        public decimal? PriceWithTax { get; set; }
        public decimal? Tax { get; set; }
        public decimal? BasePrice { get; set; }
        public string CurrencyInContract { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal? TotalTaxBase { get; set; }

        internal RateInfo Clone()
        {
            return new RateInfo
            {
                Price = this.Price,
                PriceWithTax = this.PriceWithTax,
                Tax = this.Tax,
                BasePrice = this.BasePrice,
                CurrencyInContract = this.CurrencyInContract,
                TotalTax = this.TotalTax,
                TotalTaxBase = this.TotalTaxBase
            };
        }
    }
}
